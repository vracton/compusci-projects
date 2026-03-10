using DongUtility;

namespace Geometry.Geometry2D
{
    /// <summary>
    /// Simple two-dimensional point.
    /// Represents a coordinate, not a direction.
    /// </summary>
    public struct Point(double x, double y)
    {
        /// <summary>
        /// Implemented as a vector, since mathematically there is no difference
        /// </summary>
        private Vector2D underlyingData = new(x, y);

        static public Point Origin() => new(0, 0);

        public readonly double X => underlyingData.X;
        public readonly double Y => underlyingData.Y;

        /// <summary>
        /// The distance between two points
        /// </summary>
        static public double Distance(Point p1, Point p2)
        {
            return Vector2D.Distance(p1.underlyingData, p2.underlyingData);
        }

        /// <summary>
        /// The distance squared (for faster computation) between two points
        /// </summary>
        static public double DistanceSquared(Point p1, Point p2)
        {
            return Vector2D.Distance2(p1.underlyingData, p2.underlyingData);
        }

        static public Vector2D operator-(Point p1, Point p2)
        {
            return p1.underlyingData - p2.underlyingData;
        }

        static public Point operator+(Point p1, Vector2D vec)
        {
            return (p1.underlyingData + vec).ToPoint();
        }

        static public Point operator+(Vector2D vec, Point p1)
        {
            return p1 + vec;
        }

        static public bool operator==(Point p1, Point p2)
        {
            return p1.underlyingData == p2.underlyingData;
        }

        static public bool operator!=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Returns a vector pointing from the origin to the point
        /// </summary>
        public readonly Vector2D PositionVector => underlyingData;

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   underlyingData == point.underlyingData;
        }

        public override int GetHashCode()
        {
            return underlyingData.GetHashCode();
        }
    }

    static public class Vector2DExtensions
    {
        static public Point ToPoint(this Vector2D vec)
        {
            return new Point(vec.X, vec.Y);
        }
    }
}
