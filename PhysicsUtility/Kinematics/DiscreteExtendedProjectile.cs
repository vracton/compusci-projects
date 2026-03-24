using DongUtility;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A class that represents a projectile that is made up of multiple projectiles
    /// </summary>
    public class DiscreteExtendedProjectile : ExtendedProjectile
    {
        public List<Projectile> Projectiles { get; set; } = [];

        public DiscreteExtendedProjectile(List<Projectile> projectiles) : base(CalcCenterOfMass(projectiles), CMVelocity(projectiles), TotalMass(projectiles))
        {
            Projectiles = projectiles;
            var inertialTensor = new Matrix(3, 3);
            var centerOfMass = CenterOfMass;
            // Calculation from https://en.wikipedia.org/wiki/Moment_of_inertia#Inertia_tensor
            foreach (var projectile in projectiles)
            {
                var positionMatrix = new Matrix(projectile.Position - centerOfMass);
                var tensor = projectile.Mass * (positionMatrix * positionMatrix.Transpose() - projectile.Position.MagnitudeSquared * Rotation.Identity.Matrix);
                inertialTensor += tensor;
            }

            inertialTensorInverse = inertialTensor.Inverse();
        }

        public override void Update(double timeIncrement)
        {
            var oldPosition = Position;
            var oldOrientation = Orientation;
            base.Update(timeIncrement);

            var deltaPosition = Position - oldPosition;
            var newCenterOfMass = CenterOfMass;
            // Rotate and move constituent particles;
            foreach (var projectile in Projectiles)
            {
                // Unrotate to get original position, then rotate back
                var relativePosition = projectile.Position - newCenterOfMass;
                var unrotated = oldOrientation.Inverse().ApplyRotation(relativePosition);
                Orientation.ApplyRotation(unrotated);
                relativePosition += deltaPosition;
                projectile.Position = relativePosition + newCenterOfMass;
            }

        }

        private static double TotalMass(List<Projectile> projectiles)
        {
            return projectiles.Sum(x => x.Mass);
        }

        private static Vector CMVelocity(List<Projectile> projectiles)
        {
            Vector centerOfVelocity = Vector.NullVector();
            foreach (var projectile in projectiles)
            {
                centerOfVelocity += projectile.Mass * projectile.Velocity;
            }
            return centerOfVelocity / TotalMass(projectiles);
        }

        private static Vector CalcCenterOfMass(List<Projectile> projectiles)
        {
            Vector centerOfMass = Vector.NullVector();
            foreach (var projectile in projectiles)
            {
                centerOfMass += projectile.Mass * projectile.Position;
            }
            return centerOfMass / TotalMass(projectiles);
        }

        private readonly Matrix inertialTensorInverse;
        public override Matrix InertialTensorInverse => inertialTensorInverse;

        public override Vector CenterOfMass => CalcCenterOfMass(Projectiles);
    }
}
