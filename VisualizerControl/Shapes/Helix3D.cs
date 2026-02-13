using Geometry.Geometry2D;
using System;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A helix of constant radius that extends in the z direction
    /// </summary>
    public class Helix3D(double radius, Point center, double zVelocity, Geometry.Geometry3D.Point startingPoint, double nTurns = 5)
        : FunctionShape3D(new HelixPath(radius, center, zVelocity, FindStartingParameter(center, startingPoint), 0,
                2 * Math.PI * nTurns), GetName(radius, center, zVelocity, nTurns))
    {
        static private double FindStartingParameter(Point center, Geometry.Geometry3D.Point startingPoint)
        { 
            double angle = (startingPoint.PositionVector().To2D() - center.PositionVector).Azimuthal;
            return angle;
        }

        static private string GetName(double radius, Point center, double zVelocity, double nTurns)
        {
            return "Helix" + radius.ToString() + center.X.ToString() + center.Y.ToString() + zVelocity.ToString() + nTurns.ToString();
        }

        /// <summary>
        /// Creates a helix with the given parameters
        /// </summary>
        /// <param name="azimuthal">The azimuthal angle of the tangent line at the origin</param>
        /// <param name="polar">The polar angle of the tangent line at the origin</param>
        /// <param name="radius">The radius of the helix</param>
        /// <param name="nTurns">The number of complete turns to render</param>
        static public Helix3D MakeHelix(double azimuthal, double polar, double radius, double nTurns = 5)
        {
            int sign = Math.Sign(radius);
            radius = Math.Abs(radius);
            double circlePhi = azimuthal + sign * Math.PI / 2;
            double centerx = radius * Math.Cos(circlePhi);
            double centery = radius * Math.Sin(circlePhi);
            double vz = radius / Math.Tan(polar);

            return new Helix3D(radius, new Point(centerx, centery), vz, new Geometry.Geometry3D.Point(0, 0, 0), nTurns);
        }

        /// <summary>
        /// Finds the value of the parameter when the helix is at the origin
        /// </summary>
        /// <param name="centerx">The x coordinate of the center of the circle</param>
        /// <param name="centery">The y coordinate of the center of the circle</param>
        static public double GetBeginning(double centerx, double centery)
        {
            return Math.Atan2(centery, centerx);
        }
    }
}
