/* *****************************************************
 * Test class used to test the bike by providing input *
 * from keyboard                                       *
 * *****************************************************/

using UnityEngine;
using Engine.Bike.Physics;

namespace Engine.Bike.Input {

    /// <summary>
    /// General Class that provides Input for the bike from a normal keyboard.
    /// The reason why this class is the constant testing of the physics of the bike.
    /// </summary>
    public class KeyboardBikeInput : IBikeInput
    {
        /// <summary>
        /// The maximum Steer angle
        /// </summary>
        [SerializeField] private float maxSteer;

        /// <summary>
        /// The maximum RPM we can achieve
        /// </summary>
        [SerializeField] private float maxRPM;

        // Current Data
        private float currentRPM = 0f;
        private float currentSteer = 0f;
        
        /// <summary>
        /// Interface Implementation
        /// </summary>
        /// <param name="rpm"></param>
        /// <param name="steer"></param>
        public void GetInput(ref float rpm, ref float steer)
        {
            #if UNITY_EDITOR
            //Calculate the current values and pass them to the ref parameters.

            float targetRPM = Mathf.Clamp01(UnityEngine.Input.GetAxis("Vertical")) * maxRPM;
            rpm = currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime);

            float targetSteer = UnityEngine.Input.GetAxis("Horizontal") * maxSteer;
            steer = currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.deltaTime);

            #elif UNITY_ANDROID
            float targetRPM = UnityEngine.Input.touchCount > 0 ? maxRPM : 0;
            rpm = currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime);
            #endif
        }
    }
}