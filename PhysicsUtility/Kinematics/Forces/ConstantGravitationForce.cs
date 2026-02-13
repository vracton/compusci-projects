using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A constant gravitational force applied to all objects
    /// </summary>
    public class ConstantGravitationForce(KinematicsEngine engine, Vector fieldStrength) : GlobalForce(engine)
    {
        protected override Vector GetForce(Projectile proj)
        {
            return proj.Mass * fieldStrength;
        }
    }
}
