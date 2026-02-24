using DongUtility;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualizerControl.Commands;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// Used to store values for connectors between two points, which may be fixed or moving.
    /// For example, springs.
    /// </summary>
    internal abstract class Connector
    {
        /// <summary>
        /// This is the initial radius only.
        /// The radius automatically resizes as the string stretches and contracts
        /// </summary>
        private readonly double radius;
        private double volume = 0;
        public Color Color { get; private set; }

        public Vector3D Position { get; private set; }
        public Vector3D Scale { get; private set; }
        public double AzimuthalAngle { get; private set; }
        public double PolarAngle { get; private set; }
        private readonly double nTurns;

        public Connector(double radius, Color color, double nTurns = 5)
        {
            this.radius = radius;
            Color = color;
            Scale = new Vector3D(radius * 2, radius * 2, Scale.Z);
            this.nTurns = nTurns;
        }

        /// <summary>
        /// Updates the position of the connector based on new projectile positions
        /// </summary>
        public void Update()
        {
            AdjustToTwoPoints(Point1, Point2);
        }

        /// <summary>
        /// Returns the actual command returned by this object
        /// </summary>
        /// <param name="index">The index of the object</param>
        public TransformObject GetTransformCommand(int index)
        {
            var rotation = new Rotation();
            rotation.RotateYAxis(PolarAngle);
            rotation.RotateZAxis(AzimuthalAngle);
            return new TransformObject(index, Position, Scale,
                WPFUtility.UtilityFunctions.ConvertToMatrix3D(rotation.Matrix));
        }

        /// <summary>
        /// The position of the first anchor point
        /// </summary>
        protected abstract Vector3D Point1 { get; }
        /// <summary>
        /// The position of the second anchor point
        /// </summary>
        protected abstract Vector3D Point2 { get; }

        /// <summary>
        /// Adjusts the cylinder to connect two points
        /// </summary>
        private void AdjustToTwoPoints(Vector3D point1, Vector3D point2)
        {
            // Scale
            var diff = point2 - point1;
            var length = diff.Length;

            // Adjust volume to match scale
            double scale;
            if (volume == 0)
            {
                // Sets volume in initial case
                volume = Math.PI * radius * radius * length;
                scale = radius * 2;
            }
            else
            {
                // Adjusts radius to keep volume constant
                scale = Math.Sqrt(volume / (Math.PI * length));
            }

            Scale = new Vector3D(scale, scale, length / (4 * Math.PI * nTurns));

            Position = point1;

            // Rotate
            double theta = Math.Acos(diff.Z / length);
            double phi = Math.Atan2(diff.Y, diff.X);

            AzimuthalAngle = phi;
            PolarAngle = theta;
        }
    }
}
