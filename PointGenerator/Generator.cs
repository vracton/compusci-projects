using DecisionTree;
using DongUtility;
using System.Drawing;

namespace PointGenerator
{
    /// <summary>
    /// Base class to generates a simple point
    /// </summary>
    abstract public class Generator
    {
        static public Random Random { get; private set; } = new();

        static public void SetRandomSeed(int seed)
        {
            Random = new Random(seed);
        }

        public abstract string Names { get; }

        /// <summary>
        /// Students overload this function to create a point
        /// </summary>
        /// <param name="signal">True if it is a signal event, background otherwise</param>
        abstract protected Point CreatePoint(bool signal);

        private const double positionJitter = .1;
        private const double colorJitter = 1;

        public Generator()
        {
            Console.WriteLine("Creating generator " + Names);
        }

        /// <summary>
        /// Creates a single point, with jitter added
        /// </summary>
        /// <param name="signal">True if it is a signal event, false otherwise</param>
        public Point MakePoint(bool signal)
        {
            var point = CreatePoint(signal);

            // Add jitter
            double x = PositionJitter(point.Position.X);
            double y = PositionJitter(point.Position.Y);
            double z = PositionJitter(point.Position.Z);

            byte r = ColorJitter(point.Color.R);
            byte g = ColorJitter(point.Color.G);
            byte b = ColorJitter(point.Color.B);

            Vector newPosition = new(x, y, z);
            Color newColor = Color.FromArgb(r, g, b);

            return new Point(newPosition, newColor);
        }

        static public string[] VariableNames { get; } = ["X position", "Y position", "Z position", "Red", "Green", "Blue"];

        /// <summary>
        /// Creates a DataSet of points
        /// </summary>
        public DataSet MakeDataSet(int nEntries, double signalFraction)
        {
            if (signalFraction < 0 || signalFraction > 1)
                throw new ArgumentException("Invalid signal fraction!");

            var ds = new DataSet(VariableNames);

            for (int i = 0; i < nEntries; ++i)
            {
                bool isSignal = Random.NextBool(signalFraction);
                while (true)
                {
                    try
                    {
                        var point = MakePoint(isSignal);
                        ds.AddDataPoint(point.ConvertToDataPoint());
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    break;
                }

            }

            return ds;
        }

        /// <summary>
        /// Jitters the position randomly with a Gaussian using the coordinate as a mean
        /// and the standard deviation given by the positionJitter constant
        /// </summary>
        private static double PositionJitter(double coordinate)
        {
            double jittered = Random.NextGaussian(coordinate, positionJitter);
            return Math.Clamp(jittered, Point.MinPosition, Point.MaxPosition);
        }

        /// <summary>
        /// Jitters the color randomly with a Gaussian using the color as a mean
        /// and the standard deviation given by the colorJitter constant
        /// </summary>
        private static byte ColorJitter(byte input)
        {
            double jittered = Random.NextGaussian(input, colorJitter);
            int rounded = (int)(Math.Round(jittered, 0));
            int clamped = Math.Clamp(rounded, 0, Constants.MaxByte);
            return (byte)clamped;
        }
    }
}
