using UnityEngine;
using UnityEngine.XR;

public class VRDeinitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        XRSettings.enabled = false;
    }
}
