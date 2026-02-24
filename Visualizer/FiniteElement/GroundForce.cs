using DongUtility;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;

namespace Visualizer.FiniteElement
{
    // A quick and dirty way to get simple behavior at the ground
    public class GroundForce(KinematicsEngine engine) : GlobalForce(engine)
    {
        private const double MuSlide = 100.0;
        private const double GravityMagnitude = 9.8;
        private const double TangentialSpeedEpsilon = 1e-12;

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
            if (!ConditionMet(projectile))
            {
                return Vector.NullVector();
            }

            Vector response = Vector.NullVector();

            if (projectile.Velocity.Z < 0)
            {
                projectile.Velocity = new Vector(projectile.Velocity.X, projectile.Velocity.Y, -projectile.Velocity.Z);
            }

            Vector tangentialVelocity = new Vector(projectile.Velocity.X, projectile.Velocity.Y, 0.0);
            double tangentialSpeed = tangentialVelocity.Magnitude;
            if (tangentialSpeed > TangentialSpeedEpsilon)
            {
                double frictionMagnitude = MuSlide * projectile.Mass * GravityMagnitude;
                response = -(frictionMagnitude / tangentialSpeed) * tangentialVelocity;
            }

            return response;
        }
    }
}
