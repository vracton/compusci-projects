using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force for a simple C*v^2 dependence for air resistance
    /// </summary>
    /// <param name="coefficient">The coefficient of air resistance, including drag coefficient, density of air, and cross-sectional area</param>
    public class AirResistanceForce(KinematicsEngine engine, double coefficient) : GlobalForce(engine)
    {
        override protected Vector GetForce(Projectile proj)
        {
            return proj.Velocity.UnitVector() * (-coefficient * proj.Velocity.MagnitudeSquared);
        }

    }
}
