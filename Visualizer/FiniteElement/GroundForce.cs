using DongUtility;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;

namespace Visualizer.FiniteElement
{
    // A quick and dirty way to get simple behavior at the ground
    public class GroundForce(KinematicsEngine engine) : GlobalForce(engine)
    {
        /// <summary>
        /// The condition that must be met for the force to "turn on"
        /// Otherwise, nothing happens
        /// </summary>
        protected static bool ConditionMet(Projectile projectile)
        {
            return projectile.Position.Z <= 0;
        }

        override protected Vector GetForce(Projectile projectile)
        {
            Vector response = Vector.NullVector();

            if (ConditionMet(projectile))
            {
                if (projectile.Velocity.Z < 0)
                    projectile.Velocity = new Vector(projectile.Velocity.X, projectile.Velocity.Y, -projectile.Velocity.Z);
            }
            return response;
        }
    }
}
