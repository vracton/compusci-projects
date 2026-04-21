using DongUtility;

namespace Geometry.Geometry2D
{
    /// <summary>
    /// A closed two-dimensional shape
    /// </summary>
    public abstract class Shape2D
    {
        abstract public double Area { get; }

        abstract public double Perimeter { get; }

        abstract public RangePair Range { get; }

        /// <summary>
        /// The maximum radius of the shape from the center
        /// </summary>
        abstract public double MaxRadius { get; }

        /// <summary>
        /// Translate by a given vector
        /// </summary>
        abstract public Shape2D Translate(Vector2D vector);

        /// <summary>
        /// Translate so the center is at a given point
        /// </summary>
        public Shape2D TranslateToPoint(Point point)
        {
            return Translate(point - Center);
        }

        /// <summary>
        /// Rotate by a certain angle about a given point, returning a new shape
        /// </summary>
        abstract public Shape2D Rotate(double angle, Point centerOfRotation);

        /// <summary>
        /// The center of the object. Can be defined however you want, as long as it is consistent.
        /// </summary>
        abstract public Point Center { get; set; }

        /// <summary>
        /// Returns whether a given point is inside the shape
        /// </summary>
        abstract public bool Inside(Point point);

        /// <summary>
        /// Returns the point on the shape boundary that is closest to the given point
        /// </summary>
        abstract public Point ClosestPoint(Point point);

        /// <summary>
        /// Returns the distance squared (for performance) from a given point to the nearest point on the shape boundary.
        /// </summary>
        virtual public double DistanceSquared(Point point)
        {
            Vector2D difference = ClosestPoint(point) - point;
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
        /// Returns the portion that intersects the shape
        /// </summary>
        abstract public LineSegment? Overlap(LineSegment lineSegment);

        /// <summary>
        /// Determines whether two shapes intersect
        /// </summary>
        public bool Intersects(Shape2D other)
        {
            // This is very sad

            if (other is Triangle)
            {
                return Intersects(other as Triangle);
            }
            else if (other is AlignedRectangle)
            {
                return Intersects(other as AlignedRectangle);
            }
            else if (other is Polygon)
            {
                return Intersects(other as Polygon);
            }
            else
            {
                throw new ArgumentException("Shape2D.Intersects() does not support this type of shape");
            }
        }

        // This part here is double-dispatch
        // I decided it was the least bad of the options available
        abstract protected bool Intersects(Triangle other);
        abstract protected bool Intersects(AlignedRectangle other);
        abstract protected bool Intersects(Polygon other);

        /// <summary>
        /// Determines whether a given shape is completely contained within this shape
        /// </summary>
        public bool Contains(Shape2D other)
        {
            // Here follows among the most shameful code of my career

            if (other is Triangle)
            {
                return Contains(other as Triangle);
            }
            else if (other is AlignedRectangle)
            {
                return Contains(other as AlignedRectangle);
            }
            else if (other is Polygon)
            {
                return Contains(other as Polygon);
            }
            else
            {
                throw new ArgumentException("Shape2D.Contains() does not support this type of shape");
            }

            // Fortunately (or unfortunately?) GitHub Copilot wrote it all for me in one click
        }

        // More double-dispatch!  Yay!
        abstract protected bool Contains(Triangle other);
        abstract protected bool Contains(AlignedRectangle other);
        abstract protected bool Contains(Polygon other);
    }
}
