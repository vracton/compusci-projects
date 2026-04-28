using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// Creates a simple flat quadrilateral from four points
    /// </summary>
    /// <remarks>
    /// The points in the constructor should go counterclockwise around the quadrilateral when viewed from the top
    /// </remarks>
    public class Quadrilateral3D(Vector3D p1, Vector3D p2, Vector3D p3, Vector3D p4)
        : TriangleBasedShape3D([p1, p2, p3, p3, p4, p1])
    {
    }
}
