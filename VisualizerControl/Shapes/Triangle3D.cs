using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A simple 2D triangle in 3D space, rendered on both sides
    /// </summary>
    public class Triangle3D : Shape3D
    {
        public Vector3D[] Points { get; private set; } = new Vector3D[3];

        public Triangle3D(Vector3D point1, Vector3D point2, Vector3D point3, bool freezeMesh = true) :
            base(TriangleName(point1, point2, point3), freezeMesh)
        {
            Points[0] = point1;
            Points[1] = point2;
            Points[2] = point3;
        }

        static private string TriangleName(Vector3D vertex1, Vector3D vertex2, Vector3D vertex3)
        {
            return $"Triangle: ({vertex1.X}, {vertex1.Y}, {vertex1.Z}), ({vertex2.X}, {vertex2.Y}, {vertex2.Z}), ({vertex3.X}, {vertex3.Y}, {vertex3.Z})";
        }

        static public List<Vertex> MakeVerticesForTriangle(Vector3D vertex1, Vector3D vertex2, Vector3D vertex3)
        {
            var response = new List<Vertex>();

            Vector3D dir1 = vertex2 - vertex1;
            Vector3D dir2 = vertex3 - vertex1;
            Vector3D normal = Vector3D.CrossProduct(dir1, dir2);

            double projection = Vector3D.DotProduct(dir1, dir2) / dir2.Length;

            response.Add(new Vertex((Point3D)vertex1, normal, new Point(0, 0)));
            response.Add(new Vertex((Point3D)vertex2, normal, new Point(1, 0)));
            response.Add(new Vertex((Point3D)vertex3, normal, new Point(projection, 1)));

            return response;
        }

        protected override List<Vertex> MakeVertices()
        {
            return MakeVerticesForTriangle(Points[0], Points[1], Points[2]);
        }

        protected override Int32Collection MakeTriangles()
        {
            return new Int32Collection() { 0, 1, 2, 0, 2, 1 }; // Double-sided
        }
    }
}
