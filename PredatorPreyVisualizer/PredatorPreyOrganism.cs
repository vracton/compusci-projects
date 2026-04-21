using Arena;
using DongUtility;
using Geometry.Geometry2D;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// Base class for all predator-prey organisms
    /// </summary>
    public abstract class PredatorPreyOrganism : MovingObject
    {
        public Vector2D Velocity { get; set; } = Vector2D.NullVector();

        public double Width { get; }
        public double Height { get; }

        public double MaxSpeed { get; }
        public double MaxAcceleration { get; }
        public double StepTime { get; }

        public PredatorPreyOrganism(int graphicsCode, double width, double height, double maxSpeed, double maxAccel, double stepTime, Point position) :
            base(graphicsCode, 1, width, height)
        {
            Width = width;
            Height = height;
            MaxSpeed = maxSpeed;
            MaxAcceleration = maxAccel;
            StepTime = stepTime;
            Position = position;
        }

        /// <summary>
        /// A counter to see how long it is until the next decision must be made
        /// </summary>
        private double nextDecisionTime = 0;

        /// <summary>
        /// Logic to be filled in for eating
        /// </summary>
        virtual protected void Eat()
        { }

        /// <summary>
        /// Figuring out what to do next by calling abstract functions
        /// </summary>
        private void ChooseMove()
        {
            var deltaV = ChooseVelocityChange();
            // Make sure it's not too big
            if (deltaV.Magnitude > MaxAcceleration)
                deltaV = deltaV.UnitVector() * MaxAcceleration;

            Velocity += deltaV;

            if (Velocity.Magnitude > MaxSpeed)
                Velocity = Velocity.UnitVector() * MaxSpeed;
        }

        /// <summary>
        /// The code that actually chooses how to change the velocity on each turn
        /// </summary>
        abstract protected Vector2D ChooseVelocityChange();

        private double formerTime = 0;

        protected override bool DoTurn(Turn turn)
        {
            return turn.DoTurn();
        }

        protected override void UserDefinedBeginningOfTurn()
        {
            // Do nothing
        }

        protected override Turn UserDefinedChooseAction()
        {
            double deltaT = Arena.Time - formerTime;

            if (nextDecisionTime <= Arena.Time)
            {
                nextDecisionTime += StepTime;
                ChooseMove();
            }

            var newPosition = Position + Velocity * deltaT;

            // Check to make sure we didn't go out of bounds
            if (newPosition.X < Width / 2)
            {
                newPosition = new Point(Width / 2, newPosition.Y);
                Velocity = new Vector2D(0, Velocity.Y);
            }
            else if (newPosition.X > Arena.Width - Width / 2)
            {
                newPosition = new Point(Arena.Width - Width / 2, newPosition.Y);
                Velocity = new Vector2D(0, Velocity.Y);
            }
            if (newPosition.Y < Height / 2)
            {
                newPosition = new Point(newPosition.X, Height / 2);
                Velocity = new Vector2D(Velocity.X, 0);
            }
            else if (newPosition.Y > Arena.Height - Height / 2)
            {
                newPosition = new Point(newPosition.X, Arena.Height - Height / 2);
                Velocity = new Vector2D(Velocity.X, 0);
            }

            return new Move(this, newPosition);
        }

        internal bool IsDead { get; set; } = false;

        protected override void UserDefinedEndOfTurn()
        {
            Eat();
            formerTime = Arena.Time;
        }

        public override bool IsPassable(ArenaObject? mover = null)
        {
            return true;
        }

        /// <summary>
        /// Gets the closest object of the specified type to the current object
        /// </summary>
        public T? GetClosest<T>() where T : PredatorPreyOrganism
        {
            T? closest = null;
            foreach (var animal in Arena.GetObjectsOfType<T>())
            {
                if (animal == this)
                {
                    continue;
                }
                if (closest == null)
                {
                    closest = animal;
                }
                else
                {
                    if (Point.DistanceSquared(Position, animal.Position) < Point.DistanceSquared(Position, closest.Position))
                    {
                        closest = animal;
                    }
                }
            }
            return closest;
        }
    }
}
