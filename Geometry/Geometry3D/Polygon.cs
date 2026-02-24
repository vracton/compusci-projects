namespace Geometry.Geometry3D
{
    public class Polygon : Shape2D<Geometry2D.Polygon>
    {
        public Point[] Vertices { get; }
        public int NVertices => Vertices.Length;
        public IEnumerable<LineSegment> Edges
        {
            get
            {
                // Note that this assumes sequential ordering
                for (int i = 0; i < NVertices - 1; i++)
                {
                    yield return new LineSegment(Vertices[i], Vertices[i + 1]);
                }
                // The last one connects the first and last vertex
                yield return new LineSegment(Vertices[NVertices - 1], Vertices[0]);
            }
        }

        /// <param name="points">A list of vertices entered in sequential order</param>
        public Polygon(IEnumerable<Point> points)
        {
            Vertices = [.. points];
            if (Vertices.Length < 3)
            {
                throw new ArgumentException("A polygon must have at least three vertices!");
            }
            if (!Plane.AreCoplanar(points))
            {
                throw new ArgumentException("Points must be coplanar");
            }
        }

        public override double DistanceFrom(LineSegment segment)
        {
            // First check for intersection
            if (Intersects(segment))
            {
                return 0;
            }
            // Then check perpendicular distance
            var perpendicularPoint = ContainingPlane.NearestPoint(segment);
            if (IsInside(perpendicularPoint))
            {
                // If it is inside the polygon, it must be the closest point
                return segment.Distance(perpendicularPoint);
            }

            // Otherwise, try the edges
            double minDistance2 = double.MaxValue;
            foreach (var edge in Edges)
            {
                var distance2 = segment.DistanceSquared(edge);
                if (distance2 < minDistance2)
                {
                    minDistance2 = distance2;
                }
            }
            return Math.Sqrt(minDistance2);
        }

        protected override Plane CalculateContainingPlane()
        {
            return new Plane(Vertices[0], Vertices[1], Vertices[2]);
        }

        protected override Geometry2D.Polygon CalculateUnderlyingShape()
        {
            return new Geometry2D.Polygon(Vertices.Select(p => ContainingPlane.TransformTo2D(p)));
        }

        public override Point ClosestPoint(Point point)
        {
            // First check perpendicular distance to the plane
            // If that is inside the polygon, that must be closest
            var perpendicularPoint = ContainingPlane.NearestPoint(point);
            if (IsInside(perpendicularPoint))
            {
                return perpendicularPoint;
            }

            // Otherwise, it's the closest point on the edges
            double minDistance2 = double.MaxValue;
            Point bestPoint = new(0, 0, 0); // dummy value that must be replaced

            foreach (var edge in Edges)
            {
                var closestPoint = edge.NearestPoint(point);
                var distance2 = Point.DistanceSquared(closestPoint, point);
                if (distance2 < minDistance2)
                {
                    minDistance2 = distance2;
                    bestPoint = closestPoint;
                }
            }

            return bestPoint;
        }
    }
}
