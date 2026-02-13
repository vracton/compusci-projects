using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A two-dimensional plane in 3D space
    /// </summary>
    public class Plane(Point point, Vector normal)
    {
        // Implemented internally as coeffs.X * x + coeffs.Y * y + coeffs.Z * z + d = 0
        private readonly Vector coeffs = normal;
        private readonly double constant = -Vector.Dot(normal, point.PositionVector());

        public Plane(Point p1, Point p2, Point p3) :
            this(p1, Vector.Cross(p2 - p1, p3 - p1))
        { }

        public Vector Normal => coeffs;

        private Rotation RotationTo2D
        {
            get
            {
                if (rotationTo2D == null)
                {
                    rotationTo2D = new Rotation();
                    rotationTo2D.RotateYAxis(Normal.Polar);
                    rotationTo2D.RotateZAxis(Normal.Azimuthal);
                }
                return rotationTo2D;
            }
        }

        private Rotation rotationTo2D = null;
        private Rotation rotationFrom2D = null;

        private Rotation RotationFrom2D
        {
            get
            {
                if (rotationFrom2D == null)
                {
                    rotationFrom2D = RotationTo2D.Inverse();
                }
                return rotationFrom2D;
            }
        }

        /// <summary>
        /// Transforms the point to 2D in this plane
        /// Note that this assumes that this point is in the plane!
        /// You must check separately to confirm this is the case
        /// </summary>
        public Geometry2D.Point TransformTo2D(Point point)
        {
            var rotatedPoint = RotationTo2D.ApplyRotation(point.PositionVector());
            return new Geometry2D.Point(rotatedPoint.X, rotatedPoint.Y);
        }

        public Geometry2D.Line TransformTo2D(Line line)
        {
            var ray = TransformTo2D(line.UnderlyingRay);
            return ray.UnderlyingLine;
        }

        public Geometry2D.Ray TransformTo2D(Ray ray)
        {
            var point1 = TransformTo2D(ray.EndPoint);
            var point2inSpace = ray.EndPoint.PositionVector()
                + ray.Direction;
            var point2 = TransformTo2D(point2inSpace.ToPoint());

            return new Geometry2D.Ray(point1, point2 - point1);
        }

        public Geometry2D.LineSegment TransformTo2D(LineSegment segment)
        {
            var point1 = TransformTo2D(segment.Point1);
            var point2 = TransformTo2D(segment.Point2);

            return new Geometry2D.LineSegment(point1, point2);
        }

        public bool IsInPlane(Point point)
        {
            return Vector.Dot(coeffs, point.PositionVector()) + constant == 0;
        }

        public bool IsInPlane(Line line)
        {
            return (IsInPlane(line.UnderlyingRay.EndPoint) && Vector.Perpendicular(line.UnderlyingRay.Direction, Normal));
        }

        /// <summary>
        /// Returns the point in the plane closest to the given point
        /// </summary>
        public Point NearestPoint(Point point)
        {
            if (IsInPlane(point))
                return point;

            var connectingLine = new Line(point, Normal);
            return Intersection(connectingLine) ?? throw new GeometryException("Failed to find intersection of point-normal line with plane!");
        }

        /// <summary>
        /// Returns the point in the plane closest to the given line segment
        /// </summary>
        public Point NearestPoint(LineSegment segment)
        {
            if (Intersects(segment))
            {
                return Intersection(segment.UnderlyingLine) ?? throw new GeometryException("Failed to find intersection of line segment with plane!");
            }

            double d1 = Distance2(segment.Point1);
            double d2 = Distance2(segment.Point2);

            if (d1 < d2)
                return NearestPoint(segment.Point1);
            else
                return NearestPoint(segment.Point2);
        }

        /// <summary>
        /// Finds the intersection of the plane with a line, if it exists.
        /// Returns null if there is no intersection or if the line lies in the plane.
        /// </summary>
        public Point? Intersection(Line line)
        {
            // Simple cases first
            if (!Intersects(line))
            {
                return null;
            }
            if (IsInPlane(line))
            {
                return null;
            }

            var parameter = -(Vector.Dot(coeffs, line.UnderlyingRay.EndPoint.PositionVector()) + constant) / Vector.Dot(coeffs, line.UnderlyingRay.Direction);
            return line.UnderlyingRay.EndPoint + parameter * line.UnderlyingRay.Direction;        
        }

        public Point? Intersection(LineSegment segment)
        {
            if (!Intersects(segment))
            {
                return null;
            }
            return Intersection(segment.UnderlyingLine);
        }

        public double Distance2 (Point point)
        {
            return (point - NearestPoint(point)).MagnitudeSquared;
        }

        public double Distance2 (Line line)
        {
            if (Intersects(line))
            {
                return 0;
            }
            else
            {
                return Distance2(line.UnderlyingRay.EndPoint);
            }
        }

        public double Distance2 (Ray ray)
        {
            if (Intersects(ray))
            {
                return 0;
            }
            else
            {
                return Distance2(ray.EndPoint);
            }
        }

        public double Distance2(LineSegment segment)
        {
            if (Intersects(segment))
            {
                return 0;
            }
            else
            {
                double d1Squared = Distance2(segment.Point1);
                double d2Squared = Distance2(segment.Point2);
                return d1Squared < d2Squared ? d1Squared : d2Squared;
            }
        }

        public double Distance(Point point)
        {
            return Math.Sqrt(Distance2(point));
        }

        public double Distance(Line line)
        {
            return Math.Sqrt(Distance2(line));
        }

        public double Distance(Ray ray)
        {
            return Math.Sqrt(Distance2(ray));
        }

        public double Distance(LineSegment segment)
        {
            return Math.Sqrt(Distance2(segment));
        }


        public bool Intersects(Line line)
        {
            return Vector.Dot(line.UnderlyingRay.Direction, coeffs) != 0;
        }

        public bool Intersects(Ray ray)
        {
            if (Intersects(ray.UnderlyingLine))
            {
                // If it points toward the plane, then a point on the ray should be closer than
                // the endpoint
                double endpointDistance2 = Distance2(ray.EndPoint);
                Point newPoint = ray.EndPoint + ray.Direction;
                double otherPointDistance2 = Distance2(newPoint);
                return otherPointDistance2 < endpointDistance2;
            }
            else
            {
                return false;
            }
        }

        public bool Intersects(LineSegment segment)
        {
            return !SameHalfSpace(segment.Point1, segment.Point2);
        }

        /// <summary>
        /// Returns whether two points are in the same half-space from the plane
        /// It should be noted that if either point is in the plane, this returns true
        /// That seems to be preferred behavior for most applications
        /// </summary>
        public bool SameHalfSpace(Point p1, Point p2)
        {
            double score1 = Vector.Dot(coeffs, p1.PositionVector()) + constant;
            double score2 = Vector.Dot(coeffs, p2.PositionVector()) + constant;

            //if (score1 == 0 || score2 == 0)
            //    return false;

            bool space1 = score1 >= 0;
            bool space2 = score2 >= 0;

            return space1 == space2;
        }

        static public bool AreCoplanar(IEnumerable<Point> points)
        {
            if (points.Count() < 4)
                return true;
            var pointArray = points.ToArray();
            var plane = new Plane(pointArray[0], pointArray[1], pointArray[2]);
            for (int i = 3; i < pointArray.Length; i++)
            {
                if (!plane.IsInPlane(pointArray[i]))
                    return false;
            }
            return true;
        }
    }
}
