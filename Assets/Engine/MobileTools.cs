using UnityEngine;


namespace Engine
{
    public class MobileTools
    {
        public static string TAG = "DBG";

        public static void Log(string message)
        {
            Debug.Log("[" + TAG + "] " + message);
        }
    }
}
