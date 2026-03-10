namespace Thermodynamics
{
    public enum ReactionOrder
    {
        ZeroOrder,
        FirstOrder,
        SecondOrder
    }

    public class RateLawParticleContainer(double size, ReactionOrder order, double rateConstant, string reactantName, string productName) : ParticleContainer(size)
    {
        public readonly List<double> timeHistory = [];
        public readonly List<double> concentrationHistory = [];
        public int InitialReactantCount;
        public double ConversionAccumulator;
        public double CurrentMeasuredRate;
        public double AccumulatedRateError;
        public int RateErrorSamples;
        public bool IsInitialized;
        public ReactionOrder Order { get; } = order;
        public double RateConstant { get; } = rateConstant;
        public string ReactantName { get; } = reactantName;
        public string ProductName { get; } = productName;
        public double SimulationTime { get; private set; }

        public double NormalizedConcentration => GetRawNormalizedConcentration();
        public double ProductFraction => 1.0 - NormalizedConcentration;
        public double ExpectedRate => GetExpectedRate(NormalizedConcentration);
        public double RateRmse => RateErrorSamples == 0 ? 0 : Math.Sqrt(AccumulatedRateError / RateErrorSamples);
        public double ExpectedRateConstant => RateConstant;

        public void InitializeExperiment()
        {
            if (IsInitialized)
            {
                return;
            }

            InitialReactantCount = GetNParticles(ReactantName);
            if (InitialReactantCount <= 0)
            {
                throw new InvalidOperationException("Rate-law experiment requires reactant particles before initialization.");
            }

            SimulationTime = 0;
            IsInitialized = true;
            double initialConcentration = GetRawNormalizedConcentration();
            CurrentMeasuredRate = GetExpectedRate(initialConcentration);
            AccumulatedRateError = 0;
            RateErrorSamples = 0;
            RecordSample();
        }

        public override void Update(double deltaTime)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("InitializeExperiment() must be called before updating the rate-law simulation.");
            }

            base.Update(deltaTime);

            switch (Order)
            {
                case ReactionOrder.ZeroOrder:
                    ConvertZeroOrder(deltaTime);
                    break;
                case ReactionOrder.FirstOrder:
                    ConvertFirstOrder(deltaTime);
                    break;
                case ReactionOrder.SecondOrder:
                    ConvertSecondOrder(deltaTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SimulationTime += deltaTime;
            UpdateEffectiveRateConstant(deltaTime);
            RecordSample();
        }

        public void ConvertZeroOrder(double deltaTime)
        {
            int reactantCount = GetNParticles(ReactantName);
            if (reactantCount <= 0)
            {
                return;
            }

            ConversionAccumulator += RateConstant * deltaTime * InitialReactantCount;
            int nToConvert = Math.Min(reactantCount, (int)Math.Floor(ConversionAccumulator));
            if (nToConvert <= 0)
            {
                return;
            }

            ConversionAccumulator -= nToConvert;
            List<Molecule> reactants = GetReactantParticles(nToConvert);
            ConvertParticlesToProducts(reactants);
        }

        public void ConvertFirstOrder(double deltaTime)
        {
            double reactionProbability = 1.0 - Math.Exp(-RateConstant * deltaTime);
            List<Molecule> reactants = [];
            foreach (var particle in Particles)
            {
                if (particle.Info.Name == ReactantName && Random.NextDouble() < reactionProbability)
                {
                    reactants.Add(particle);
                }
            }

            ConvertParticlesToProducts(reactants);
        }

        public void ConvertSecondOrder(double deltaTime)
        {
            double pairReactionProbability = 1.0 - Math.Exp(-(RateConstant / InitialReactantCount) * deltaTime);
            if (pairReactionProbability <= 0)
            {
                return;
            }

            List<Molecule> reactants = GetReactantParticles();
            HashSet<Molecule> chosenParticles = [];

            for (int i = 0; i < reactants.Count; ++i)
            {
                Molecule first = reactants[i];
                if (chosenParticles.Contains(first))
                {
                    continue;
                }

                for (int j = i + 1; j < reactants.Count; ++j)
                {
                    Molecule second = reactants[j];
                    if (chosenParticles.Contains(second))
                    {
                        continue;
                    }

                    if (Random.NextDouble() >= pairReactionProbability)
                    {
                        continue;
                    }

                    chosenParticles.Add(first);
                    chosenParticles.Add(second);
                    break;
                }
            }

            if (chosenParticles.Count == 0)
            {
                return;
            }

            ConvertParticlesToProducts(chosenParticles);
        }

        public void UpdateEffectiveRateConstant(double deltaTime)
        {
            if (deltaTime <= 0)
            {
                CurrentMeasuredRate = GetExpectedRate(NormalizedConcentration);
                return;
            }

            double currentConcentration = NormalizedConcentration;
            if (concentrationHistory.Count == 0)
            {
                CurrentMeasuredRate = GetExpectedRate(currentConcentration);
                return;
            }

            int referenceIndex = Math.Max(0, concentrationHistory.Count - 5);
            double referenceConcentration = concentrationHistory[referenceIndex];
            double referenceTime = timeHistory[referenceIndex];
            double elapsed = Math.Max(SimulationTime - referenceTime, deltaTime);
            CurrentMeasuredRate = -(currentConcentration - referenceConcentration) / elapsed;
            double rateError = GetExpectedRate(currentConcentration) - CurrentMeasuredRate;
            AccumulatedRateError += rateError * rateError;
            ++RateErrorSamples;
        }

        public void RecordSample()
        {
            timeHistory.Add(SimulationTime);
            concentrationHistory.Add(NormalizedConcentration);
        }

        public List<Molecule> GetReactantParticles(int maxCount = int.MaxValue)
        {
            List<Molecule> reactants = [];
            foreach (var particle in Particles)
            {
                if (particle.Info.Name == ReactantName)
                {
                    reactants.Add(particle);
                    if (reactants.Count == maxCount)
                    {
                        break;
                    }
                }
            }

            return reactants;
        }

        public void ConvertParticlesToProducts(IEnumerable<Molecule> reactants)
        {
            foreach (var particle in reactants)
            {
                Molecule product = Dictionary.MakeParticle(particle.Position, particle.Velocity, ProductName);

                //make sure the visualizer and the underlying particle list is in sunc
                RemoveParticle(particle);
                RemoveParticleDirectly(particle);
                AddParticle(product);
                AddParticleDirectly(product);
            }
        }

        public double GetRawNormalizedConcentration()
        {
            return (double)GetNParticles(ReactantName) / InitialReactantCount;
        }

        public double GetExpectedRate(double concentration)
        {
            return Order switch
            {
                ReactionOrder.ZeroOrder => RateConstant,
                ReactionOrder.FirstOrder => RateConstant * concentration,
                ReactionOrder.SecondOrder => RateConstant * concentration * concentration,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
