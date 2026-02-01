using Engine.Forces;
using System.Runtime.CompilerServices;

namespace Engine.Core
{
    public class World
    {
        public double Time { get; private set; }
        public List<Projectile> Projectiles { get; private set; }
        public List<Force> Forces { get; private set; }

        public World()
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

        public void AddProjectiles(params Projectile[] projectiles)
        {
            foreach (var projectile in projectiles)
            {
                this.AddProjectile(projectile);
            }
        }

        public void AddForces(params Force[] forces)
        {
            foreach (var force in forces)
            {
                this.AddForce(force);
            }
        }

        public void Tick(double deltaTime, Action? logFunc = null)
        {
            foreach (var projectile in Projectiles)
            {
                foreach (var force in Forces)
                {
                    projectile.ApplyForce(force);
                }
            }

            logFunc?.Invoke();

            foreach (var projectile in Projectiles)
            {
                projectile.Tick(deltaTime);
            }
            Time += deltaTime;
        }
    }
}
