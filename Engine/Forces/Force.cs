using Helpers;

namespace Engine.Forces
{
    public abstract class Force
    {
        public abstract Vector CalculateForce(Projectile projectile);
    }
}