using DongUtility;
using System.Drawing;
using VisualizerControl;
using VisualizerControl.Shapes;

namespace Thermodynamics
{
    /// <summary>
    /// A container class for holding gas particles in a cubical container
    /// </summary>
    public class ParticleContainer(double xSize, double ySize, double zSize)
    {
        private const string AmmoniaName = "NH3";
        private const string HydrochloricAcidName = "HCl";
        private const string AmmoniumChlorideName = "NH4Cl";
        private const double reactionRadius = 2.5;
        private const double exothermicEnergy = 2.9e-19;

        /// <summary>
        /// The size of the container
        /// Each component of the vector is the size in that dimension
        /// </summary>
        public Vector Size { get; set; } = new Vector(xSize, ySize, zSize);
        /// <summary>
        /// All the particles in the container
        /// </summary>
        public List<Molecule> Particles { get; } = [];

        /// <summary>
        /// A dictionary to keep track of all the particle types
        /// </summary>
        public ParticleDictionary Dictionary { get; } = new ParticleDictionary();

        /// <summary>
        /// Access to the random generator
        /// </summary>
        static protected Random Random { get { return RandomGenerator.RandomGen; } }

        private const double boltzmannConstant = 1.38e-23;
        private readonly Dictionary<(int x, int y, int z), List<Molecule>> spatialHash = [];
        private readonly HashSet<Molecule> reactedParticles = [];

        public double Temperature {
            get
            {
                if (Particles.Count == 0)
                {
                    return 0;
                }

                double KESum = 0;
                foreach (Molecule part in Particles)
                {
                    KESum += 0.5 * part.Mass * part.Velocity.MagnitudeSquared;
                }
                return (KESum / Particles.Count) * 2.0 / (3.0 * boltzmannConstant);
            }
        }

        public ParticleContainer(double size) :
            this(size, size, size)
        { }

        public void RegisterParticleType(string name, double mass, Color color)
        {
            RegisterParticleType(name, mass, color, new Sphere3D());
        }

        public void RegisterParticleType(string name, double mass, Color color, Shape3D shape)
        {
            Dictionary.AddParticle(new ParticleInfo(name, mass, color, shape));
        }

        public List<Molecule> ParticlesToAdd { get; } = [];
        public List<Molecule> ParticlesToRemove { get; } = [];
        public void AddParticle(Molecule part)
        {
            ParticlesToAdd.Add(part);
        }

        public void RemoveParticle(Molecule part)
        {
            ParticlesToRemove.Add(part);
        }

        /// <summary>
        /// Adds a particle directly
        /// Not for general use; hence, it is protected
        /// </summary>
        protected virtual void AddParticleDirectly(Molecule part)
        {
            Particles.Add(part);
        }

        /// <summary>
        /// Removes a particle directly
        /// Not for general use; hence, it is protected
        /// </summary>
        /// <param name="part"></param>
        protected virtual void RemoveParticleDirectly(Molecule part)
        {
            Particles.Remove(part);
        }

        /// <summary>
        /// Gets the total number of particles of a given name
        /// </summary>
        public int GetNParticles(string name)
        {
            int total = 0;

            foreach (var part in Particles)
            {
                if (part.Info.Name == name)
                {
                    ++total;
                }
            }

            return total;
        }

        /// <summary>
        /// Adds a number of particles at random
        /// </summary>
        /// <param name="generator">The random generator to use</param>
        /// <param name="name">The name of the particle type</param>
        /// <param name="number">The number of particles to add</param>
        public void AddRandomParticles(RandomGenerator generator, string name, int number)
        {
            for (int i = 0; i < number; ++i)
            {
                AddParticleDirectly(generator.GetRandomParticle(name));
            }
        }

        //add to range instead of whole box
        public void AddRandomParticles(RandomGenerator generator, string name, int number, DongUtility.Range xRange, DongUtility.Range yRange, DongUtility.Range zRange)
        {
            for (int i = 0; i < number; ++i)
            {
                AddParticleDirectly(generator.GetRandomParticle(name, xRange, yRange, zRange));
            }
        }

        /// <summary>
        /// Updates all particles for a given time increment
        /// </summary>
        public virtual void Update(double deltaTime)
        {
            ParticlesToAdd.Clear();
            ParticlesToRemove.Clear();

            Setup();

            foreach (var part in Particles)
            {
                part.Update(deltaTime);
                CheckParticle(part);
            }

            BuildSpatialHash();

            foreach (var part in Particles)
            {
                ParticleUpdate(part);
            }

            ParticlesToAdd.ForEach((x) => AddParticleDirectly(x));
            ParticlesToRemove.ForEach((x) => RemoveParticleDirectly(x));
        }

        /// <summary>
        /// A function that can be overridden to update particles in a specific way
        /// </summary>
        protected virtual void ParticleUpdate(Molecule part)
        {
            if (reactedParticles.Contains(part))
            {
                return;
            }

            //get the name of the particle that this one can react with
            string partnerName = GetReactionPartnerName(part.Info.Name);
            if (partnerName == string.Empty)
            {
                return;
            }

            foreach (var other in GetNearbyParticles(part, reactionRadius))
            {
                if (ReferenceEquals(part, other) || reactedParticles.Contains(other) || other.Info.Name != partnerName || Vector.Distance2(part.Position, other.Position) > reactionRadius * reactionRadius )
                {
                    continue;
                }

                ReactParticles(part, other);
                break;
            }
        }

