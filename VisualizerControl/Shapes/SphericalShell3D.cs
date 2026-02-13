using System;

namespace VisualizerControl.Shapes
{
    public class SphericalShell3D(int nFacePoints, double startR, double endR) : 
        SphereSegment3D(nFacePoints, 0, 2 * Math.PI, 0, Math.PI, startR, endR)
    {
    }
}
