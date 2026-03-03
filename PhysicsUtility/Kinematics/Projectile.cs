using DongUtility;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A basic projectile which can have forces applied to it
    /// </summary>
    public class Projectile(Vector position, Vector velocity, double mass)
    {
        public Vector Position { get; set; } = position;
        public Vector Velocity { get; set; } = velocity;
        public Vector Acceleration { get; protected set; } = Vector.NullVector();
        public Vector NetForce { get; protected set; } = Vector.NullVector();

        public double Mass
        {
            get
            {
                return mass;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Mass", "Mass cannot be zero or negative!");
                }
                else
                {
                    mass = value;
                }
            }
        }

        public Vector Momentum => mass * Velocity;

        public double KineticEnergy => .5 * mass * Velocity.MagnitudeSquared;

        override public string ToString()
        {
            return $"{Position.PrintWithTabs()}\t{Velocity.PrintWithTabs()}\t{Acceleration.PrintWithTabs()}";
        }

        protected void UpdateAcceleration()
        {
            Acceleration = NetForce / mass;
        }

        protected void UpdateVelocity(double timeIncrement)
        {
            Velocity += Acceleration * timeIncrement;
        }

        protected void UpdatePosition(double timeIncrement)
        {
            Position += Velocity * timeIncrement + .5 * Acceleration * UtilityFunctions.Square(timeIncrement);
        }

        private static readonly Lock forceLocker = new();

        /// <summary>
        /// Adds a force to the particle.
        /// This lasts only until the next time Update() is called
        /// </summary>
        public void AddForce(Vector force)
        {
            // Add locking to allow concurrency
            lock (forceLocker)
            {
                NetForce += force;
            }
        }

        /// <summary>
        /// Updates the position, velocity, and acceleration of the particle
        /// </summary>
        virtual public void Update(double timeIncrement)
        {
            UpdateAcceleration();
            NetForce = Vector.NullVector(); // Reset forces
            UpdatePosition(timeIncrement);
            UpdateVelocity(timeIncrement);
        }
    }
}