        /// <summary>
        /// Find all particles near a given particle.
        /// By default, returns all particles.
        /// Can be overridden in derived classes
        /// </summary>
        /// <param name="center">The position of the current particle</param>
        /// <param name="rad">The radius to look within</param>        /// <param name="toBeRemoved">A list of particles that have already been removed from simulation</param>
        /// <returns>All particles within the radius rad from the given particle, plus maybe some extra</returns>
        //get nearby particles by looking at nearby cells
        protected virtual IEnumerable<Molecule> GetNearbyParticles(Molecule center, double rad)
        {
            int cellSpan = Math.Max(1, (int)Math.Ceiling(rad / reactionRadius));
            var cell = GetCell(center.Position);

            for (int x = cell.x - cellSpan; x <= cell.x + cellSpan; ++x)
            {
                for (int y = cell.y - cellSpan; y <= cell.y + cellSpan; ++y)
                {
                    for (int z = cell.z - cellSpan; z <= cell.z + cellSpan; ++z)
                    {
                        if (spatialHash.TryGetValue((x, y, z), out List<Molecule>? contents))
                        {
                            foreach (var particle in contents)
                            {
                                yield return particle;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepare for a single loop
        /// </summary>
        protected virtual void Setup()
        {
            reactedParticles.Clear();
            spatialHash.Clear();
        }

        /// <summary>
        /// A function that extracts a specific property of a particle
        /// </summary>
        public delegate double ParticleFunction(Molecule part);

        /// <summary>
        /// Get a list of values for a given property of all particles
        /// Useful for histograms
        /// </summary>
        public List<double> GetParticlePropertyList(ParticleFunction func)
        {
            var response = new List<double>();

            foreach (var part in Particles)
            {
                response.Add(func(part));
            }

            return response;
        }

        /// <summary>
        /// Make sure the particle lies within the bounds of the box
        /// Reflect it back if it is not
        /// </summary>
        protected virtual void CheckParticle(Molecule particle)
        {
            Vector newVec = particle.Position;
            if (particle.Position.X < 0 || particle.Position.X > Size.X)
            {
                particle.Velocity = new Vector(-particle.Velocity.X, particle.Velocity.Y, particle.Velocity.Z);
                if (particle.Position.X < 0)
                {
                    newVec.X = 0;
                }
                else if (particle.Position.X > Size.X)
                {
                    newVec.X = Size.X;
                }
            }
            if (particle.Position.Y < 0 || particle.Position.Y > Size.Y)
            {
                particle.Velocity = new Vector(particle.Velocity.X, -particle.Velocity.Y, particle.Velocity.Z);
                if (particle.Position.Y < 0)
                {
                    newVec.Y = 0;
                }
                else if (particle.Position.Y > Size.Y)
                {
                    newVec.Y = Size.Y;
                }
            }
            if (particle.Position.Z < 0 || particle.Position.Z > Size.Z)
            {
                particle.Velocity = new Vector(particle.Velocity.X, particle.Velocity.Y, -particle.Velocity.Z);
                if (particle.Position.Z < 0)
                {
                    newVec.Z = 0;
                }
                else if (particle.Position.Z > Size.Z)
                {
                    newVec.Z = Size.Z;
                }
            }
            particle.Position = newVec;
        }

        //split the box into cells and keep track of which particles are in which cells
        private void BuildSpatialHash()
        {
            spatialHash.Clear();
            foreach (var part in Particles)
            {
                var cell = GetCell(part.Position);
                if (!spatialHash.TryGetValue(cell, out List<Molecule>? contents))
                {
                    contents = [];
                    spatialHash[cell] = contents;
                }
                contents.Add(part);
            }
        }

        private static string GetReactionPartnerName(string name)
        {
            return name switch
            {
                AmmoniaName => HydrochloricAcidName,
                HydrochloricAcidName => AmmoniaName,
                _ => string.Empty
            };
        }

        private void ReactParticles(Molecule first, Molecule second)
        {
            //get info for ammonium chloride
            if (!Dictionary.Map.TryGetValue(AmmoniumChlorideName, out ParticleInfo? productInfo))
            {
                return;
            }

            reactedParticles.Add(first);
            reactedParticles.Add(second);
            RemoveParticle(first);
            RemoveParticle(second);

            //put the released energy directly into the product's translational motion
            double totalMass = first.Mass + second.Mass;
            Vector position = (first.Mass * first.Position + second.Mass * second.Position) / totalMass;
            Vector momentum = first.Momentum + second.Momentum;
            double productKineticEnergy = first.KineticEnergy + second.KineticEnergy + exothermicEnergy;
            double productSpeed = Math.Sqrt(2.0 * productKineticEnergy / productInfo.Mass);
            Vector direction = momentum.IsNull ? Vector.RandomDirection(1, Random) : momentum.UnitVector();
            Vector velocity = direction * productSpeed;

            AddParticle(Dictionary.MakeParticle(position, velocity, AmmoniumChlorideName));
        }

        //pos -> cell
        private static (int x, int y, int z) GetCell(Vector position)
        {
            return (
                (int)Math.Floor(position.X / reactionRadius),
                (int)Math.Floor(position.Y / reactionRadius),
                (int)Math.Floor(position.Z / reactionRadius));
        }
    }
}
