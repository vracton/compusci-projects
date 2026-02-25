using PhysicsUtility.Kinematics;
using System.Collections.Generic;
using System.Windows.Media;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// An adapter class to get KinematicsEngine to fit IEngine
    /// </summary>
    class EngineAdapter(KinematicsEngine engine) : IEngine
    {
        protected KinematicsEngine Engine { get; } = engine;
        public double ParticleSize { get; set; } = .5;
        //public Color ParticleColor { get; set; } = Colors.Tomato;

        public double Time => Engine.Time;

        virtual public List<IProjectile> Projectiles
        {
            get
            {
                var list = new List<IProjectile>();

                foreach (var proj in Engine.Projectiles)
                {
                    var newIProjectile = new ProjectileAdapter(proj, ParticleSize);
              //      {
            //            Color = ParticleColor
          //          };
                    list.Add(newIProjectile);
                    if (ProjectileMap.Count == 0)
                    {
                        ProjectileMap.Add(proj, newIProjectile);
                    }
                }

                return list;
            }
        }

        protected Dictionary<Projectile, IProjectile> ProjectileMap { get; } = [];

        // Allows us to find the IProjectile that matches a Projectile
        public IProjectile GetProjectile(Projectile projectile)
        {
            return ProjectileMap[projectile];
        }

        public bool Tick(double newTime)
        {
            double increment = newTime - Engine.Time;
            return Engine.Increment(increment);
        }
    }
}
