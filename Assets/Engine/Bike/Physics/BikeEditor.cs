using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Engine.Bike.Input;
using Sirenix.Serialization;

namespace Engine.Bike.Physics
{
    /// <summary>
    /// Makes a bike ready to have its own physics.
    /// </summary>
    public class BikeEditor : SerializedMonoBehaviour
    {
        /// <summary>
        /// Struct for getting all the bike input from unity
        /// </summary>
        [Serializable]
        private struct PhysicsInput
        {
            [SerializeField, BoxGroup("General"), MinValue(0.1f)]
            public float MaxSteer;
            [SerializeField, BoxGroup("General"), MinValue(0.1f)]
            public float MaxRPM;
            [SerializeField, BoxGroup("General"), MinValue(0.1f)]
            public float Wheelbase;

            [SerializeField, Required, BoxGroup("Front Wheel")]
            public Rigidbody FWheelRigidbody;
            [SerializeField, Required, BoxGroup("Front Wheel")]
            public Transform FWheelMesh;
            [SerializeField, MinValue(0.1f), BoxGroup("Front Wheel")]
            public float FWheelRadius;

            [SerializeField, Required, BoxGroup("Back Wheel")]
            public Rigidbody BWheelRigidbody;
            [SerializeField, Required, BoxGroup("Back Wheel")]
            public Transform BWheelMesh;
            [SerializeField, MinValue(0.1f), BoxGroup("Back Wheel")]
            public float BWheelRadius;

            [SerializeField, Required, BoxGroup("Body")]
            public Transform Body;
            [SerializeField, BoxGroup("Body")]
            public Vector3 RelativeWheelPosition;
        }


        /// <summary>
        /// Load all the input in this variable
        /// </summary>
        [SerializeField]
        private PhysicsInput input;

        /// <summary>
        /// The input
        /// </summary>
        [OdinSerialize]
        private IBikeInput bikeInput;

        /// <summary>
        /// Should we simulate the bike?
        /// </summary>
        public bool Simulate { get { return simulate; } set { simulate = value; } }
        [SerializeField] private bool simulate = true;

        /// <summary>
        /// Bike instance
        /// </summary>
        private Bike bike;


        void Awake()
        {
            //Check for errors in input
            if (input.FWheelRigidbody == null || input.BWheelRigidbody == null || input.FWheelRadius <= 0 || input.BWheelRadius <= 0 || input.Body == null)
                throw new Exception("There is something wrong with the input");

            //Create instance
            bike = new Bike(input.MaxRPM, input.MaxSteer);
            Bike.Body body = new Bike.Body(bike, input.Body, input.RelativeWheelPosition);
            Bike.SteerWheel frontWheel = new Bike.SteerWheel(bike, input.FWheelRigidbody, input.FWheelMesh, input.FWheelRadius);
            Bike.MotorWheel backWheel = new Bike.MotorWheel(bike, input.BWheelRigidbody, input.BWheelMesh, input.BWheelRadius);
            bike.AddComponents(frontWheel, backWheel, body);
        }

        void FixedUpdate()
        {
            if (Simulate)
            {
                float rpm = 0f, steer = 0f;
                bikeInput.GetInput(ref rpm, ref steer);
                bike.UpdateBikePhysics(rpm, steer);
            }
        }
    }
}