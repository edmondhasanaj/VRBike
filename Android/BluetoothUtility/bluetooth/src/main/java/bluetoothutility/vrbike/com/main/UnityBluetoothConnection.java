package bluetoothutility.vrbike.com.main;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

public class UnityBluetoothConnection {
    //Settings
    private final String FRAGMENT_TAG = "VRBikeServerConnectionFragment";
    private ServerConnectFragment connectFragment;
    private final BluetoothHandler bluetoothHandler;


    /**
     * Init this object
     * @param handler
     */
    public UnityBluetoothConnection(BluetoothHandler handler)
    {
        bluetoothHandler = handler;
    }

    /**
     * Start the server connection process. When we start this, the user
     * will need to connect to a valid VRBike Server. Only if the user connects
     * to that, this window will be closed. There is no chance that we can get back
     * without connecting successfully
     * @param unityGOName
     * @param onConnectionStoredCallback
     * @return True if the connection window starts, false if it is already started
     */
    public boolean Start(final String unityGOName, final String onConnectionStoredCallback)
    {
        Log.d("[DBG]", "Attempt to start Connection");

        //Check if we are already trying to connect
        if(connectFragment != null)
            return false;

        Log.d("[DBG]", "Starting Connection");

        //Display the fragment as a dialog
        connectFragment = new ServerConnectFragment(bluetoothHandler, new OnConnectedListener() {
            @Override
            public void OnConnected() {
                //Reset & Send Message
                Log.d("[DBG]","Connection Succeeded");
                connectFragment.dismiss();
                connectFragment = null;
                UnityPlayer.UnitySendMessage(unityGOName, onConnectionStoredCallback, "");
            }
        });
        connectFragment.show(UnityPlayer.currentActivity.getFragmentManager().beginTransaction(), FRAGMENT_TAG);

        return true;
    }
}
