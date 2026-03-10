using DongUtility;

namespace Thermodynamics
{
    /// <summary>
    /// A container for particles that can react chemicallywith each other
    /// </summary>
    public class ReactingParticleContainer : ParticleContainer
    {
        protected double CollisionRadius { get; private set; }

        public ReactingParticleContainer(double xSize, double ySize, double zSize, double collisionRadius) :
            base(xSize, ySize, zSize)
        {
            CollisionRadius = collisionRadius;
        }

        public ReactingParticleContainer(double side, double collisionRadius) :
            base(side)
        {
            CollisionRadius = collisionRadius;
        }

        public double Temperature
        {
            get
            {
                double sumKE = Particles.Sum(x => x.KineticEnergy);

                return sumKE / Particles.Count * 2.0 / 3.0 / Constants.BoltzmannConstant;
            }
        }

        /// <summary>
        /// Add a specific particle of a specific type
        /// </summary>
        public void AddParticle(string name, Vector position, Vector velocity)
        {
            ParticlesToAdd.Add(Dictionary.MakeParticle(position, velocity, name));
        }

        /// <summary>
        /// Add a number of random particles of a specific type (given by a name)
        /// </summary>
        public void AddRandomParticles(RandomGenerator generator, int number, string name)
        {
            for (int i = 0; i < number; ++i)
            {
                AddParticleDirectly(generator.GetRandomParticle(name));
            }
        }

        /// <summary>
        /// Checks whether a the particle is close enough to another particle to react, and calls React() if so
        /// </summary>
        private void CheckCollisions(Molecule particle)
        {
            if (ParticlesToRemove.Contains(particle))
            {
                return;
            }
            var particles = GetNearbyParticles(particle, CollisionRadius);
            var particleList = new List<Molecule>();
            foreach (var part in particles)
            {
                if (!ParticlesToRemove.Contains(part))
                    particleList.Add(part);
            }

            if (particleList.Count > 1)
            {
                React(particle, particleList);
            }
        }

        protected virtual void React(Molecule particle, List<Molecule> nearby)
        {
            // Here the particle is the primary particle, and nearby is a list of all
            // nearby particles.  This determines what happens when you react two particles
            // Use this for Level III
        }

        /// <summary>
        /// Updates a single particle in the container
        /// </summary>
        protected override void ParticleUpdate(Molecule part)
        {
            CheckCollisions(part);
            CheckDecay(part);
        }

        protected virtual void CheckDecay(Molecule part)
        {
            // Here is where you would check for decay of a particle, and implement the results of the spontaneous decay
        }
    }
}
