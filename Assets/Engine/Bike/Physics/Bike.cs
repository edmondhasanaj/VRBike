//TODO: Check for 3D Scenerios (rotation on X, or Z)
//TODO: Add code logic for checking for a crash of bike(maybe tight turn)
using UnityEngine;
using GPL;

namespace Engine.Bike.Physics
{
    /// <summary>
    /// Represents a normal bike with all the components
    /// </summary>
    public class Bike
    {
        /// <summary>
        /// Class that provides tools for the physics
        /// </summary>
        private static class Tools {
            public static float RPM_TO_RPS(float rpm) { return rpm / 60f; }
            public static float RPM_TO_SPEED(float rpm, float radius) { return RPM_TO_RPS(rpm) * (2 * Mathf.PI * radius); }
        }

        /// <summary>
        /// Class that holds important data for a single wheel of the bike
        /// </summary>
        public abstract class Wheel {

            /// <summary>
            /// Rigidbody attached to the wheel.
            /// </summary>
            public Rigidbody Rig { get; set; }

            /// <summary>
            /// The wheel mesh attached to the wheel. We need this in order to rotate the wheel
            /// </summary>
            public Transform Mesh { get; set; }

            /// <summary>
            /// Returns the bike that has this wheel
            /// </summary>
            public Bike ParentBike { get; private set; }

            /// <summary>
            /// The radius of this wheel
            /// </summary>
            public float Radius { get; set; }

            /// <summary>
            /// Current RPM of this wheel
            /// </summary>
            public float CurrentRPM { get; private set; }

            /// <summary>
            /// Returns the current speed of this wheel
            /// </summary>
            public float CurrentSpeed { get { return Tools.RPM_TO_SPEED(CurrentRPM, Radius); } }

            /// <summary>
            /// Front Direction of the Wheel
            /// </summary>
            public Vector3 Forward { get { return Rig.transform.forward; } }

            /// <summary>
            /// The wheel position
            /// </summary>
            public Vector3 Position { get { return Rig.position; } }

            /// <summary>
            /// The position the wheel will have the next frame
            /// </summary>
            public abstract Vector3 NextPosition { get; }


            public Wheel(Bike parentBike, Rigidbody rig, Transform mesh, float radius)
            {
                ParentBike = parentBike;
                Rig = rig;
                Radius = radius;
                Mesh = mesh;
            }


            /// <summary>
            /// Update the wheel, by providing the speed(rpm) and the steer
            /// for this frame. Should be called from fixed update. 
            /// </summary>
            /// <param name="speed"></param>
            /// <param name="steer"></param>
            public void UpdateWheel(float rpm, float steer)
            {
                UpdateWheelPhysics(rpm, steer);
                UpdateWheelAngularV(rpm);

                CurrentRPM = rpm;
            }

            /// <summary>
            /// Function that takes care only of the physics.
            /// </summary>
            /// <param name="speed"></param>
            /// <param name="steer"></param>
            protected abstract void UpdateWheelPhysics(float rpm, float steer);

            /// <summary>
            /// Update the angular velocity of the wheel
            /// </summary>
            /// <param name="rpm"></param>
            private void UpdateWheelAngularV(float rpm)
            {
                //Apply rotation of the mesh(based on the speed). This is nothing physical, it is just to give the effect of moving
                float rps = rpm / 60f;
                float rpf = (rps * 360) * Time.deltaTime; // Rotation per Frame in degrees
                Mesh.Rotate(Vector3.left, -rpf, Space.Self);
            }
        }

        /// <summary>
        /// Class that concentrates and represents a steering wheel sepcifically
        /// </summary>
        public class SteerWheel : Wheel
        {
            /// <summary>
            /// The position of the wheel in the next frame
            /// </summary>
            public override Vector3 NextPosition { get { return nextFramePos; } }
            private Vector3 nextFramePos;


            public SteerWheel(Bike parentBike, Rigidbody rig, Transform mesh, float radius) : base(parentBike, rig, mesh, radius)
            {
                nextFramePos = rig.position;
            }


            /// <summary>
            /// Update the wheel, by providing the speed(rpm) and the steer
            /// for this frame. Should be called from fixed update
            /// </summary>
            /// <param name="speed"></param>
            /// <param name="steer"></param>
            protected override void UpdateWheelPhysics(float rpm, float steer)
            {
                //Move to position
                Rig.MovePosition(new Vector3(nextFramePos.x, Rig.position.y, nextFramePos.z));

                //Rotation Per Second
                float rps = Tools.RPM_TO_RPS(rpm); 

                //Convert from RPM to speed to apply this frame
                float speed = Tools.RPM_TO_SPEED(rpm, Radius) * Time.deltaTime;

                //Convert steer to Vector direction
                Vector3 velocityDir = Math3D.RotateY(ParentBike.WheelBaseForward, -steer);

                //Apply velocity
                nextFramePos = Rig.position + velocityDir * speed;

                //Get and apply the rotation of the wheel(based on steer) 
                float yGlobalRotation = Math3D.Angle(Vector3.forward, velocityDir, Vector3.up);
                Rig.MoveRotation(Quaternion.Euler(0, yGlobalRotation, 0));
            }
        }

        /// <summary>
        /// Class that concentrates and represents a motor wheel specifically
        /// </summary>
        public class MotorWheel : Wheel
        {
            /// <summary>
            /// The position of the wheel in the next frame
            /// </summary>
            public override Vector3 NextPosition { get { return nextFramePos; } }
            private Vector3 nextFramePos;
            
            /// <summary>
            /// Reference to the front steering wheel
            /// </summary>
            private SteerWheel steerWheel { get { return ParentBike.FrontWheel; } }


