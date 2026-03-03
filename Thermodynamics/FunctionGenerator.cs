namespace Thermodynamics
{
    /// <summary>
    /// A base class for random generators that generate particles with a random speed according to a function
    /// </summary>
    abstract public class FunctionGenerator(ParticleContainer cont, int nDivisions = 10000, int projectedCalls = 10000) : RandomGenerator(cont)
    {
        private double minVal;
        private double maxVal;
        private double increment = double.NaN;
        private double normalization;
        private readonly double threshold = 1.0 / projectedCalls / 10;

        /// <summary>
        /// Basic setup to find the right scale for the generator.
        /// Automatically called on the first call to GetSpeed()
        /// </summary>
        protected void Setup()
        {
            var range = GetScale(threshold);
            minVal = Math.Pow(10, range.Item1);
            maxVal = Math.Pow(10, range.Item2);
            increment = (maxVal - minVal) / nDivisions;
            normalization = Normalize();
        }

        // These are the minimum and maximum exponents for doubles
        private const int minExponent = -324;
        private const int maxExponent = 308;

        /// <summary>
        /// Find the general scale in which the function is non-negligible by guess-and-checking across all possible orders of magnitude
        /// </summary>
        /// <param name="threshold">The minimum amount at which the function should begin to count</param>
        /// <returns></returns>
        private Tuple<int, int> GetScale(double threshold)
        {
            // Test all exponents
            var exponentMap = new Dictionary<int, double>();

            // Find the function value at each exponent
            for (int exponent = minExponent; exponent <= maxExponent; ++exponent)
            {
                double value = Math.Pow(10, exponent);
                exponentMap.Add(exponent, Function(value));
            }

            double maxVal = exponentMap.Values.Max();
            int minExp = int.MaxValue;
            int maxExp = int.MinValue;
            double maxFunc = double.MinValue;

            // Find the range of exponents where the function is non-negligible
            foreach (var entry in exponentMap)
            {
                double scale = entry.Value / maxVal;
                if (scale > threshold)
                {
                    if (minExp == int.MaxValue)
                        minExp = entry.Key;
                    if (scale > maxFunc)
                    {
                        maxFunc = scale;
                        maxExp = entry.Key;
                    }
                }
            }

            return new Tuple<int, int>(minExp - 1, maxExp + 1); // Add a buffer of one order of magnitude on each side to be safe
        }

        /// <summary>
        /// Normalizes the function to ensure that the total probability is 1.
        /// Necessary because the normalization will not be perfect due to the finite region we are scanning
        /// </summary>
        private double Normalize()
        {
            double total = 0;

            for (double current = minVal; current < maxVal; current += increment)
            {
                total += increment * Function(current);
            }

            return 1 / total;
        }

        protected override double GetSpeed(ParticleInfo info)
        {
            // Set up function if it is the first time (so increment is the default value, NaN)
            if (double.IsNaN(increment))
            {
                Setup();
            }

            // Choose a speed by calculating a CDF up until a randomly chosen value
            double ran = RandomGen.NextDouble();
            double speed = minVal;
            double cumulative = 0;
            while (cumulative < ran)
            {
                speed += increment;
                cumulative += increment * normalization * Function(speed);
            }

            return speed;
        }

        /// <summary>
        /// The function that is called to generate the distribution
        /// </summary>
        /// <param name="speed">The parameter of the function</param>
        /// <returns>The probability density function at the point when v = speed</returns>
        abstract protected double Function(double speed);
    }
}
