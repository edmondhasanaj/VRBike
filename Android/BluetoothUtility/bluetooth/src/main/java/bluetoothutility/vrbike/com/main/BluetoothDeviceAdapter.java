package bluetoothutility.vrbike.com.main;

import android.bluetooth.BluetoothDevice;
import android.content.Context;
import android.support.annotation.NonNull;
import android.support.annotation.Nullable;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import java.util.ArrayList;

public class BluetoothDeviceAdapter extends ArrayAdapter<BluetoothDevice> {
    public BluetoothDeviceAdapter(@NonNull Context context, @NonNull ArrayList<BluetoothDevice> objects) {
        super(context, R.layout.paired_device_list, objects);
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        LayoutInflater inflater = LayoutInflater.from(getContext());
        View row = inflater.inflate(R.layout.paired_device_list, parent, false);

        TextView pdn_text = (TextView) row.findViewById(R.id.paired_device_name);
        TextView pdm_text = (TextView) row.findViewById(R.id.paired_device_mac);
        pdn_text.setText(getItem(position).getName());
        pdm_text.setText(getItem(position).getAddress());

        return row;
    }
}
