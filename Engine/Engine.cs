using Engine.Forces;

namespace Engine
{
    public class Engine
    {
        public double Time { get; private set; }
        public List<Projectile> Projectiles { get; private set; }
        public List<Force> Forces { get; private set; }

        public Engine()
        {
            Time = 0;
            Projectiles = [];
            Forces = [];
        }

        public void AddProjectile(Projectile projectile)
        {
            Projectiles.Add(projectile);
        }

        public void AddForce(Force force)
        {
            Forces.Add(force);
        }

        public void AddProjectiles(Projectile[] projectiles)
        {
            Projectiles.AddRange(projectiles);
        }

        public void AddForces(Force[] forces)
        {
            Forces.AddRange(forces);
        }

        public void Update(double deltaTime)
        {
            foreach (var projectile in Projectiles)
            {
                foreach (var force in Forces)
                {
                    projectile.ApplyForce(force);
                }
            }

            foreach (var projectile in Projectiles)
            {
                if (Time > 0)
                {
                    projectile.Tick(deltaTime);
                }
            }
            Time += deltaTime;
        }
    }
}
