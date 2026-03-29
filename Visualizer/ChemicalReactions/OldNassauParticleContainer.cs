using DongUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using Thermodynamics;

namespace Visualizer.ChemicalReactions
{
    internal sealed class OldNassauParticleContainer : ParticleContainer
    {
        private const double KspHgI2 = 2.2e-12;
        private const double RelativeTolerance = 1.0e-4;
        private const double MinimumChemistryStep = 1.0e-8;
        private const double MaximumChemistryStep = 0.25;
        private const double DefaultTracerSpeed = 0.15;

        private readonly string[] visibleSpecies;
        private readonly Dictionary<string, double> bulkState;
        private readonly int tracerBudget;
        private readonly Random random = new();
        private readonly double k1;
        private readonly double k2;
        private readonly double k3;

        public OldNassauParticleContainer(IEnumerable<ParticleInfo> particles, double side,
            IDictionary<string, double> initialState, IEnumerable<string> visibleSpecies, int tracerBudget,
            double baseK1, double baseK2, double baseK3, double chemistryRandomness) : base(side)
        {
            foreach (var particle in particles)
            {
                Dictionary.AddParticle(particle);
            }

            bulkState = new Dictionary<string, double>(initialState);
            this.visibleSpecies = [..visibleSpecies];
            this.tracerBudget = tracerBudget;
            k1 = ApplyRandomness(baseK1, chemistryRandomness);
            k2 = ApplyRandomness(baseK2, chemistryRandomness);
            k3 = ApplyRandomness(baseK3, chemistryRandomness);

            ParticlesToAdd.Clear();
            ParticlesToRemove.Clear();
            SyncTracersToBulkState();
            ParticlesToAdd.ForEach(AddParticleDirectly);
            ParticlesToAdd.Clear();
            ParticlesToRemove.Clear();
        }

        public IReadOnlyList<string> VisibleSpecies => visibleSpecies;

        public double Temperature
        {
            get
            {
                if (Particles.Count == 0)
                {
                    return 0;
                }

                double sumKineticEnergy = Particles.Sum(x => x.KineticEnergy);
                return sumKineticEnergy / Particles.Count * 2.0 / 3.0 / Constants.BoltzmannConstant;
            }
        }

        public double GetSpeciesMolarity(string speciesName)
        {
            return bulkState.GetValueOrDefault(speciesName);
        }

        public override void Update(double deltaTime)
        {
            ParticlesToAdd.Clear();
            ParticlesToRemove.Clear();

            foreach (var part in Particles)
            {
                part.Update(deltaTime);
                CheckParticle(part);
            }

            AdvanceBulkChemistry(deltaTime);
            SyncTracersToBulkState();

            ParticlesToAdd.ForEach(AddParticleDirectly);
            ParticlesToRemove.ForEach(RemoveParticleDirectly);
        }

        private void AdvanceBulkChemistry(double deltaTime)
        {
            double remaining = deltaTime;
            double step = Math.Min(deltaTime, 0.01);

            while (remaining > 0)
            {
                step = Math.Min(step, remaining);

                var full = Rk4Step(bulkState, step);
                var half = Rk4Step(Rk4Step(bulkState, step / 2.0), step / 2.0);
                double error = RelativeError(full, half);

                if (error > RelativeTolerance && step > MinimumChemistryStep)
                {
                    step = Math.Max(step / 2.0, MinimumChemistryStep);
                    continue;
                }

                ReplaceState(half);
                remaining -= step;

                if (error < RelativeTolerance / 100.0)
                {
                    step = Math.Min(step * 1.5, MaximumChemistryStep);
                }
            }
        }

        private void SyncTracersToBulkState()
        {
            var targetCounts = GetTargetTracerCounts();
            var currentBySpecies = Particles.GroupBy(x => x.Info.Name)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (string species in visibleSpecies)
            {
                currentBySpecies.TryGetValue(species, out List<Molecule>? current);
                current ??= [];

                int target = targetCounts.GetValueOrDefault(species);
                while (current.Count > target)
                {
                    Molecule particle = current[^1];
                    current.RemoveAt(current.Count - 1);
                    ParticlesToRemove.Add(particle);
                }

                while (current.Count < target)
                {
                    Molecule particle = Dictionary.MakeParticle(RandomPosition(), RandomVelocity(), species);
                    ParticlesToAdd.Add(particle);
                    current.Add(particle);
                }
            }
        }

