using Helpers;
using Engine.Core;

namespace Engine.Forces
{
    public class Gravity : Force
    {
        public double g { get; set; }

        public Gravity(double g)
        {
            this.g = g;
        }

        public override Vector CalculateForce(Projectile projectile)
        {
            return new Vector(0, 0, -projectile.Mass * g);
        }
    }
}
