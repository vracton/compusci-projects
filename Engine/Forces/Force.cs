using Helpers;
using Engine.Core;

namespace Engine.Forces
{
    public abstract class Force
    {
        public abstract Vector CalculateForce(Projectile projectile);
    }
}