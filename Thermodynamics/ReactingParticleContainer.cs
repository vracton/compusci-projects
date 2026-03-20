using DongUtility;
using System.IO.Packaging;
using System.Linq;

namespace Thermodynamics
{

    public struct Reaction
    {
        public string[] Reactants { get; init; }
        public string[] Products { get; init; }
        public double Enthalpy { get; init; }

        public Reaction(string[] reactants, string[] products, double enthalpy)
        {
            Reactants = reactants;
            Products = products;
            Enthalpy = enthalpy;
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

        public ReactingParticleContainer(ParticleInfo[] molecules, (string, double)[] equations, double side, double collisionRadius, int updateThreads) :
            base(side)
        {
            CollisionRadius = collisionRadius;
            NumThreads = updateThreads;
            RegisterReactions(molecules, equations);
        }

        public List<Reaction> Reactions { get; private set; } = new List<Reaction>();

        private void RegisterReactions(ParticleInfo[] molecules, (string, double)[] equations)
        {
            //add molecules to dictionary
            foreach (ParticleInfo mol in molecules)
            {
                Dictionary.AddParticle(mol);
            }

            //register reactions
            foreach ((string equation, double enthalpy) in equations)
            {
                string[] sides = equation.Split("->");
                string[] reactants = ExpandReactionSide(sides[0]);
                string[] products = ExpandReactionSide(sides[1]);

                foreach (string comp in reactants.Concat(products))
                {
                    if (!Dictionary.Map.ContainsKey(comp))
                    {
                        throw new Exception($"particle {comp} in equation {equation} is not defined in the particle dictionary");
                    }
                }

                Reactions.Add(new Reaction(reactants, products, enthalpy));
            }
        }

        //expand components, i.e. 2H+O becomes H,H,O
        private static string[] ExpandReactionSide(string side)
        {
            var expanded = new List<string>();

            foreach (string rawComponent in side.Split("+"))
            {
                string component = rawComponent.Trim();

                int index = 0;
                while (index < component.Length && char.IsDigit(component[index]))
                {
                    index++;
                }

                int coefficient = 1;
                if (index > 0)
                {
                    coefficient = int.Parse(component[..index]);
                    component = component[index..].Trim();
                }

                for (int i = 0; i < coefficient; ++i)
                {
                    expanded.Add(component);
                }
            }

            return [..expanded];
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

        //add to range instead of whole box
        public void AddRandomParticles(RandomGenerator generator, string name, int number, DongUtility.Range xRange, DongUtility.Range yRange, DongUtility.Range zRange)
        {
            for (int i = 0; i < number; ++i)
            {
                AddParticleDirectly(generator.GetRandomParticle(name, xRange, yRange, zRange));
            }
        }

        /// <summary>
        /// Checks whether a the particle is close enough to another particle to react, and calls React() if so
        /// </summary>
        private List<Molecule> CheckCollisions(Molecule particle)
        {
            var particles = GetNearbyParticles(particle, CollisionRadius);
            var particleList = new List<(Molecule, double)>();
            foreach (var part in particles)
            {
                //make sure particle is usable
                if (!ReferenceEquals(particle, part) && Vector.Distance(particle.Position, part.Position) <= CollisionRadius)
                    particleList.Add((part, Vector.Distance(particle.Position, part.Position)));
            }

            //sort by decreasing distance
            particleList.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            var sortedParticleList = particleList.Select(x => x.Item1).ToList();

            if (particleList.Count > 0)
            {
                return React(particle, sortedParticleList);
            }

            return [];
        }

        public int CompletedReactions { get; private set; } = 0;
        private readonly object _reactionLock = new object();

        protected virtual List<Molecule> React(Molecule particle, List<Molecule> nearby)
        {
            var usedParticles = new List<Molecule>();
            Reaction chosenReaction = default;

            //check if a reaction is possible with nearby particles
            lock (_reactionLock)
            {
                if (reactedParticles.Contains(particle) || ParticlesToRemove.Contains(particle))
                {
                    return [];
                }

                foreach (var reaction in Reactions)
                {
                    bool particleUsed = false;
                    bool possible = true;
                    var remaining = new List<Molecule>(nearby);
                    var used = new List<Molecule>();

                    foreach (string reactant in reaction.Reactants)
                    {
                        if (!particleUsed && particle.Info.Name == reactant)
                        {
                            particleUsed = true;
                            continue;
                        }

                        int matchIndex = remaining.FindIndex(x => (x.Info.Name == reactant && !reactedParticles.Contains(x) && !ParticlesToRemove.Contains(x)));
                        if (matchIndex == -1)
                        {
                            possible = false;
                            break;
                        }

                        used.Add(remaining[matchIndex]);
                        remaining.RemoveAt(matchIndex);
                    }

                    if (possible && particleUsed)
                    {
                        used.Add(particle);
                        usedParticles = [..used];
                        chosenReaction = reaction;
                        break;
                    }
                }

                if (usedParticles.Count == 0)
                    return [];

                //remove reactants
                foreach (var part in usedParticles)
                {
                    reactedParticles.Add(part);
                    ParticlesToRemove.Add(part);
                }
            }

            double massOfReactants = chosenReaction.Reactants.Sum(x => Dictionary.Map[x].Mass);
            double massOfProducts = chosenReaction.Products.Sum(x => Dictionary.Map[x].Mass);
            Vector momOfReactants = usedParticles.Aggregate(Vector.NullVector(), (acc, x) => acc + x.Momentum);
            Vector centerOfReactants = usedParticles.Aggregate(Vector.NullVector(), (acc, x) => acc + x.Mass * x.Position) / massOfReactants;
            Vector comVel = momOfReactants / massOfProducts;

            //create products
            if (chosenReaction.Products.Length == 1)
            {
                //if there's only product, then we just conserve momentum - not neccisarily kinetic energy
                return [Dictionary.MakeParticle(centerOfReactants, comVel, chosenReaction.Products[0])];
            }
            else
            {
                //for multiple products, both momentum and kinetic energy can be conserved
                //velocity of molecule i, u_i, equals v_{COM} plus some w_i, such that the sum of momenta relative to COM is 0
                double reactantKE = usedParticles.Sum(x => x.KineticEnergy);
                double KEout = reactantKE + chosenReaction.Enthalpy;
                double KErel = KEout - 0.5 * massOfProducts * Math.Pow(comVel.Magnitude, 2);

                Vector[] w = new Vector[chosenReaction.Products.Length];

                if (KErel >= 0)
                {
                    Vector[] a = new Vector[chosenReaction.Products.Length];
                    Vector aAvg = Vector.NullVector();

                    //generate random directions, while making sure COM-relative momenta sum to 0
                    for (int i = 0; i < w.Length; i++)
                    {
                        a[i] = Vector.RandomDirection(1, new Random());
                        aAvg += a[i] * Dictionary.Map[chosenReaction.Products[i]].Mass;
                    }
                    aAvg /= massOfProducts;

                    double curKE = 0;
                    for (int i = 0; i < w.Length; i++)
                    {
                        a[i] -= aAvg;
                        curKE += 0.5 * Dictionary.Map[chosenReaction.Products[i]].Mass * Math.Pow(a[i].Magnitude, 2);
                    }

                    //scale a_i so KE is conserved, \lambda * a_i = w_i
                    double lambda = Math.Sqrt(KErel / curKE);

                    for (int i = 0; i < w.Length; i++)
                    {
                        w[i] = a[i] * lambda;
                    }
                }
                else
                {
                    //conservation of momentum requires more energy than is available, so KE is not conserved in this case
                    Array.Fill(w, Vector.NullVector());
                }

                List<Molecule> products = new List<Molecule>();

                for (int i = 0; i < chosenReaction.Products.Length; i++)
                {
                    products.Add(Dictionary.MakeParticle(centerOfReactants, comVel + w[i], chosenReaction.Products[i]));
                }

                return products;
            }
        }

        /// <summary>
        /// Updates a single particle in the container
        /// </summary>
        protected override List<Molecule> ParticleUpdate(Molecule part)
        {
            List<Molecule> newParticles = [];

            newParticles.AddRange(CheckCollisions(part));
            CheckDecay(part);

            return newParticles;
        }

        protected virtual void CheckDecay(Molecule part)
        {
            // Here is where you would check for decay of a particle, and implement the results of the spontaneous decay
        }
    }
}
