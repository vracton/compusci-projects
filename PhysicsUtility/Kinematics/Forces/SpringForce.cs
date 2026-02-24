using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force from a spring
    /// The position of the other end of the spring is abstract
    /// </summary>
    abstract public class SpringForce(Projectile projectile, double springConstant,
        double unstretchedLength = 0) : SingleProjectileForce(projectile)
    {
        protected abstract Vector SpringPosition();

        protected override Vector GetForce()
        {
            Vector difference = Particle.Position - SpringPosition();
            double magnitude = springConstant * (unstretchedLength - difference.Magnitude);
            return magnitude * difference.UnitVector();
        }
    }
}