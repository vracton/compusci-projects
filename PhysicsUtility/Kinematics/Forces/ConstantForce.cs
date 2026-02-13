using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// Any force on a particle that is the same at all times
    /// </summary>
    public class ConstantForce(Projectile proj, Vector force) : SingleProjectileForce(proj)
    {
        protected override Vector GetForce()
        {
            return force;
        }
    }
}
