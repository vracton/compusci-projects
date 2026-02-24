using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force which stops a projectile entirely
    /// </summary>
    abstract public class StoppingForce(KinematicsEngine engine) : GlobalForce(engine)
    {
        private Vector forceLastTime = Vector.NullVector();
        private double formerTime = 0;

        /// <summary>
        /// The condition that must be met for the force to "turn on"
        /// Otherwise, nothing happens
        /// </summary>
        abstract protected bool ConditionMet(Projectile projectile);

        override protected Vector GetForce(Projectile projectile)
        {
            Vector response = Vector.NullVector();

            if (Time > 0 && ConditionMet(projectile))
            {
                double timeIncrement = Time - formerTime;

                // Adjust for velocity
                Vector counterAcc = projectile.Velocity / (-timeIncrement);
                Vector counterForce1 = counterAcc * projectile.Mass;

                // Adjust for other forces
                Vector counterForce2 = forceLastTime - projectile.Acceleration * projectile.Mass;

                response = counterForce1 + counterForce2;
            }

            formerTime = Time;
            forceLastTime = response;
            return response;
        }
    }
}
