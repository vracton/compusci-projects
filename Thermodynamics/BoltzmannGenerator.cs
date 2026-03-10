using DongUtility;

namespace Thermodynamics
{
    /// <summary>
    /// A random generator that creates particles according to the Boltzmann distribution
    /// </summary>
    public class BoltzmannGenerator(ParticleContainer cont, double temperature, ParticleInfo info,
        int projectedCalls = 10000, int nDivisions = 10000) : FunctionGenerator(cont, projectedCalls, nDivisions)
    {
        private readonly double mass = info.Mass;

        protected override double Function(double speed)
        {
            double speedSquared = UtilityFunctions.Square(speed);
            return 4 * Math.PI * Math.Pow(mass / (2 * Math.PI * Constants.BoltzmannConstant * temperature), 1.5)
                * speedSquared
                * Math.Exp(-mass * speedSquared / (2 * Constants.BoltzmannConstant * temperature));
        }
    }
}
