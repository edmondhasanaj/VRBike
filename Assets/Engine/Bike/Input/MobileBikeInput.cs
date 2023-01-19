/* *****************************************************
 * Test class used to test the bike by providing input *
 * from keyboard                                       *
 * *****************************************************/

using UnityEngine;
using Engine.Bike.Physics;

namespace Engine.Bike.Input {

    /// <summary>
    /// General Class that provides Input for the bike from a normal phone.
    /// The reason why this class is the constant testing of the physics of the bike.
    /// </summary>
    public class MobileBikeInput : MonoBehaviour, IBikeInput
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
            rpm = currentRPM;
            steer = currentSteer;
        }

        public void Update()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch finger = UnityEngine.Input.GetTouch(0);
                Debug.Log(finger.position);

                float width = Screen.currentResolution.width;
                if (finger.position.x < width / 3)
                {
                    //Turn left
                    currentRPM = maxRPM;
                    currentSteer = -maxSteer;
                }
                else if (finger.position.x > 2 * width / 3)
                {
                    //Turn Right
                    currentRPM = maxRPM;
                    currentSteer = maxSteer;
                }
                else
                {
                    //Move forward
                    currentRPM = maxRPM;
                    currentSteer = 0;
                }
            }
        }
    }
}