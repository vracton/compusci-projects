using DongUtility;

namespace Geometry.Geometry2D
{
    /// <summary>
    /// Generic polygon class.  Does not guard against crossing lines or self-intersections.
    /// </summary>
    public class Polygon(IEnumerable<Point> vertices) : Shape2D
    {
        /// <summary>
        /// Returns all the vertices of the polygon, in order
        /// </summary>
        public virtual List<Point> Vertices { get; } = [.. vertices];

        /// <summary>
        /// Returns all the edges of the polygon, in order
        /// </summary>
        public IEnumerable<LineSegment> Edges
        {
            get
            {
                var vertices = Vertices.ToList();
                for (int i = 0; i < vertices.Count - 1; ++i)
                {
                    int otherIndex = i == vertices.Count - 1 ? 0 : i + 1;
                    yield return new LineSegment(vertices[i], vertices[otherIndex]);
                }
            }
        }

        /// <summary>
        /// Breaks the polygon into triangles
        /// </summary>
        public IEnumerable<Triangle> Triangles
        {
            get
            {
                var vertices = Vertices.ToList();
                for (int i = 1; i < vertices.Count - 1; ++i)
                {
                    yield return new Triangle(vertices[0], vertices[i], vertices[i + 1]);
                }
            }
        }

        public override double Area => throw new NotImplementedException();

        public override double Perimeter => throw new NotImplementedException();

        public override RangePair Range => throw new NotImplementedException();

        public override double MaxRadius => throw new NotImplementedException();

        public override Point Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Point ClosestPoint(Point point)
        {
            throw new NotImplementedException();
        }

        public override bool Inside(Point point)
        {
            throw new NotImplementedException();
        }

        public override LineSegment? Overlap(LineSegment lineSegment)
        {
            var intersections = new List<Point>();
            foreach (var edge in Edges)
            {
                if (LineSegment.Intersect(edge, lineSegment))
                {
                    intersections.Add(LineSegment.Intersection(edge, lineSegment));
                }
            }

            if (intersections.Count < 2)
            {
                return null;
            }
            else if (intersections.Count == 2)
            {
                return new LineSegment(intersections[0], intersections[1]);
            }
            else
            {
                throw new Exception("Polygon has more than two intersections with line segment!");
            }
        }

        public override Shape2D Rotate(double angle, Point centerOfRotation)
        {
            var rotationMatrix = new Matrix(2, 2);
            rotationMatrix[0, 0] = Math.Cos(angle);
            rotationMatrix[0, 1] = -Math.Sin(angle);
            rotationMatrix[1, 0] = Math.Sin(angle);
            rotationMatrix[1, 1] = Math.Cos(angle);

            var newVertices = new List<Point>();
            foreach (var vertex in Vertices)
            {
                var translated = vertex - centerOfRotation;
                var rotated = rotationMatrix * translated;
                newVertices.Add(rotated + centerOfRotation);
            }

            return new Polygon(newVertices);
        }

        public override Shape2D Translate(Vector2D vector)
        {
            throw new NotImplementedException();
        }

        protected override bool Contains(Triangle other)
        {
            throw new NotImplementedException();
        }

        protected override bool Contains(AlignedRectangle other)
        {
            throw new NotImplementedException();
        }

        protected override bool Intersects(Triangle other)
        {
            throw new NotImplementedException();
        }

        protected override bool Intersects(AlignedRectangle other)
        {
            throw new NotImplementedException();
        }

        protected override bool Intersects(Polygon other)
        {
            throw new NotImplementedException();
        }

        protected override bool Contains(Polygon other)
        {
            throw new NotImplementedException();
        }
    }
}
