package bluetoothutility.vrbike.com.main;

import android.bluetooth.BluetoothAdapter;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.widget.TextView;


public class BluetoothStateBroadcastReceiver extends BroadcastReceiver {
    //The text that displays the current bluetooth state
    private TextView bluetoothStateText;

    //The global handler for the bluetooth
    BluetoothHandler bluetoothHandler;

    public BluetoothStateBroadcastReceiver(TextView stateText, BluetoothHandler handler)
    {
        bluetoothStateText = stateText;
        bluetoothHandler = handler;

        //Load the state text for the first time
        bluetoothStateText.setText(bluetoothHandler.IsBluetoothEnabled() ? "On" : "Off");
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        final String action = intent.getAction();

        if (action.equals(BluetoothAdapter.ACTION_STATE_CHANGED)) {
            final int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE,
                    BluetoothAdapter.ERROR);
            switch (state) {
                case BluetoothAdapter.STATE_OFF:
                    bluetoothStateText.setText("Off");
                    break;
                case BluetoothAdapter.STATE_ON:
                    bluetoothStateText.setText("On");
                    break;
            }
        }
    }
}
