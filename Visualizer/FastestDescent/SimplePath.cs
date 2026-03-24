using DongUtility;

namespace Visualizer.FastestDescent
{
    abstract public class SimplePath(Vector point1, Vector point2) : Path
    {
        protected Vector Point1 { get; } = point1;
        protected Vector Point2 { get; } = point2;

        public override double InitialParameter => 0;
        public override double FinalParameter => 1;
    }
}
