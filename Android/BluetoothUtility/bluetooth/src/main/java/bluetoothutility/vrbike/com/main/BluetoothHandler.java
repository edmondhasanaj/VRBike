package bluetoothutility.vrbike.com.main;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.util.Log;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.util.Set;
import java.util.UUID;

/**
 * Plugin that enables Unity to check if there is bluetooth, and communicate
 * with the Bluetooth Server of VRBike
 */
public class BluetoothHandler{
    private final BluetoothAdapter bAdapter;
    private BluetoothSocket currentSocket;
    private PrintWriter outStream;
    private BufferedReader inReader;

    /**
     * Initializes the class
     */
    public BluetoothHandler()
    {
        bAdapter = BluetoothAdapter.getDefaultAdapter();
    }

    /**
     * Check if the device supports bluetooth or not
     * @return true if the bluetooth adapter was found
     */
    public boolean HasBluetooth() {
        return bAdapter != null;
    }

    /**
     * Check if bluetooth is enabled or not
     * @return The state of bluetooth
     */
    public boolean IsBluetoothEnabled(){
        return bAdapter != null && bAdapter.isEnabled();
    }

    /**
     * Returns a list of all the paired devices. If the bluetooth is not enabled,
     * the list will contain 0 devices
     * @return
     */
    public BluetoothDevice[] GetPairedDevices() {
        Set<BluetoothDevice> pairedDevices = bAdapter.getBondedDevices();
        return pairedDevices.toArray(new BluetoothDevice[0]);
    }

    /**
     * Connect to the specified device. Bluetooth must be enabled
     * and the device must be paired in order to do this. This is a blocking method
     * and should not be called from the main thread. It takes approx 5 seconds for the
     * connection to succeed.
     * @param device
     * @param UUIDCode
     * @return  Returns true if the device was successfully connected
     */
    public boolean ConnectToDevice(BluetoothDevice device, String UUIDCode) {
        //Calc the UUID
        UUID uuid = UUID.fromString(UUIDCode);

        //Init Socket
        try {
            currentSocket = device.createRfcommSocketToServiceRecord(uuid);
            Log.d("EAK", "Creating socket succeeded");
        } catch (IOException e) {
            Log.e("EAK", "Socket's create() method failed", e);
            return false;
        }

        //If socket was successful, init Out Stream
        try {
            outStream = new PrintWriter(currentSocket.getOutputStream());
        } catch (IOException e) {
            Log.e("EAK", "Could not receive the output stream");
            return false;
        }

        //If all those were successful, init In Stream
        try {
            inReader = new BufferedReader(new InputStreamReader(currentSocket.getInputStream()));
            Log.d("EAK", (currentSocket.getInputStream() != null) ? "Got the Reader" : "Null");
        } catch (IOException e) {
            Log.e("EAK", "Could not receive the input stream");
            return false;
        }

        bAdapter.cancelDiscovery();

        //Finally connect
        try {
            currentSocket.connect();
        } catch (IOException e) {
            Log.e("EAK", "Could not connect to the received socket");

            //Reset connection
            CloseChannel();
            return false;
        }

        return true;
    }

    /**
     * After the device has successfully connected with another device,
     * this function reads the next line coming from that device.
     * If there is no connection, the returned string will be null.
     * This is a blocking call. If there is a connection, the thread
     * will be blocked until we receive something.
     * @return
     */
    public String ReadNextLine(){
        if(currentSocket == null || outStream == null || inReader == null)
            return null;

        try {
            return inReader.readLine();
        } catch (Exception e) {
            Log.e("EAK", "Could not read the next line");
            return null;
        }
    }

    /**
     * Writes a line and sends it to the connected bluetooth device
     * @param line
     * @return True if the line was sent successfully
     */
    public boolean WriteLine(String line) {
        if(currentSocket == null || outStream == null || inReader == null)
            return false;

        outStream.write(line);
        outStream.flush();
        return true;
    }

    /**
     * Closes the current channel(after we connected to a device).
     * Even if we are not connected to any device, this method can be called,
     * though it won't do anything, but clear the connection resources.
     */
    public void CloseChannel(){
        Log.d("EAK","Closing everything");

        try {
            currentSocket.close();
        }
        catch (Exception e) {
            Log.e("EAK", "Could not close Socket");
        }

        try {
            outStream.close();
        }
        catch (Exception e) {
            Log.e("EAK", "Could not close Out Writer");
        }

        try {
            inReader.close();
        }
        catch(Exception e)
        {
            Log.e("EAK", "Could not close In Reader");
        }


        inReader = null;
        outStream = null;
        currentSocket = null;

        Log.d("EAK", "Closed everything");
    }
}
