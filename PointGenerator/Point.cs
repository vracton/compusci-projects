using DecisionTree;
using DongUtility;
using System.Drawing;

namespace PointGenerator
{
    /// <summary>
    /// Class holding information for a single point
    /// </summary>
    public class Point
    {
        public Vector Position { get; }

        public Color Color { get; }

        public const double MinPosition = 0;
        public const double MaxPosition = 100;

        public Point (Vector position, Color color)
        {
            double x = Math.Clamp(position.X, MinPosition, MaxPosition);
            double y = Math.Clamp(position.Y, MinPosition, MaxPosition);
            double z = Math.Clamp(position.Z, MinPosition, MaxPosition);
            Position = new Vector(x, y, z);
            Color = color;
        }

        /// <summary>
        /// Converts the point to a DataPoint for easy reading by decision trees
        /// </summary>
        public DataPoint ConvertToDataPoint()
        {
            var dp = new DataPoint(6);
            dp.Variables[0] = Position.X;
            dp.Variables[1] = Position.Y;
            dp.Variables[2] = Position.Z;
            dp.Variables[3] = Color.R;
            dp.Variables[4] = Color.G;
            dp.Variables[5] = Color.B;

            return dp;
        }

        public Point(DataPoint dp) :
            this(new Vector(dp.Variables[0], dp.Variables[1], dp.Variables[2]),
                Color.FromArgb((int)dp.Variables[3], (int)dp.Variables[4], (int)dp.Variables[5]))
        { }
    }
}
