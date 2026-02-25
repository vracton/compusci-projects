using DongUtility;
using PhysicsUtility.Kinematics;
using System;

namespace Visualizer.FiniteElement
{
    public class SpringSettlingKinematicsEngine : KinematicsEngine
    {
        //number of internal integration steps used for each external Increment() call
        public int RelaxationSubsteps { get; set; } = 12;
        //exponential damping rate in 1/s applied to velocity relative to COM
        public double VibrationDampingRate { get; set; } = 60.0;
        public double InternalKineticEnergy { get; private set; } = 0.0;
        public double LastDampingFactor { get; private set; } = 1.0;

        //for each normal increment, we copmute substeps and damp at each one
        public override bool Increment(double timeIncrement)
        {
            int substeps = Math.Max(1, RelaxationSubsteps);
            double substepTimeIncrement = timeIncrement / substeps;

            bool keepGoing = true;
            for (int i = 0; i < substeps && keepGoing; ++i)
            {
                keepGoing = base.Increment(substepTimeIncrement);
                ApplyVibrationDamping(substepTimeIncrement);
            }

            InternalKineticEnergy = ComputeInternalKineticEnergy();
            return keepGoing;
        }

        //apply exponential damping relative to COM
        private void ApplyVibrationDamping(double deltaTime)
        {
            if (Projectiles.Count == 0 || VibrationDampingRate <= 0)
            {
                LastDampingFactor = 1.0;
                return;
            }

            double dampingFactor = Math.Exp(-VibrationDampingRate * deltaTime);
            LastDampingFactor = dampingFactor;

            Vector centerOfMassVelocity = Vector.NullVector();
            double totalMass = 0.0;
            foreach (var projectile in Projectiles)
            {
                centerOfMassVelocity += projectile.Mass * projectile.Velocity;
                totalMass += projectile.Mass;
            }

            if (totalMass <= 0.0)
            {
                return;
            }

            centerOfMassVelocity /= totalMass;
            foreach (var projectile in Projectiles)
            {
                Vector relativeVelocity = projectile.Velocity - centerOfMassVelocity;
                projectile.Velocity = centerOfMassVelocity + relativeVelocity * dampingFactor;
            }
        }

        //for graphs, compute by summing kinetic energy of each projectile
        private double ComputeInternalKineticEnergy()
        {
            if (Projectiles.Count == 0)
            {
                return 0.0;
            }

            Vector centerOfMassVelocity = Vector.NullVector();
            double totalMass = 0.0;
            foreach (var projectile in Projectiles)
            {
                centerOfMassVelocity += projectile.Mass * projectile.Velocity;
                totalMass += projectile.Mass;
            }

            if (totalMass <= 0.0)
            {
                return 0.0;
            }

            centerOfMassVelocity /= totalMass;

            double response = 0.0;
            foreach (var projectile in Projectiles)
            {
                Vector relativeVelocity = projectile.Velocity - centerOfMassVelocity;
                response += 0.5 * projectile.Mass * relativeVelocity.MagnitudeSquared;
            }

            return response;
        }
    }
}
