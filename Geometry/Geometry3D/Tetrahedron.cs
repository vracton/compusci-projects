using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A simple (not necessarily regular) tetrahedron.
    /// </summary>
    public class Tetrahedron(IEnumerable<Point> points) : ConvexPolyhedron(points)
    {
        public Tetrahedron(Point p1, Point p2, Point p3, Point p4) 
            : this([p1, p2, p3, p4])
        {}

        private Triangle[] triangles = [];
        /// <summary>
        /// The four faces of the tetrahedron
        /// </summary>
        public Triangle[] Triangles
        {
            get
            {
                if (triangles.Length == 0)
                {
                    CreateTriangles();
                }
                return triangles!;
            }
        }

        private void CreateTriangles()
        {
            triangles = [new (Vertices[0], Vertices[1], Vertices[2]),
                new (Vertices[0], Vertices[1], Vertices[3]),
                new (Vertices[0], Vertices[2], Vertices[3]),
                new (Vertices[1], Vertices[2], Vertices[3])];
        }

        /// <summary>
        /// Gets the center of mass, assuming uniform mass distribution
        /// </summary>
        public override Point CenterOfMass
        {
            get
            {
                // From https://math.stackexchange.com/questions/1592128/finding-center-of-mass-for-tetrahedron
                var sum = Vertices[0].PositionVector() + Vertices[1].PositionVector() + Vertices[2].PositionVector() + Vertices[3].PositionVector();
                return (.25 * sum).ToPoint();
            }
        }

        public override double Volume
        {
            // Formula from Wikipedia, using Vertices[0] as a, Vertices[1] as b, etc.
            get
            {
                var crossProduct = Vector.Cross(Vertices[1] - Vertices[3], Vertices[2] - Vertices[3]);
                var dotProduct = Vector.Dot(Vertices[0] - Vertices[3], crossProduct);
                return Math.Abs(dotProduct) / 6;
            }
        }

        public override IEnumerable<Polygon> Faces => Triangles;

        public override IEnumerable<LineSegment> Edges
        {
            get
            {
                yield return new LineSegment(Vertices[0], Vertices[1]);
                yield return new LineSegment(Vertices[0], Vertices[2]);
                yield return new LineSegment(Vertices[0], Vertices[3]);
                yield return new LineSegment(Vertices[1], Vertices[2]);
                yield return new LineSegment(Vertices[1], Vertices[3]);
                yield return new LineSegment(Vertices[2], Vertices[3]);
            }
        }

        // Answer is found in https://thescipub.com/pdf/jmssp.2005.8.11.pdf
        public override Matrix InertialTensor => throw new NotImplementedException();
    }
}
