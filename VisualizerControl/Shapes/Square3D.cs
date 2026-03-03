using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A simple two-dimensional square in a three-dimensional space, with both sides rendered
    /// </summary>
    public class Square3D : Shape3D
    {
        public Square3D() : base("Square")
        { }

        protected override List<Vertex> MakeVertices()
        {
            var list = new List<Vertex>();

            Point3D[] pointList = [new(-1, -1, 0), new(-1, 1, 0), new(1, 1, 0), new(1, -1, 0)];

            foreach (var point in pointList)
            {
                double u = (point.X + 1) / 2;
                double v = (point.Y + 1) / 2;
                list.Add(new Vertex(point, new Vector3D(0, 0, 1), new Point(u, v)));
            }
            foreach (var point in pointList)
            {
                double u = (point.X + 1) / 2;
                double v = (point.Y + 1) / 2;
                list.Add(new Vertex(point, new Vector3D(0, 0, -1), new Point(u, v)));
            }

            return list;
        }

        protected override Int32Collection MakeTriangles()
        {
            return [0, 3, 2, 1, 0, 2, 4, 6, 7, 5, 6, 4];
        }

    }
}
