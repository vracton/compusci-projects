using Forces;

namespace Helpers
{
    public class Projectile
    {
        public double Mass { get; init; }
        public Vector Position { get; set; }
        public Vector Velocity { get; set; }
        public Vector Acceleration { get; set; }

        public Projectile(double mass, Vector position, Vector velocity, Vector acceleration)
        {
            Mass = mass;
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
        }

        public Projectile(double mass, Vector position, Vector velocity)
            : this(mass, position, velocity, new Vector()) { }

        public Projectile(double mass)
            : this(mass, new Vector(), new Vector(), new Vector()) { }

        public void ApplyForce(Force force)
        {
            Acceleration += force.CalculateForce(this);
        }

        public void Tick(double dt)
        {
            Velocity += Acceleration * dt;
            Position += Velocity * dt;
            Acceleration = new Vector();
        }
    }
}
