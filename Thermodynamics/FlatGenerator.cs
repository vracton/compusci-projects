using DongUtility;

namespace Thermodynamics
{
    /// <summary>
    /// A random generator that generates particles with a flat distribution between two endpoints
    /// </summary>
    /// <param name="min">The minimum possible speed</param>
    /// <param name="max">The maximum possible speed</param>
    public class FlatGenerator(ParticleContainer cont, double min, double max) : RandomGenerator(cont)
    {
        override protected double GetSpeed(ParticleInfo info)
        {
            return RandomGen.NextDouble(min, max);
        }
    }
}
