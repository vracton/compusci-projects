
using Geometry.Geometry2D;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A two-dimensional shape in three-dimensional space
    /// NOT THE SAME as Geometry2D.Shape2D!!
    /// Used for objects such as the faces of a polyhedron
    /// </summary>
    public abstract class Shape2D<T>() where T : Geometry2D.Shape2D
    {
        private T? underlyingShape = null;
        public T UnderlyingShape
        {
            get
            {
                underlyingShape ??= CalculateUnderlyingShape();
                return underlyingShape;
            }
        }
        abstract protected T CalculateUnderlyingShape();


        private Plane? containingPlane = null;
        public Plane ContainingPlane 
        {
            get
            {
                containingPlane ??= CalculateContainingPlane();
                return containingPlane;
            }
        }

        abstract protected Plane CalculateContainingPlane();

        /// <summary>
        /// Whether a given line segment intersects the shape
        /// </summary>
        public bool Intersects(LineSegment segment)
        {
            if (!ContainingPlane.Intersects(segment))
            {
                return false;
            }

            return Intersects(segment.UnderlyingLine);
        }

        public bool Intersects(Line line)
        {
            var point = ContainingPlane.Intersection(line);
            if (point == null)
            {
                return false;
            }
            return IsInside(point.Value);
        }

        public Point? Intersection(Line line)
        {
            return ContainingPlane.Intersection(line);
        }

        public Point? Intersection(LineSegment segment)
        {
            return ContainingPlane.Intersection(segment);
        }

        /// <summary>
        /// Returns whether a point known to be in the containing plane lies inside the shape
        /// </summary>
        public bool IsInside(Point point)
        {
            var rotatedPoint = ContainingPlane.TransformTo2D(point);
            return UnderlyingShape.Inside(rotatedPoint);
        }

        /// <summary>
        /// Finds the distance from the shape to a line segment
        /// </summary>
        abstract public double DistanceFrom(LineSegment segment);

        /// Finds the closest point on the shape to a given point in space
        abstract public Point ClosestPoint(Point point);

    }
}
