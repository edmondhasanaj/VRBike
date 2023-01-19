namespace Engine.Bike.Physics
{
    /// <summary>
    /// Defines the input for the bike physics
    /// </summary>
    public interface IBikeInput
    {
        /// <summary>
        /// Gives back the RPM and the steer.
        /// RPM should always be positive, and steer should be positive to the right,
        /// and negative to the left side
        /// </summary>
        /// <param name="rpm"></param>
        /// <param name="steer"></param>
        void GetInput(ref float rpm, ref float steer);
    }
}
