using DongUtility;

namespace Visualizer.FastestDescent
{
    public class SimpleLinearPath(Vector point1, Vector point2) : SimplePath(point1, point2)
    {
        private readonly double distance = Vector.Distance(point1, point2);
        private Vector displacement = (point2 - point1).UnitVector();

        protected override Vector Function(double parameter)
        {
            return Point1 + displacement * parameter * distance;
        }
    }
}
