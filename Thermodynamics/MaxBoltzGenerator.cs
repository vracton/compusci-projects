using DongUtility;
using System.CodeDom;

namespace Thermodynamics
{
    /// <summary>
    /// A random generator that generates particles with a Maxwell-Boltzmann distribution of speeds by temperature
    /// </summary>
    /// <param name="temp">The expected temperature of the particles</param>
    public class MaxBoltzGenerator(ParticleContainer cont, double temp, double maxSpeed, double maxP) : RandomGenerator(cont)
    {
        private const double boltzmannConstant = 1.38e-23;

        //~~https://en.wikipedia.org/wiki/Maxwell%E2%80%93Boltzmann_distribution#Distribution_for_the_velocity_vector~~
        override protected double GetSpeed(ParticleInfo info)
        {
            //double stdDev = Math.Sqrt(boltzmannConstant * temp / info.Mass);
            //double vx = RandomGen.NextGaussian(0, stdDev);
            //double vy = RandomGen.NextGaussian(0, stdDev);
            //double vz = RandomGen.NextGaussian(0, stdDev);
            //return Math.Sqrt(vx * vx + vy * vy + vz * vz);

            double randX = RandomGen.NextDouble(0, maxSpeed);
            double randY = RandomGen.NextDouble(0, maxP);
            while (randY > GetProb(info.Mass, temp, randX))
            {
                randX = RandomGen.NextDouble(0, maxSpeed);
                randY = RandomGen.NextDouble(0, maxP);
            }

            return randX;
        }

        static public double GetProb(double mass, double temp, double speed)
        {
            return 4.0 * Math.PI * Math.Pow(mass / (2.0 * Math.PI * boltzmannConstant * temp), 3.0 / 2.0) * (speed * speed) * Math.Exp(-(mass * speed * speed)/(2 * boltzmannConstant * temp));
        }
    }
}
