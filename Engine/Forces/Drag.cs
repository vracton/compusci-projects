using Helpers;

namespace Engine.Forces
{
    public class Drag : Force
    {
        public double C { get; set; }

        public Drag(double C)
        {
            this.C = C;
        }

        public override Vector CalculateForce(Projectile projectile)
        {
            return -C * projectile.Velocity.Magnitude * projectile.Velocity;
        }
    }
}