            public MotorWheel(Bike parentBike, Rigidbody rig, Transform mesh, float radius) : base(parentBike, rig, mesh, radius)
            {
                nextFramePos = rig.position;
            }


            /// <summary>
            /// Update the wheel, by providing the speed(rpm) and the steer
            /// for this frame. Should be called from fixed update
            /// </summary>
            /// <param name="speed"></param>
            /// <param name="steer"></param>
            protected override void UpdateWheelPhysics(float rpm, float steer)
            {
                //Move to the next posiition
                Rig.MovePosition(new Vector3(nextFramePos.x, Rig.position.y, nextFramePos.z));

                //We need to have a stable distance from the front wheel. Dist = Wheelbase
                Vector3 currentPos = Position;
                Vector3 steerWheelPos = steerWheel.NextPosition;
                Vector3 dir = (steerWheelPos - currentPos).normalized;
                Vector3 ourNewPos = steerWheelPos - dir * ParentBike.WheelBase;
                nextFramePos = ourNewPos;

                //Apply rotation of the wheel
                float yGlobalRotation = Math3D.Angle(Vector3.forward, dir, Vector3.up);
                Rig.MoveRotation(Quaternion.Euler(0, yGlobalRotation, 0));
            }
        }

        /// <summary>
        /// Represents the body of the bike
        /// </summary>
        public class Body {
            /// <summary>
            /// The mesh of the body
            /// </summary>
            public Transform Mesh { get; private set; }

            /// <summary>
            /// Position relative to back wheel
            /// </summary>
            public Vector3 RelativePosition { get; private set; }

            /// <summary>
            /// The parent bike
            /// </summary>
            public Bike ParentBike { get; private set; }
            

            public Body(Bike parentBike, Transform mesh, Vector3 relaivePosition)
            {
                Mesh = mesh;
                RelativePosition = relaivePosition;
                ParentBike = parentBike;
            }

            
            /// <summary>
            /// Updates the physics
            /// </summary>
            public void UpdatePhysics()
            {
                //We only need to update the mesh. No physics
                Vector3 newPos = ParentBike.BackWheel.Position + ParentBike.WheelBaseForward * RelativePosition.z + new Vector3(0f, RelativePosition.y, 0f);
                Mesh.position = newPos;

                //Apply rotation of the wheel. It will have the same rotation of the back wheel
                float yGlobalRotation = Math3D.Angle(Vector3.forward, ParentBike.WheelBaseForward, Vector3.up);
                Mesh.rotation = (Quaternion.Euler(0, yGlobalRotation, 0));
            }
        }


        /// <summary>
        /// The frame 
        /// </summary>
        public Body Frame { get; private set; }

        /// <summary>
        /// The wheel in the front
        /// </summary>
        public SteerWheel FrontWheel { get; private set; }

        /// <summary>
        /// The wheel in the back
        /// </summary>
        public MotorWheel BackWheel { get; private set; }

        /// <summary>
        /// Wheelbase is the distance between both wheels
        /// </summary>
        public float WheelBase { get; private set; }

        /// <summary>
        /// Speed in m/s
        /// </summary>
        public float Speed { get { return (BackWheel.CurrentSpeed + FrontWheel.CurrentSpeed) / 2f; } }

        /// <summary>
        /// The wheel base forward direction. Equals to the back wheel direction
        /// </summary>
        public Vector3 WheelBaseForward { get { return BackWheel.Forward; } }

        /// <summary>
        /// The forward direction of the bike. Equals to the front direction of the front wheel
        /// </summary>
        public Vector3 Forward { get { return FrontWheel.Forward; } }

        /// <summary>
        /// The maximum RPM this bike can reach
        /// </summary>
        public float MaxRPM { get; private set; }

        /// <summary>
        /// The maximum Steer this bike can reach
        /// </summary>
        public float MaxSteer { get; private set; }



        public Bike(float maxRPM, float maxSteer)
        {
            MaxRPM = maxRPM;
            MaxSteer = maxSteer;
        }
        public Bike(SteerWheel frontWheel, MotorWheel backWheel, Body frame, float maxRPM, float maxSteer) : this(maxRPM, maxSteer)
        {
            AddComponents(frontWheel, backWheel, frame);
        }


        /// <summary>
        /// Add the needed components to this bike. Especially needed, if the default constructor was called
        /// </summary>
        /// <param name="frontWheel"></param>
        /// <param name="backWheel"></param>
        /// <param name="body"></param>
        public void AddComponents(SteerWheel frontWheel, MotorWheel backWheel, Body frame)
        {
            FrontWheel = frontWheel;
            BackWheel = backWheel;
            Frame = frame;

            //Calculate wheelbase
            WheelBase = Vector3.Distance(FrontWheel.Position, BackWheel.Position);
        }

        /// <summary>
        /// Update the front wheel's physics
        /// </summary>
        /// <param name="steerAngle"></param>
        /// <param name="rpm"></param>
        public void UpdateBikePhysics(float rpm, float steer)
        {
            //Normalize steer and RPM
            steer = Mathf.Clamp(steer, -MaxSteer, MaxSteer);
            rpm = Mathf.Clamp(rpm, 0, MaxRPM);

            FrontWheel.UpdateWheel(rpm, steer);
            BackWheel.UpdateWheel(rpm, steer);
            Frame.UpdatePhysics();

            SteerCrashCheck(rpm, steer);
        }

        /// <summary>
        /// Check if there is too much steer being applied
        /// </summary>
        private void SteerCrashCheck(float rpm, float steer)
        {
            //TODO: Implement crashin code
        }
    }
}