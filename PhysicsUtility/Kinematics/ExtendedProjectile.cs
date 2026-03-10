using DongUtility;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A base class for a projectile that takes up space and is not pointlike
    /// </summary>
    /// <param name="position">Position of the center of mass of the object</param>
    /// <param name="velocity">Linear velocity of the object</param>
    abstract public class ExtendedProjectile(Vector position, Vector velocity, double mass) : Projectile(position, velocity, mass)
    {
        /// <summary>
        /// The orientation of the extended object, given as a rotation from the default orientation
        /// </summary>
        public Rotation Orientation { get; set; } = Rotation.Identity;
        public Vector AngularVelocity { get; set; } = Vector.NullVector();
        public Vector AngularAcceleration { get; protected set; } = Vector.NullVector();
        public Vector NetTorque { get; protected set; } = Vector.NullVector();

        /// <summary>
        /// Adds the force at a particular location on the object, allowing it to automatically calculate torque
        /// </summary>
        public void AddForce(Vector force, Vector location)
        {
            AddForce(force);
            AddTorque(Vector.Cross(location - CenterOfMass, force));
        }

        public abstract Vector CenterOfMass { get; }

        /// <summary>
        /// The inverse of the inertial tensor, which is the important part for angular acceleration
        /// </summary>
        abstract public Matrix InertialTensorInverse { get; }

        public void AddTorque(Vector torque)
        {
            NetTorque += torque;
        }

        private void UpdateAngularAcceleration()
        {
            AngularAcceleration = InertialTensorInverse * NetTorque;
        }

        private void UpdateAngularVelocity(double timeIncrement)
        {
            AngularVelocity += AngularAcceleration * timeIncrement;
        }

        private void UpdateOrientation(double timeIncrement)
        {
            Orientation *= new Rotation(AngularVelocity, AngularVelocity.Magnitude * timeIncrement);
        }

        public override void Update(double timeIncrement)
        {
            base.Update(timeIncrement);

            UpdateAngularAcceleration();
            NetTorque = Vector.NullVector(); // Reset torques
            UpdateAngularVelocity(timeIncrement);
            UpdateOrientation(timeIncrement);
        }
    }
}
