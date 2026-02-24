using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A closed three-dimensional shape
    /// </summary>
    public abstract class Shape3D
    {
        abstract public Shape3D Clone();
        abstract public double Volume { get; }

        abstract public double SurfaceArea { get; }

        /// <summary>
        /// The inertial tensor of the shape, assuming a uniform density
        /// </summary>
        public abstract Matrix InertialTensor { get; }

        /// <summary>
        /// The maximum distance of the shape from the center
        /// </summary>
        abstract public double MaxRadius { get; }

        /// <summary>
        /// Translate by a given vector
        /// </summary>
        abstract public Shape3D Translate(Vector vector);

        /// <summary>
        /// Translate so the center is at a given point
        /// </summary>
        public Shape3D TranslateToPoint(Point point)
        {
            return Translate(point - Center);
        }

        /// <summary>
        /// The center of the object. Can be defined however you want, as long as it is consistent.
        /// </summary>
        public Point Center 
        { 
            get
            {
                return GetCenter();
            }
            set
            {
                TranslateToPoint(value);
            }
        }

        abstract protected Point GetCenter();

        /// <summary>
        /// The center of mass of the shape, assuming constant density (centroid)
        /// </summary>
        abstract public Point CenterOfMass { get; }

        /// <summary>
        /// Returns whether a given point is inside the shape
        /// </summary>
        abstract public bool IsInside(Point point);

        /// <summary>
        /// Returns the point on the shape boundary that is closest to the given point
        /// </summary>
        abstract public Point ClosestPoint(Point point);

        /// <summary>
        /// Returns the distance squared (for performance) from a given point to the nearest point on the shape boundary.
        /// </summary>
        virtual public double DistanceSquared(Point point)
        {
            Vector difference = ClosestPoint(point) - point;
            return difference.MagnitudeSquared;
        }

        /// <summary>
        /// Returns the distance from a given point to the nearest point on the shape boundary.
        /// </summary>
        virtual public double Distance(Point point)
        {
            return Math.Sqrt(DistanceSquared(point));
        }

        /// <summary>
        /// Returns the portion of a line segment that intersects the shape
        /// </summary>
        abstract public LineSegment? Overlap(LineSegment lineSegment);

        /// <summary>
        /// Determines whether two shapes intersect
        /// </summary>
        public bool Intersects(Shape3D other)
        {
            // This is very sad

            throw new NotImplementedException();
        }


        /// <summary>
        /// Determines whether a given shape is completely contained within this shape
        /// </summary>
        public bool Contains(Shape3D other)
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all points of intersection between the shape and a line
        /// </summary>
        public abstract IEnumerable<Point> Intersection(Line line);
        public abstract IEnumerable<Point> Intersection(LineSegment segment);
    }
}