        private Dictionary<string, int> GetTargetTracerCounts()
        {
            var tracked = visibleSpecies
                .Where(name => name != "H^p")
                .ToDictionary(name => name, name => Math.Max(0.0, bulkState.GetValueOrDefault(name)));

            double total = tracked.Values.Sum();
            var targets = visibleSpecies.ToDictionary(name => name, _ => 0);

            if (total <= 1.0e-12)
            {
                return targets;
            }

            int assigned = 0;
            foreach ((string species, double concentration) in tracked.OrderByDescending(x => x.Value))
            {
                int target = (int)Math.Round(tracerBudget * concentration / total);
                if (concentration > 0 && target == 0)
                {
                    target = 1;
                }

                targets[species] = target;
                assigned += target;
            }

            while (assigned > tracerBudget)
            {
                string species = tracked.OrderByDescending(x => targets[x.Key]).First(x => targets[x.Key] > 0).Key;
                targets[species]--;
                assigned--;
            }

            while (assigned < tracerBudget && tracked.Count > 0)
            {
                string species = tracked.OrderByDescending(x => x.Value).First().Key;
                targets[species]++;
                assigned++;
            }

            return targets;
        }

        private Vector RandomPosition()
        {
            return new Vector(
                random.NextDouble(0, Size.X),
                random.NextDouble(0, Size.Y),
                random.NextDouble(0, Size.Z));
        }

        private Vector RandomVelocity()
        {
            return Vector.RandomDirection(DefaultTracerSpeed, random);
        }

        private void ReplaceState(Dictionary<string, double> nextState)
        {
            foreach ((string key, double value) in nextState)
            {
                bulkState[key] = value;
            }
        }

        private Dictionary<string, double> ReactionDerivatives(Dictionary<string, double> state)
        {
            double io3 = state["IO_3^-"];
            double hso3 = state["HSO_3^-"];
            double iodide = state["I^-"];
            double hydrogen = Math.Max(state["H^p"], 0.0);
            double iodine = state["I_2"];

            double v1 = k1 * io3 * hso3 * hydrogen;
            double v2 = k2 * io3 * iodide * iodide * hydrogen * hydrogen;
            double v3 = k3 * iodine * hso3;

            return new Dictionary<string, double>
            {
                ["IO_3^-"] = -v1 - v2,
                ["HSO_3^-"] = -3.0 * v1 - v3,
                ["I^-"] = v1 - 5.0 * v2 + 2.0 * v3,
                ["H^p"] = 3.0 * v1 - 6.0 * v2 + 3.0 * v3,
                ["I_2"] = 3.0 * v2 - v3,
                ["Hg^2p"] = 0.0,
                ["HgI_2"] = 0.0
            };
        }

        private static Dictionary<string, double> CombineState(Dictionary<string, double> state, double factor,
            Dictionary<string, double> derivative)
        {
            var output = new Dictionary<string, double>();
            foreach ((string key, double value) in state)
            {
                output[key] = Math.Max(0.0, value + factor * derivative[key]);
            }

            EnforceHgI2Solubility(output);
            return output;
        }

        private Dictionary<string, double> Rk4Step(Dictionary<string, double> state, double dt)
        {
            var k1 = ReactionDerivatives(state);
            var k2 = ReactionDerivatives(CombineState(state, dt / 2.0, k1));
            var k3 = ReactionDerivatives(CombineState(state, dt / 2.0, k2));
            var k4 = ReactionDerivatives(CombineState(state, dt, k3));

            var output = new Dictionary<string, double>();
            foreach ((string key, double value) in state)
            {
                double delta = dt * (k1[key] + 2.0 * k2[key] + 2.0 * k3[key] + k4[key]) / 6.0;
                output[key] = Math.Max(0.0, value + delta);
            }

            EnforceHgI2Solubility(output);
            return output;
        }

        private static double RelativeError(Dictionary<string, double> reference, Dictionary<string, double> candidate)
        {
            double worst = 0.0;
            foreach ((string key, double value) in reference)
            {
                double scale = Math.Max(1.0e-12, Math.Max(Math.Abs(value), Math.Abs(candidate[key])));
                worst = Math.Max(worst, Math.Abs(value - candidate[key]) / scale);
            }

            return worst;
        }

        private double ApplyRandomness(double baseline, double randomness)
        {
            if (randomness <= 0)
            {
                return baseline;
            }

            double scale = 1.0 + randomness * (2.0 * random.NextDouble() - 1.0);
            return baseline * Math.Max(0.01, scale);
        }

        private static void EnforceHgI2Solubility(Dictionary<string, double> state)
        {
            double mercury = state["Hg^2p"];
            double iodide = state["I^-"];

            if (mercury <= 0.0 || iodide <= 0.0 || mercury * iodide * iodide <= KspHgI2)
            {
                return;
            }

            double low = 0.0;
            double high = Math.Min(mercury, iodide / 2.0);

            for (int i = 0; i < 80; ++i)
            {
                double x = 0.5 * (low + high);
                double residual = (mercury - x) * Math.Pow(Math.Max(iodide - 2.0 * x, 0.0), 2);
                if (residual > KspHgI2)
                {
                    low = x;
                }
                else
                {
                    high = x;
                }
            }

            double precipitated = high;
            state["Hg^2p"] -= precipitated;
            state["I^-"] -= 2.0 * precipitated;
            state["HgI_2"] += precipitated;
        }
    }
}
