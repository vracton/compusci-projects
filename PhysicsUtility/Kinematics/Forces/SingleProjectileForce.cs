using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force which affects only a single projectile
    /// </summary>
    abstract public class SingleProjectileForce(Projectile particle) : Force
    {
        protected Projectile Particle { get; } = particle;

        /// <summary>
        /// Returns the actual force on the particle
        /// </summary>
        abstract protected Vector GetForce();

        public override void AddForce(double deltaTime)
        {
            Particle.AddForce(GetForce());
        }
    }
}
