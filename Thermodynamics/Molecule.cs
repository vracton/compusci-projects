using DongUtility;
using PhysicsUtility.Kinematics;

namespace Thermodynamics
{
    /// <summary>
    /// A single particle in the simulation.
    /// Basically a Projectile with an Info object for its type
    /// </summary>
    public class Molecule(Vector position, Vector velocity, ParticleInfo info) : Projectile(position, velocity, info.Mass)
    {
        public ParticleInfo Info { get; init; } = info;
    }
}
