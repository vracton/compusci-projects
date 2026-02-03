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

        //multi adds call normal add methods in case we need to do any processing in the future
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
            //calculate all forces first and then update projectiles simultaneously
            foreach (var projectile in Projectiles)
            {
                foreach (var force in Forces)
                {
                    projectile.ApplyForce(force);
                }
            }

            logFunc?.Invoke(); //log after forces applied before projectiles updated

            foreach (var projectile in Projectiles)
            {
                projectile.Tick(deltaTime);
            }

            Time += deltaTime;
            Forces.Clear();
        }
    }
}
