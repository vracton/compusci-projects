using Helpers;
using Engine.Core;

namespace Engine.Forces
{
    public class Spring : Force
    {
        public Vector Anchor { get; set; }
        public double k { get; set; }
        public double RestLength { get; set; }

        public Spring(Vector anchor, double k, double restLength)
        {
            Anchor = anchor;
            this.k = k;
            RestLength = restLength;
        }

        public override Vector CalculateForce(Projectile projectile)
        {
            Vector displacement = Anchor - projectile.Position;
            return -k * (displacement.Magnitude - RestLength) * projectile.Position.UnitVector;
        }
    }
}
