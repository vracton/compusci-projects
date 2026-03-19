using DongUtility;
using System.IO.Packaging;
using System.Linq;

namespace Thermodynamics
{

    public struct Reaction
    {
        public string[] Reactants { get; init; }
        public string[] Products { get; init; }

        public Reaction(string[] reactants, string[] products)
        {
            Reactants = reactants;
            Products = products;
        }
    }

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

        public ReactingParticleContainer(double side, double collisionRadius, int updateThreads) :
            base(side)
        {
            CollisionRadius = collisionRadius;
            NumThreads = updateThreads;
        }

        public ReactingParticleContainer(ParticleInfo[] molecules, string[] equations, double side, double collisionRadius, int updateThreads) :
            base(side)
        {
            CollisionRadius = collisionRadius;
            NumThreads = updateThreads;
            RegisterReactions(molecules, equations);
        }

        public List<Reaction> Reactions { get; private set; } = new List<Reaction>();

        private void RegisterReactions(ParticleInfo[] molecules, string[] equations)
        {
            //add molecules
            foreach (ParticleInfo mol in molecules)
            {
                Dictionary.AddParticle(mol);
            }

            //register reactions
            foreach (string equation in equations)
            {
                string[] sides = equation.Split("->");
                string[] reactants = sides[0].Split("+").Select(x => x.Trim()).ToArray();
                string[] products = sides[1].Split("+").Select(x => x.Trim()).ToArray();

                foreach (string comp in reactants.Concat(products))
                {
                    if (!Dictionary.Map.ContainsKey(comp))
                    {
                        throw new Exception($"particle {comp} in equation {equation} is not defined in the particle dictionary");
                    }
                }

                Reactions.Add(new Reaction(reactants, products));
            }
        }

        private HashSet<Molecule> reactedParticles = [];

        public double Temperature
        {
            get
            {
                double sumKE = Particles.Sum(x => x.KineticEnergy);

                return sumKE / Particles.Count * 2.0 / 3.0 / Constants.BoltzmannConstant;
            }
        }

        protected override void Setup()
        {
            reactedParticles.Clear();
            base.Setup();
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
            var particleList = new List<(Molecule, double)>();
            foreach (var part in particles)
            {
                if (!ParticlesToRemove.Contains(part))
                    particleList.Add((part, Vector.Distance(particle.Position, part.Position)));
            }

            //sort by decreasing distance
            particleList.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            var sortedParticleList = particleList.Select(x => x.Item1).ToList();

            if (particleList.Count > 1)
            {
                React(particle, sortedParticleList);
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
