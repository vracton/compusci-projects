using DongUtility;

namespace Geometry.Geometry2D
{
    /// <summary>
    /// A rectangle whose sides are parallel to the x and y axes
    /// </summary>
    /// <remarks>
    /// Construced by the upper and lower points of the rectangle
    /// </remarks>
    public class AlignedRectangle(Point upperLeft, Point lowerRight) : Polygon([upperLeft, 
        new Point(lowerRight.X, upperLeft.Y), lowerRight, new Point(upperLeft.X, lowerRight.Y)])
    {
        /// <summary>
        /// The four corners of the rectangle
        /// </summary>
        public Point UpperLeft { get; private set; } = upperLeft;
        public Point LowerRight { get; private set; } = lowerRight;
        public Point UpperRight => new(LowerRight.X, UpperLeft.Y);
        public Point LowerLeft => new(UpperLeft.X, LowerRight.Y);

        public double MinY => UpperLeft.Y;
        public double MaxY => LowerRight.Y;
        public double MinX => UpperLeft.X;
        public double MaxX => LowerRight.X;

        // The corners of the rectangle

        public Vector2D MinXMinY => new(MinX, MinY);
        public Vector2D MaxXMinY => new(MaxX, MinY);
        public Vector2D MinXMaxY => new(MinX, MaxY);
        public Vector2D MaxXMaxY => new(MaxX, MaxY);

        // The contour of the rectangle
        public LineSegment Top => new(UpperLeft, UpperRight);
        public LineSegment Bottom => new(LowerLeft, LowerRight);
        public LineSegment Left => new(UpperLeft, LowerLeft);
        public LineSegment Right => new(UpperRight, LowerRight);

        /// <summary>
        /// Can also construct by the center and the width and height
        /// </summary>
        public AlignedRectangle(Point center, double width, double height) :
            this(new Point(center.X - width / 2, center.Y - height / 2), new Point(center.X + width / 2, center.Y + height / 2))
        {
        }

        public double Width => LowerRight.X - UpperLeft.X;
        public double Height => LowerRight.Y - UpperLeft.Y;

        public override double Area => Width * Height;

        public override double Perimeter => 2 * (Width + Height);

        public override Point Center
        {
            get
            {
                return new Point((LowerRight.X + UpperLeft.X) / 2, (LowerRight.Y + UpperLeft.Y) / 2);
            }
            set
            {
                var difference = value - Center;
                UpperLeft += difference;
                LowerRight += difference;
            }
        }

        public override RangePair Range => new(UpperLeft.X, UpperLeft.Y, LowerRight.X, LowerRight.Y);

        public override double MaxRadius => Math.Sqrt(Point.Distance(UpperLeft, LowerRight)) / 2;

        public override List<Point> Vertices => [UpperLeft, UpperRight, LowerRight, LowerLeft];

        public override Point ClosestPoint(Point point)
        {
            var top = Top.NearestPoint(point);
            var bottom = Bottom.NearestPoint(point);
            var left = Left.NearestPoint(point);
            var right = Right.NearestPoint(point);

            double distanceTop = Point.DistanceSquared(point, top);
            double distanceBottom = Point.DistanceSquared(point, bottom);
            double distanceLeft = Point.DistanceSquared(point, left);
            double distanceRight = Point.DistanceSquared(point, right);

            double minDistance = Math.Min(distanceTop, Math.Min(distanceBottom, Math.Min(distanceLeft, distanceRight)));
            if (minDistance == distanceTop)
            {
                return top;
            }
            else if (minDistance == distanceBottom)
            {
                return bottom;
            }
            else if (minDistance == distanceLeft)
            {
                return left;
            }
            else
            {
                return right;
            }
        }

        public override bool Inside(Point point)
        {
            return UtilityFunctions.Between(point.X, UpperLeft.X, LowerRight.X)
                && UtilityFunctions.Between(point.Y, LowerRight.Y, UpperLeft.Y);
        }

        /// <summary>
        /// Returns whether two rectangles overlap
        /// </summary>
        public bool Overlaps(AlignedRectangle other)
        {
            // Either one edge or the other of Other is in this range, or the entirety (and thus the center) of this range lies in Other
            return OverlapsX(other) && OverlapsY(other);
        }

        /// <summary>
        /// Returns whether two rectangles overlap in the x direction
        /// </summary>
        public bool OverlapsX(AlignedRectangle other)
        {
            return UtilityFunctions.Between(other.MinX, MinX, MaxX) || UtilityFunctions.Between(other.MaxX, MinX, MaxX) || UtilityFunctions.Between(Center.X, other.MinX, other.MaxX);
        }

        /// <summary>
        /// Returns whether two rectangles overlap in the y direction
        /// </summary>
        public bool OverlapsY(AlignedRectangle other)
        {
            return UtilityFunctions.Between(other.MinY, MinY, MaxY) || UtilityFunctions.Between(other.MaxY, MinY, MaxY) || UtilityFunctions.Between(Center.Y, other.MinY, other.MaxY);
        }

        public override bool Equals(object? obj)
        {
            return obj is AlignedRectangle rectangle && Equals(rectangle);
        }

        public bool Equals(AlignedRectangle other)
        {
            return EqualityComparer<Point>.Default.Equals(Center, other.Center) &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Center, Width, Height);
        }

        public override Shape2D Translate(Vector2D vector)
        {
            return new AlignedRectangle(UpperLeft + vector, LowerRight + vector);
        }

        protected override bool Intersects(Triangle other)
        {
            throw new NotImplementedException();
        }

        protected override bool Intersects(AlignedRectangle other)
        {
            return Overlaps(other);
        }

        protected override bool Intersects(Polygon other)
        {
            throw new NotImplementedException();
        }

        protected override bool Contains(Triangle other)
        {
            throw new NotImplementedException();
        }

        protected override bool Contains(AlignedRectangle other)
        {
            return other.UpperLeft.X >= UpperLeft.X && other.LowerRight.X <= LowerRight.X
                && other.UpperLeft.Y >= UpperLeft.Y && other.LowerRight.Y <= LowerRight.Y;
        }

        protected override bool Contains(Polygon other)
        {
            throw new NotImplementedException();
        }

        static public bool operator ==(AlignedRectangle r1, AlignedRectangle r2)
        {
            return r1.Center == r2.Center && r1.Width == r2.Width && r1.Height == r2.Height;
        }

        static public bool operator !=(AlignedRectangle r1, AlignedRectangle r2)
        {
            return !(r1 == r2);
        }
    }
}
