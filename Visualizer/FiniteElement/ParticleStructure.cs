using DongUtility;
using PhysicsUtility.Kinematics;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Visualizer.FiniteElement
{
    /// <summary>
    /// A structure of projectiles and springs that connect them together
    /// </summary>
    public class ParticleStructure
    { 

        public List<Projectile> Projectiles { get; } = [];
        public List<Connector> Connectors { get; } = [];

        public Vector CenterOfMass
        {
            get
            {
                Vector sum = new();
                double tMass = 0;
                foreach (Projectile p in Projectiles)
                {
                    sum += p.Position * p.Mass;
                    tMass += p.Mass;
                }
                return sum / tMass;
            }
        }

        public Vector VelocityOfCOM
        {
            get
            {
                Vector sum = new();
                double tMass = 0;
                foreach (Projectile p in Projectiles)
                {
                    sum += p.Velocity * p.Mass;
                    tMass += p.Mass;
                }
                return sum / tMass;
            }
        }

        public Vector AccelOfCOM
        {
            get
            {
                Vector sum = new();
                double tMass = 0;
                foreach (Projectile p in Projectiles)
                {
                    sum += p.Acceleration * p.Mass;
                    tMass += p.Mass;
                }
                return sum / tMass;
            }
        }

        /// <summary>
        /// Find the index of a given projectile
        /// </summary>
        public int GetIndex(Projectile projectile)
        {
            return Projectiles.IndexOf(projectile);
        }
        
        /// <summary>
        /// Gets the indices of the projectiles connected to a connector
        /// </summary>
        public Tuple<int, int> GetIndexOfProjectiles(Connector connector)
        {
            return new Tuple<int, int>(GetIndex(connector.Projectile1), GetIndex(connector.Projectile2));
        }

        /// <summary>
        /// A class representing a spring connecting two projectiles
        /// </summary>
        /// <param name="springConstant">In kg/s^2</param>
        /// <param name="unstretchedLength">The unstretched length of the spring</param>
        public class Connector(Projectile proj1, Projectile proj2, double springConstant, double unstretchedLength)
        {
            public Color Color { get; set; } = Colors.Orange;

            public Projectile Projectile1 { get; set; } = proj1;
            public Projectile Projectile2 { get; set; } = proj2;

            public double UnstretchedLength { get; set; } = unstretchedLength == 0 ? unstretchedLength : (proj1.Position - proj2.Position).Magnitude;
            public double SpringConstant { get; } = springConstant;
            public double CurrentLength => Vector.Distance(Projectile1.Position, Projectile2.Position);
            /// <summary>
            /// Magnitude of the force on the spring
            /// </summary>
            public double Force => Math.Abs(SpringConstant * (CurrentLength - UnstretchedLength));
        }

        /// <summary>
        ///  Add a projectile to the structure
        /// </summary>
        public void AddProjectile(Projectile proj)
        {
            if (proj.Mass == 0)
            {
                throw new Exception("Mass cannot be zero!");
            }
            Projectiles.Add(proj);
        }

        /// <summary>
        ///  Add a projectile to the structure
        /// </summary>
        public void AddProjectile(Vector position, double mass)
        {
            if (mass == 0)
            {
                throw new Exception("Mass cannot be zero!");
            }
            var proj = new Projectile(position, Vector.NullVector(), mass);
            AddProjectile(proj);
        }

        /// <summary>
        /// Add a new connector to the structure
        /// </summary>
        public void AddConnector(Projectile proj1, Projectile proj2, double springConstant, double unstretchedLength)
        {
            if (proj1 == proj2)
            {
                return;
            }

            // Check to make sure connectors do not already exist
            foreach (var con in Connectors)
            {
                if ((con.Projectile1 == proj1 && con.Projectile2 == proj2) || (con.Projectile1 == proj2 && con.Projectile2 == proj1))
                {
                    return; // This returns without error because I suspect people would like that behavior
                }
            }
            Connectors.Add(new Connector(proj1, proj2, springConstant, unstretchedLength));
        }
    }
}
