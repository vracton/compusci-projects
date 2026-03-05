using DongUtility;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A spring force for which the other end of the spring is connected to another projectile
    /// Note that this needs to be set on both projectiles for Newton's third law to work properly
    /// </summary>
    public class ProjectileBoundSpringForce(Projectile projectile1,
        Projectile projectile2, double springConstant,
        double unstretchedLength = 0, double dampingConstant = 0)
        : SpringForce(projectile1, springConstant, unstretchedLength)
    {
        protected override Vector SpringPosition()
        {
            return projectile2.Position;
        }

        protected override Vector GetForce()
        {
            Vector springForce = base.GetForce();
            Vector springDirection = (Particle.Position - projectile2.Position).UnitVector();
            Vector relativeVelocity = Particle.Velocity - projectile2.Velocity;
            double relativeSpeedAlongSpring = Vector.Dot(relativeVelocity, springDirection);
            Vector dampingForce = -dampingConstant * relativeSpeedAlongSpring * springDirection;
            return springForce + dampingForce;
        }
    }
}
