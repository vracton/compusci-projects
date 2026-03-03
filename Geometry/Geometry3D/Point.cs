using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// Simple three-dimensional point.
    /// Represents a coordinate, not a direction.
    /// </summary>
    public struct Point(double x, double y, double z)
    {
        /// <summary>
        /// Implemented as a vector, since mathematically there is no difference
        /// </summary>
        private Vector underlyingData = new(x, y, z);

        static public Point Origin() => new(0, 0, 0);

        public readonly double X => underlyingData.X;
        public readonly double Y => underlyingData.Y;
        public readonly double Z => underlyingData.Z;

        /// <summary>
        /// The distance between two points
        /// </summary>
        static public double Distance(Point p1, Point p2)
        {
            return Vector.Distance(p1.underlyingData, p2.underlyingData);
        }

        /// <summary>
        /// The distance squared (for faster computation) between two points
        /// </summary>
        static public double DistanceSquared(Point p1, Point p2)
        {
            return Vector.Distance2(p1.underlyingData, p2.underlyingData);
        }

        static public Point Midpoint(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
        }

        static public Vector operator-(Point p1, Point p2)
        {
            return p1.underlyingData - p2.underlyingData;
        }

        static public Point operator+(Point p1, Vector vec)
        {
            return (p1.underlyingData + vec).ToPoint();
        }

        static public bool operator==(Point p1, Point p2)
        {
            return p1.X == p2.X
                && p1.Y == p2.Y
                && p1.Z == p2.Z;
        }

        static public bool operator!=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Returns a vector pointing from the origin to the point
        /// </summary>
        public readonly Vector PositionVector()
        {
            return underlyingData;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Point point &&
                   X == point.X &&
                   Y == point.Y &&
                   Z == point.Z;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }

    static public class VectorExtensions
    {
        static public Point ToPoint(this Vector vec)
        {
            return new Point(vec.X, vec.Y, vec.Z);
        }
    }
}
