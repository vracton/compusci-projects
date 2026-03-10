using DongUtility;
using Path = DongUtility.Path;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A projectile that is constrained to move along a given path
    /// </summary>
    public class ConstrainedProjectile : Projectile
    {
        /// <summary>
        /// The path to which the projectile is constrained
        /// </summary>
        public Path Path { get; set; }
        /// <summary>
        /// The parameter for the parametric equation of the path
        /// </summary>
        public double Parameter { get; set; }

        public ConstrainedProjectile(Vector position, Vector velocity, double mass, Path path) : base(position, velocity, mass)
        {
            Path = path;
            var closestParameter = Path.GetClosestParameter(Position);
            Position = Path.GetPosition(closestParameter);
            Parameter = closestParameter;
            Velocity = Path.Project(velocity, Parameter);
        }

        public ConstrainedProjectile(Projectile proj, Path path) :
            this(proj.Position, proj.Velocity, proj.Mass, path)
        { }

        static public double Tolerance { get; set; } = .001;

        public override void Update(double timeIncrement)
        {
            // Get acceleration but project along path
            UpdateAcceleration();
            Acceleration = Path.Project(Acceleration, Parameter);
            NetForce = Vector.NullVector(); // Reset forces

            // Get velocity but project along path
            UpdateVelocity(timeIncrement);
            Velocity = Path.Project(Velocity, Parameter);

            // Find new parameter using chain rule
            // v = dx/dt = dx/dq * dq/dt for the parameter q
            // v * dx/dq = (dx/dq)^2 * dq/dt
            // dq/dt = v * dx/dq / (dx/dq)^2
            var pathDerivative = Path.Derivative(Parameter);
            var parameterDerivative = Vector.Dot(Velocity, pathDerivative) / pathDerivative.MagnitudeSquared;
            var newParameter = Parameter + parameterDerivative * timeIncrement;

            // Check the new position to make sure it isn't illegal
            var newPosition = Path.GetPosition(newParameter);
            double distanceSquared = Vector.Distance2(newPosition, Position);
            double classicalDistanceSquared = (Velocity * timeIncrement).MagnitudeSquared;

            int numberOfTries = 0;
            const int maxTries = 10;
            while (classicalDistanceSquared - distanceSquared + Tolerance < 0)
            {
                // Try a parameter that is smaller by the difference
                double proportion = classicalDistanceSquared / distanceSquared;
                newParameter = Parameter + (parameterDerivative * timeIncrement) * proportion;
                newPosition = Path.GetPosition(newParameter);
                distanceSquared = Vector.Distance2(newPosition, Position);
                classicalDistanceSquared = (Velocity * timeIncrement).MagnitudeSquared;

                // Stop if you have tried too many times
                if (numberOfTries++ > maxTries)
                {
                    newParameter = Parameter;
                    newPosition = Position;
                    Velocity = Vector.NullVector();
                    break;
                }
            }

            // Update the position based on the parameter
            Parameter = newParameter;
            Position = newPosition;
        }
    }
}
