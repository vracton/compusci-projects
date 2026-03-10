using System.Collections.Generic;
using System.Windows.Media;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A cylinder with caps
    /// </summary>
    public class Cylinder3D : Shape3D
    {
        static public int NSegments { get; set; } = 16;

        public Cylinder3D() :
            base("Cylinder")
        {
        }

        protected override List<Vertex> MakeVertices()
        {
            // CylinderFactory does all the work
            return CylinderFactory.MakeVertices(true, NSegments);
        }

        protected override Int32Collection MakeTriangles()
        {
            return CylinderFactory.MakeTriangles(true, NSegments);
        }
    }
}
