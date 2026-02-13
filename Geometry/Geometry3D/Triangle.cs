using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A triangle in three dimensions
    /// </summary>
    public class Triangle(IEnumerable<Point> points) : Polygon(points)
    {
        public Triangle(Point p1, Point p2, Point p3) : 
            this([p1, p2, p3])
        {}

        /// <summary>
        /// Finds the distance from the triangle to a line segment
        /// </summary>
        override public double DistanceFrom(LineSegment segment)
        {
            if (Intersects(segment))
                return 0;

            var nearestPointInPlane = ContainingPlane.NearestPoint(segment);
            if (IsInside(nearestPointInPlane))
            {
                return ContainingPlane.Distance(segment);
            }

            var line1 = new LineSegment(Vertices[0], Vertices[1]);
            var line2 = new LineSegment(Vertices[1], Vertices[2]);
            var line3 = new LineSegment(Vertices[2], Vertices[0]);

            double p1 = segment.DistanceSquared(line1);
            double p2 = segment.DistanceSquared(line2);
            double p3 = segment.DistanceSquared(line3);

            double minDistance2 = UtilityFunctions.Min(p1, p2, p3);

            return Math.Sqrt(minDistance2);
        }
    }
}
