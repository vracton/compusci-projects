using DongUtility;
using Geometry.Geometry2D;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// Polyhedron base class
    /// </summary>
    public abstract class Polyhedron(IEnumerable<Point> points) : Shape3D
    {
        public abstract IEnumerable<Polygon> Faces { get; }
        public abstract IEnumerable<LineSegment> Edges { get; }
        public Point[] Vertices { get; } = [.. points];

        public override double SurfaceArea => Faces.Sum((x) => x.UnderlyingShape.Area);

        public override double MaxRadius
        {
            get
            {
                var center = Center;
                return Vertices.Max((x) => Point.DistanceSquared(center, x));
            }
        }

        protected override Point GetCenter()
        {
            return UtilityFunctions.GetCenter(Vertices.Select(x => x.PositionVector())).ToPoint();
        }

        public override Point ClosestPoint(Point point)
        {
            return Faces.Max((x) => x.ClosestPoint(point));
        }

        public override Shape3D Translate(Vector vector)
        {
            if (Clone() is not Polyhedron clone)
            {
                throw new Exception("Clone of polyhedron returned null or non-polyhedron!");
            }
            for (var i = 0; i < clone.Vertices.Length; ++i)
            {
                var position = clone.Vertices[i].PositionVector();
                position += vector;
                clone.Vertices[i] = position.ToPoint();
            }
            return clone; 
        }

        public override Shape3D Clone()
        {
            // Get the runtime type (handles derived types)
            var type = GetType();

            // Use the Vertices array to reconstruct the points for the constructor
            var ctor = type.GetConstructor([typeof(IEnumerable<Point>)]) ?? throw new InvalidOperationException($"No suitable constructor found for {type.Name}.");

            // Create a new instance
            var clone = (Polyhedron)ctor.Invoke([Vertices]);

            return clone;
        }

        public override IEnumerable<Point> Intersection(Line line)
        {
            foreach (var face in Faces)
            {
                var intersection = face.Intersection(line);
                if (intersection != null)
                {
                    yield return (Point)intersection;
                }
            }
        }

        public override IEnumerable<Point> Intersection(LineSegment segment)
        {
            foreach (var face in Faces)
            {
                var intersection = face.Intersection(segment);
                if (intersection != null)
                {
                    yield return (Point)intersection;
                }
            }
        }
    }
}
