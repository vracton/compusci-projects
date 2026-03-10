using DongUtility;

namespace Thermodynamics
{
    /// <summary>
    /// A generator used to create random particles in a specified way
    /// </summary>
    /// <param name="cont">The particle container that will hold the generated particles</param>
    abstract public class RandomGenerator(ParticleContainer cont)
    {

        /// <summary>
        /// The static random number generator, internal so it can be synchronized across all classes
        /// </summary>
        static internal protected Random RandomGen { get; } = new Random();

        /// <summary>
        /// Choose a random position inside the container
        /// </summary>
        private static Vector RandomPosition(DongUtility.Range xRange, DongUtility.Range yRange, DongUtility.Range zRange)
        {
            return new Vector(RandomGen.NextDouble(xRange.Min, xRange.Max), RandomGen.NextDouble(yRange.Min, yRange.Max), RandomGen.NextDouble(zRange.Min, zRange.Max));
        }

        /// <summary>
        /// Choose a speed at random for a new particle
        /// </summary>
        abstract protected double GetSpeed(ParticleInfo info);

        /// <summary>
        /// Creates a new random particle of a given type 
        /// </summary>
        /// <param name="name">The name, as a string, of the particle type</param>
        public Molecule GetRandomParticle(string name, DongUtility.Range xRange, DongUtility.Range yRange, DongUtility.Range zRange)
        {
            var particleType = cont.Dictionary.Map[name];
            var speed = GetSpeed(particleType);
            Vector velocity = Vector.RandomDirection(speed, RandomGen);
            Vector position = RandomPosition(xRange, yRange, zRange);

            return cont.Dictionary.MakeParticle(position, velocity, name);
        }

        public Molecule GetRandomParticle(string name)
        {
            return GetRandomParticle(name, new DongUtility.Range(0, cont.Size.X), new DongUtility.Range(0, cont.Size.Y), new DongUtility.Range(0, cont.Size.Z));
        }
    }
}
