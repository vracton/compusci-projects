using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A spring force for which the end is oscillating sinusoidally
    /// </summary>
    public class OscillatingSpringForce(Projectile projectile, double springConstant,
        Vector origin, double unstretchedLength, Vector amplitude, double frequency,
        KinematicsEngine engine) 
        : SpringForce(projectile, springConstant, unstretchedLength)
    {
        protected override Vector SpringPosition()
        {
            return origin + Math.Sin(2 * Math.PI * frequency * engine.Time) * amplitude;
        }
    }
}
