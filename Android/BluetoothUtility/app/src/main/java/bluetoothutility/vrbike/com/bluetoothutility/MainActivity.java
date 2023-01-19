package bluetoothutility.vrbike.com.bluetoothutility;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;

import bluetoothutility.vrbike.com.main.BluetoothHandler;
import bluetoothutility.vrbike.com.main.OnConnectedListener;
import bluetoothutility.vrbike.com.main.ServerConnectFragment;
import bluetoothutility.vrbike.com.main.VRBikeServerConnection;

public class MainActivity extends AppCompatActivity {
    ServerConnectFragment connectFragment = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Log.d("EAK", "Attempt to start Connection");

        //Check if we are already trying to connect
        if(connectFragment != null)
            Log.d("EAK", "Already initialized screen");

        Log.d("EAK", "Starting Connection");

        //Display the fragment as a dialog
        connectFragment = new ServerConnectFragment(new BluetoothHandler(), new OnConnectedListener() {
            @Override
            public void OnConnected() {
                //Reset & Send Message
                Log.d("EAK", "Connection Succeeded");
                connectFragment.dismiss();
                connectFragment = null;
            }
        });
        connectFragment.show(getFragmentManager(), "Connection");
    }
}
