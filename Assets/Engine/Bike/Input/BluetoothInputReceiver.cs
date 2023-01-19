using System;
using System.Collections.Generic;
using UnityEngine;
using Engine;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;

/// <summary>
/// Class that has the sole purpose of connecting to bluetooth and listening for data.
/// To do that, it continuously communicates with Android Native Classes. A thread 
/// is opened only for the listening part. Also has a mechanismus that checks for 
/// timeout. In that case, the connection will be rejected again and the devices will
/// need to pair and connect again.
/// </summary>
public class BluetoothInputReceiver : MonoBehaviour, Engine.Bike.Physics.IBikeInput
{
    /// <summary>
    /// Struct that contains all the information for the bike
    /// </summary>
    private struct VRBikeData
    {
        public int RPM { get; private set; }
        public int Steer { get; private set; }
        public int Error { get; private set; }

        public VRBikeData(int rpm, int steer, int error)
        {
            RPM = rpm;
            Steer = steer;
            Error = error;
        }
    }

    /// <summary>
    /// The state of the listener thread
    /// </summary>
    private enum ListenerState { Connected, Disconnected }


    /// <summary>
    /// The timeout of listening. If the data is delayed more than this,
    /// we will be disconnected. The time is in seconds
    /// </summary>
    public int ListenTimeout { get { return listenTimeout; } }
    [SerializeField, Tooltip("How long should we wait for, if no data is available, to break the communication completely?")]
    private int listenTimeout;

    /// <summary>
    /// The native Android bluetooth handler
    /// </summary>
    private AndroidJavaObject bluetoothHandler;

    /// <summary>
    /// The native bluetooth server connector
    /// </summary>
    private AndroidJavaObject serverConnector;

    /// <summary>
    /// This GO Name
    /// </summary>
    private string goName;

    /// <summary>
    /// The listener state
    /// </summary>
    private ListenerState listenerState;

    /// <summary>
    /// The timestamp since no connection between server & app
    /// </summary>
    private int noConnectionTimestamp;

    /// <summary>
    /// The thread that is responsible for reading data from the bike
    /// </summary>
    private Thread listenerThread;

    /// <summary>
    /// The thread to check if there was a timeout
    /// </summary>
    private Thread timeoutCheckThread;

    /// <summary>
    /// Time Manager
    /// </summary>
    private Stopwatch timeManager;

    /// <summary>
    /// The queued data that has come, and is not yet processed
    /// </summary>
    private Queue<VRBikeData> queuedData;

    /// <summary>
    /// The last data that was fetched
    /// </summary>
    private VRBikeData lastData;


    //Start function
    private void Start()
    {
        //Init data
        bluetoothHandler = new AndroidJavaObject("bluetoothutility.vrbike.com.main.BluetoothHandler");
        serverConnector = new AndroidJavaObject("bluetoothutility.vrbike.com.main.UnityBluetoothConnection", bluetoothHandler);
        queuedData = new Queue<VRBikeData>();
        goName = gameObject.name;
        timeManager = new System.Diagnostics.Stopwatch();
        timeManager.Start();
        noConnectionTimestamp = -1;
        lastData = new VRBikeData(0, 0, 0);

        //Start a new Thread that will listen for data every frame
        listenerState = ListenerState.Disconnected;
        OnDisconnected();
        listenerThread = new Thread(ReadData);
        listenerThread.Start();

        //Start a new Thread that will listen for timeouts
        timeoutCheckThread = new Thread(TimeoutCheck);
        timeoutCheckThread.Start();
    }

    #region Connection/Disconnection

    /// <summary>
    /// Checks for timeout in connection
    /// </summary>
    private void TimeoutCheck()
    {
        AndroidJNI.AttachCurrentThread();

        while (true)
        {
            //If we are disconnected, don't check
            if (listenerState != ListenerState.Connected)
                continue;

            //If the queue is empty and the counter hasn't started, start it now
            if (queuedData.Count == 0 && noConnectionTimestamp == -1)
            {
                noConnectionTimestamp = (int)timeManager.Elapsed.TotalSeconds;
            }
            //If the queue is empty, the counter has started and the timeout has exceeded, we are disconnected
            else if (queuedData.Count == 0 && noConnectionTimestamp != -1f && (int)timeManager.Elapsed.TotalSeconds - noConnectionTimestamp > ListenTimeout)
            {
                noConnectionTimestamp = -1;
                OnDisconnected();
            }
            //If the queue is not empty, reset the timer
            else if (queuedData.Count != 0f)
            {
                noConnectionTimestamp = -1;
            }
        }
    }

    /// <summary>
    /// Called when we are disconnected from the VRBikeServer.
    /// </summary>
    private void OnDisconnected()
    {
        MobileTools.Log("Disconnected");
        listenerState = ListenerState.Disconnected;

        //Init connection process
        serverConnector.Call<bool>("Start", goName, "OnConnected");
    }

    /// <summary>
    /// Called when we are connected to VRBike Server. This will be called
    /// by the Android Plugin
    /// natively.
    /// </summary>
    private void OnConnected()
    {
        MobileTools.Log("Connected");
        listenerState = ListenerState.Connected;
    }

    #endregion

    #region Listener Thread

    /// <summary>
    /// Reads the data every frame. Executed in the extra thread
    /// </summary>
    private void ReadData()
    {
        AndroidJNI.AttachCurrentThread();

        //Endless loop
        while (true)
        {
            if (listenerState != ListenerState.Connected)
                continue;

            MobileTools.Log("Request Data");

            //Send request to server about the data
            string req = "$DatReq$";
            if (bluetoothHandler.Call<bool>("WriteLine", req))
            {
                //Receive answer
                string reply = bluetoothHandler.Call<string>("ReadNextLine");
                if (reply != null)
                    DecodeReply(reply);
                else
                    MobileTools.Log("Failed to get reply");
            }
            else
            {
                MobileTools.Log("Failed to send request");
            }
        }
    }

    /// <summary>
    /// Tries to decode the reply from server
    /// </summary>
    /// <param name="reply"></param>
    private void DecodeReply(string reply)
    {
        string pattern = @"\$(\d+)\$";
        Match m = Regex.Match(reply, pattern);
        if (m.Success)
        {
            try
            {
                uint data = uint.Parse(m.Groups[1].Value);
                int rpm = (int)(data >> 17);

                int steer = (int)((data << 15) >> 17);
                steer = steer < 180 ? steer : steer - 360;

                int error = (int)((data << 30) >> 30);                

                VRBikeData currentInfo = new VRBikeData(rpm, steer, error);
                queuedData.Enqueue(currentInfo);
            }
            catch (Exception e)
            {
                MobileTools.Log("Could not convert reply into VRBikeData");
            }
        }
    }

    #endregion

    /// <summary>
    /// Gives back the bike data for a frame
    /// </summary>
    /// <param name="rpm"></param>
    /// <param name="steer"></param>
    /// <returns></returns>
    public void GetInput(ref float rpm, ref float steer)
    {
        //If there is something to give back, give that, otherwise continue with the last data
        if (queuedData.Count > 0)
            lastData = queuedData.Dequeue();

        rpm = lastData.RPM;
        steer = lastData.Steer;
    }
}