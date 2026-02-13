using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A right axis-aligned rectangular prism
    /// </summary>
    public class Box : ConvexPolyhedron
    {
        public Box(Point center, double width, double length, double height) :
            base([
                center + new Vector(-width / 2, -length / 2, -height / 2),
                center + new Vector(width / 2, -length / 2, -height / 2),
                center + new Vector(width / 2, length / 2, -height / 2),
                center + new Vector(-width / 2, length / 2, -height / 2),
                center + new Vector(-width / 2, -length / 2, height / 2),
                center + new Vector(width / 2, -length / 2, height / 2),
                center + new Vector(width / 2, length / 2, height / 2),
                center + new Vector(-width / 2, length / 2, height / 2),
            ])
        {
            Points = [.. Vertices];
        }

        private readonly Point[] Points;

        public override IEnumerable<Polygon> Faces
        {
            get
            {
                yield return new Quadrilateral(Points[0], Points[1], Points[2], Points[3]);
                yield return new Quadrilateral(Points[0], Points[1], Points[5], Points[4]);
                yield return new Quadrilateral(Points[1], Points[2], Points[6], Points[5]);
                yield return new Quadrilateral(Points[2], Points[3], Points[7], Points[6]);
                yield return new Quadrilateral(Points[3], Points[0], Points[4], Points[7]);
                yield return new Quadrilateral(Points[4], Points[5], Points[6], Points[7]);
            }
        }

        public override IEnumerable<LineSegment> Edges
        {
            get
            {
                yield return new LineSegment(Points[0], Points[1]);
                yield return new LineSegment(Points[1], Points[2]);
                yield return new LineSegment(Points[2], Points[3]);
                yield return new LineSegment(Points[3], Points[0]);
                yield return new LineSegment(Points[0], Points[4]);
                yield return new LineSegment(Points[1], Points[5]);
                yield return new LineSegment(Points[2], Points[6]);
                yield return new LineSegment(Points[3], Points[7]);
                yield return new LineSegment(Points[4], Points[5]);
                yield return new LineSegment(Points[5], Points[6]);
                yield return new LineSegment(Points[6], Points[7]);
                yield return new LineSegment(Points[7], Points[4]);
            }
        }

        public override double Volume
        {
            get
            {
                double width = Points[1].X - Points[0].X;
                double length = Points[3].Y - Points[0].Y;
                double height = Points[4].Z - Points[0].Z;
                return width * length * height;
            }
        }

        public override Matrix InertialTensor
        {
            get
            {
                var matrix = new Matrix(3, 3);
                matrix[0, 0] = (UtilityFunctions.Square(Points[3].Y - Points[0].Y) + UtilityFunctions.Square(Points[4].Z - Points[0].Z)) * 8 / 3;
                matrix[1, 1] = (UtilityFunctions.Square(Points[1].X - Points[0].X) + UtilityFunctions.Square(Points[4].Z - Points[0].Z)) * 8 / 3;
                matrix[2, 2] = (UtilityFunctions.Square(Points[1].X - Points[0].X) + UtilityFunctions.Square(Points[3].Y - Points[0].Y)) * 8 / 3;
                return matrix;
            }
        }

        public override Point CenterOfMass => new((Points[1].X + Points[0].X) / 2, (Points[3].Y + Points[0].Y) / 2, (Points[4].Z + Points[0].Z) / 2);

        public override IEnumerable<Point> Intersection(Line line)
        {
            throw new NotImplementedException();
        }
    }
}
