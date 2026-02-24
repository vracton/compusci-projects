using DongUtility;
using Geometry.Geometry2D;
using System;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A path in a helix shape, oriented along the z axis
    /// </summary>
    /// <param name="radius">The radius of the helix.  Negative numbers indicate counterclockwise winding</param>
    /// <param name="center">The center of the helix, placed at the starting point</param>
    /// <param name="zVelocity">The speed at which it moves in the z direction, with the parametrix parameter as time</param>
    /// <param name="angularOffset">At what azimuthal angle the helix begins</param>
    /// <param name="initialValue">Where to start the parametric calculation</param>
    /// <param name="finalValue">Where to stop the parametric calculation</param>
    class HelixPath(double radius, Point center, double zVelocity, double angularOffset, double initialValue, double finalValue) : Path
    {
        public override double InitialParameter { get; } = initialValue;

        public override double FinalParameter { get; } = finalValue;

        public double Radius { get; } = Math.Abs(radius);

        /// <summary>
        /// True if the helix is oriented clockwise
        /// </summary>
        public bool Clockwise { get; } = radius < 0;
        public Point Center { get; } = center;
        public double ZVelocity { get; } = zVelocity;
        public double AngularOffset { get; } = angularOffset;

        protected override Vector Function(double parameter)
        {
            double x = Radius * Math.Cos(parameter + AngularOffset) + Center.X;
            double y = (Clockwise ? -1 : 1) * Radius * Math.Sin(parameter + AngularOffset) + Center.Y;
            double z = ZVelocity * parameter;
            return new Vector(x, y, z);
        }
    }
}
