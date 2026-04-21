using DongUtility;
using Geometry.Geometry2D;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// A simple lynx class that uses a simple rule to determine its movement
    /// </summary>
    class Lynx : PredatorPreyOrganism
    {
        internal const double MyWidth = .3;
        internal const double MyLength = 1;
        private const double myMaxSpeed = 12;
        private const double myStepTime = .3;
        private const double myMaxAccel = 8;
        private const double initialx = 25;
        private const double initialy = 40;
        private const int graphicsCode = 2;

        public override string Name => "Lynx";

        public Lynx() :
            base(graphicsCode, MyWidth, MyLength, myMaxSpeed, myMaxAccel, myStepTime, new Point(initialx, initialy))
        { }

        protected override Vector2D ChooseVelocityChange()
        {
            Hare closestHare = GetClosest<Hare>();

            Vector2D diff = closestHare.Position - Position;

            return diff.UnitVector() * myMaxAccel;
        }

        protected override void Eat()
        {
            foreach (var hare in Arena.GetObjectsOfType<Hare>())
            {
                if (Overlaps(hare))
                {
                    hare.IsDead = true;
                }
            }
        }
    }
}
