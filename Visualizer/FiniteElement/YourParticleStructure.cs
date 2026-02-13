using DongUtility;
using PhysicsUtility.Kinematics;

namespace Visualizer.FiniteElement
{
    /// <summary>
    /// Use this structure, or make other classes like it, to create your solid objects
    /// </summary>
    class YourParticleStructure : ParticleStructure
    {
        public YourParticleStructure()
        {
            const double mass = .1;
            // Here's how you add a projectile
            var newProjectile = new Projectile(
                 new Vector(0, 0, 10), // Initial position
                 new Vector(0, 0, 0), // Initial velocity
                 mass);
            AddProjectile(newProjectile);
            var newProjectile2 = new Projectile(
                 new Vector(1, 0, 10), // Initial position
                 new Vector(0, 0, 0), // Initial velocity
                 mass);
            AddProjectile(newProjectile2);

            const double springConstant = 1;
            // Here's how you connect two projectiles with springs
            AddConnector(newProjectile, newProjectile2, springConstant, 1);
        }
    }
}
