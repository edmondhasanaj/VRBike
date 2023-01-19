package bluetoothutility.vrbike.com.main;

import android.annotation.SuppressLint;
import android.app.DialogFragment;
import android.app.ProgressDialog;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.DialogInterface;
import android.content.IntentFilter;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.util.DisplayMetrics;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import java.util.ArrayList;

@SuppressLint("ValidFragment")
public class ServerConnectFragment extends DialogFragment {
    //Bluetooth Handler
    BluetoothHandler btHandler;

    //What to call when connected
    private OnConnectedListener onConnectedListener;

    //Bluetooth State Change Receiver
    private BluetoothStateBroadcastReceiver stateReceiver;

    //Layout Resources from XML
    private TextView bluetoothAvailableText;
    private TextView bluetoothStateText;
    private Button pairedDevicesButton;
    private ListView pairedDevicesList;
    private ArrayAdapter<BluetoothDevice> pairedDevicesListAdapter;

    //The popup window
    private ProgressDialog loadingPopup;
    private final String PORT_UUID = "00001101-0000-1000-8000-00805f9b34fb";


    @SuppressLint("ValidFragment")
    public ServerConnectFragment(BluetoothHandler btHandler, OnConnectedListener onConnectedListener)
    {
        this.btHandler = btHandler;
        this.onConnectedListener = onConnectedListener;
    }

    @Nullable
    @Override
    public View onCreateView(LayoutInflater inflater, @Nullable ViewGroup container, Bundle savedInstanceState) {
        //Load XML
        View newView =  inflater.inflate(R.layout.bluetooth_server_connection, container, false);

        //Load Variables
        bluetoothAvailableText = (TextView) newView.findViewById(R.id.t_bluetooth_available);
        bluetoothStateText = (TextView) newView.findViewById(R.id.t_bluetooth_status);
        pairedDevicesButton = (Button) newView.findViewById(R.id.b_paired_devices);
        pairedDevicesList = (ListView) newView.findViewById(R.id.lv_paired_devices);
        pairedDevicesListAdapter = new BluetoothDeviceAdapter(getActivity(), new ArrayList<BluetoothDevice>());
        pairedDevicesList.setAdapter(pairedDevicesListAdapter);
        pairedDevicesList.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                BluetoothDevice device = (BluetoothDevice)pairedDevicesList.getItemAtPosition(position);
                TryConnectToDevice(device);
            }
        });

        //Init
        bluetoothAvailableText.setText(btHandler.HasBluetooth() ? "Yes" : "No");
        pairedDevicesButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                LoadPairedDevices();
            }
        });

        //Add Broadcast Receiver for when bluetooth's state changes
        stateReceiver = new BluetoothStateBroadcastReceiver(bluetoothStateText, btHandler);
        IntentFilter bluetoothStateFilter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
        getActivity().registerReceiver(stateReceiver, bluetoothStateFilter);

        //Return result
        return newView;
    }

    @Override
    public void onStart() {
        DisplayMetrics displayMetrics = new DisplayMetrics();
        getActivity().getWindowManager().getDefaultDisplay().getMetrics(displayMetrics);
        int height = displayMetrics.heightPixels;
        int width = displayMetrics.widthPixels;

        int padding = (int)(0.05f * width);

        //Set properties
        setCancelable(false);
        getDialog().getWindow().setLayout(width - 2 * padding, height - 2 * padding);
        super.onStart();
    }

    @Override
    public void onDestroy() {
        super.onDestroy();

        //Unregister
        getActivity().unregisterReceiver(stateReceiver);
    }

    private void LoadPairedDevices() {
        if(!btHandler.HasBluetooth() || !btHandler.IsBluetoothEnabled())
            return;

        BluetoothDevice devices[] = btHandler.GetPairedDevices();

        pairedDevicesListAdapter.clear();
        pairedDevicesListAdapter.addAll(devices);
    }

    private void TryConnectToDevice(final BluetoothDevice device){
        //Init a new thread to connect to the device
        new Thread(){
            @Override
            public void run() {
                //Did the device connect successfully?
                boolean connected = btHandler.ConnectToDevice(device, PORT_UUID);

                //True if we got connected and the device is a valid server
                final boolean connectionSucceeded;

                //If the connection was successful, we need a last check to know if we connected to a valid server
                //In this check we send a message to the server and it must reply with a specifc answer
                if(connected) {
                    //Send Request & wait for answer
                    btHandler.WriteLine("$ConReq$");

                    //TODO: Implement Timeout
                    String res = btHandler.ReadNextLine();

                    //Was the response OK?
                    if (res != null && res.equals("$ConAck$")) {
                        connectionSucceeded = true;
                    } else {
                        //If not, disconnect from the current device
                        connectionSucceeded = false;
                        btHandler.CloseChannel();
                    }
                }
                else
                    connectionSucceeded = false;

                //Run on UI Thread
                getActivity().runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        //Callback
                        if(connectionSucceeded)
                            OnConnectionSucceeded();
                        else
                            OnConnectionFailed();
                    }
                });
            }
        }.start();

        //Create the new popup
        ClosePopup();
        loadingPopup = ProgressDialog.show(getActivity(), "Please Wait", "We are validating the server and connecting...", true);
        loadingPopup.setCancelable(true);
        loadingPopup.setOnCancelListener(new DialogInterface.OnCancelListener() {
            @Override
            public void onCancel(DialogInterface dialog) {
                //When the dialog is canceled, the extra thread should directly stop
                //We close the bluetooth channel, and the thread should then close by itself
                btHandler.CloseChannel();
            }
        });
    }

    private void OnConnectionSucceeded(){
        ClosePopup();
        Toast.makeText(getActivity(), "Connection Succeeded", Toast.LENGTH_SHORT).show();
        onConnectedListener.OnConnected();
    }

    private void OnConnectionFailed(){
        ClosePopup();
        Toast.makeText(getActivity(), "Connection Failed", Toast.LENGTH_SHORT).show();
    }

    private void ClosePopup() {
        if(loadingPopup != null)
        {
            loadingPopup.dismiss();
            loadingPopup = null;
        }
    }
}
