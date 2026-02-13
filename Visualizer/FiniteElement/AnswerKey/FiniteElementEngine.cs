using DongUtility;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualizer.FiniteElement.AnswerKey
{
    internal class FiniteElementEngine : KinematicsEngine
    {
        private ParticleStructure structure;
        private List<Force> shortTermForces = new();

        public FiniteElementEngine(ParticleStructure structure, double dampingCoefficient)
        {
            this.structure = structure;

            foreach (var particle in structure.Projectiles)
            {
                AddProjectile(particle);
            }

            foreach (var connector in structure.Connectors)
            {
                shortTermForces.Add(new ProjectileBoundSpringForce(connector.Projectile1, connector.Projectile2, connector.SpringConstant, connector.UnstretchedLength));
                shortTermForces.Add(new ProjectileBoundSpringForce(connector.Projectile2, connector.Projectile1, connector.SpringConstant, connector.UnstretchedLength));

                // For a check
                //AddForce(new ProjectileBoundSpringForce(connector.Projectile1, connector.Projectile2, connector.SpringConstant, connector.UnstretchedLength));
                //AddForce(new ProjectileBoundSpringForce(connector.Projectile2, connector.Projectile1, connector.SpringConstant, connector.UnstretchedLength));
            }

            shortTermForces.Add(new AirResistanceForce(this, dampingCoefficient));
        }

        /// <summary>
        /// The time increment of the short spring section
        /// </summary>
        public double ShortTimeIncrement { get; set; } = 1e-7;

        /// <summary>
        /// The maximum spring force at which point the short spring simulation stops
        /// </summary>
        public double ForceThreshold { get; set; } = 1000;

        public override bool Increment(double timeIncrement)
        {
            var response = base.Increment(timeIncrement);
            //return response;
            double maxForce;

            // Run the short spring simulation
            //do
            for (int i = 0; i < 10000; ++i)
            {

                // Add the forces
#if PARALLEL
                Parallel.ForEach(shortTermForces, force =>
#else
                foreach (var force in shortTermForces)
#endif
                {
                    force.AddForce(i);
                }
#if PARALLEL
);
#endif
                maxForce = MaxForce();

                // Move the particles and apply damping
#if PARALLEL
                Parallel.ForEach(structure.Projectiles, projectile =>
#else
                foreach (var projectile in structure.Projectiles)
#endif
                {
                    projectile.Update(ShortTimeIncrement);
                }
#if PARALLEL
);
#endif
            }// while (maxForce > ForceThreshold);
            return response;
        }

        private static double MaxLengthDeviation(ParticleStructure structure)
        {
            double max = structure.Connectors.Max(p => p.CurrentLength / p.UnstretchedLength);
            double min = structure.Connectors.Min(p => p.CurrentLength / p.UnstretchedLength);

            return Math.Max(max - 1, 1 - min);
        }

        private double MaxForce()
        {
            double max = structure.Projectiles.Max(p => p.NetForce.Magnitude);
            return max;
        }
    }
}
