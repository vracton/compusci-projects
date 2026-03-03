using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force simulating non-constant Newtonian gravity (for planets and similar projectiles)
    /// </summary>
    public class UniversalGravitationForce(KinematicsEngine engine) : GlobalForce(engine)
    {
        protected override Vector GetForce(Projectile proj)
        {
            Vector netForce = Vector.NullVector();
            foreach (var point in AllProjectiles)
            {
                if (point.Position != proj.Position)
                {
                    Vector difference = point.Position - proj.Position;
                    double distance2 = difference.MagnitudeSquared;
                    double denom = distance2 * Math.Sqrt(distance2);
                    Vector force = (Constants.GravitationalConstant * proj.Mass * point.Mass / denom) * difference;
                    netForce += force;
                }
            }
            return netForce;
        }
    }
}
