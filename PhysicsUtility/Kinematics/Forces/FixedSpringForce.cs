using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A spring force for which one end is fixed at a given position
    /// </summary>
    public class FixedSpringForce(Projectile projectile, double springConstant,
        Vector position, double unstretchedLength = 0) 
        : SpringForce(projectile, springConstant, unstretchedLength)
    {
        protected override Vector SpringPosition()
        {
            return position;
        }
    }
}
