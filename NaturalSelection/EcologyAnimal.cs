using System;
using System.Collections.Generic;
using System.Windows.Media;
using Arena;
using DongUtility;

namespace NaturalSelection
{
    abstract public class EcologyAnimal : MovingObject
    {
        public int Age { get; private set; } = 0;
        private int deathAge;

        private double energyReserve = 0;
        private double eatenToday = 0;
        protected double Energy => energyReserve;

        private int gestationElapsed = 0;
        private readonly IList<EcologyAnimal> fetus = [];

        public bool IsMale => GetGene("Male") > 0;
        public bool IsDead { get; set; } = false;

        abstract public Color Color { get; }

        public bool IsPregnant => fetus.Count > 0;

        private const int orgLayer = 2;

        private readonly Genotype genotype;

        public EcologyAnimal(int graphicCode, double width, double height) :
            base(graphicCode, orgLayer, width, height)
        {
            Initialize();

            if (!GeneDictionary.HasAnimal(this))
            {
                GeneDictionary.AddGene(this, GetGeneList());
            }

            var father = new List<double>();
            var mother = new List<double>();

            foreach (var gene in GeneDictionary.GetGeneList(this))
            {
                if (gene.Name == "Male")
                {
                    father.Add(ArenaEngine.Random.Next(2));
                    mother.Add(0);
                }
                else
                {
                    double fVal = GetRanGene(gene);
                    double mVal = GetRanGene(gene);
                    father.Add(fVal);
                    mother.Add(mVal);
                }
            }
            genotype = new Genotype(father, mother);
        }

        private void Initialize()
        {
            FillStats();
            energyReserve = Stats.InitialEnergy;
            deathAge = GetDeathAge();
        }

        private static double GetRanGene(GeneInfo gi)
        {
            if (gi.SD < 0)
            {
                return ArenaEngine.Random.Next(2);
            }
            else
            {
                return ArenaEngine.Random.NextGaussian(gi.Mean, gi.SD);
            }
        }

        public EcologyAnimal(Genotype genotype, int graphicCode, double width, double height) :
            base(graphicCode, orgLayer, width, height)
        {
            this.genotype = genotype;
            Initialize();
        }


        internal void SetRandomAge()
        {
            Age = ArenaEngine.Random.Next(0, deathAge);
        }

        abstract protected void FillStats();

        public class AnimalStats
        {
            public double MetabolicConsumption { get; set; }
            public double MaxEnergyStorage { get; set; }
            public double AgeOfSexualMaturity { get; set; }
            public double GestationTime { get; set; }
            public int LitterSize { get; set; }
            public double DistanceToMate { get; set; }
            public double DistanceToEat { get; set; } // Relevant to lynxes only
            public double InitialEnergy { get; set; }
            public double EnergyAsFood { get; set; } // Relevant to hares only
            public double DailyEnergyMax { get; set; }
            public double EnergyToMate { get; set; }
            public double EnergyToMove { get; set; }
            public double MaxMovingDistance { get; set; }
            public double EnergyToEat { get; set; }
            public double MeanLifeSpan { get; set; }
        }

        public AnimalStats Stats { get; set; } = new AnimalStats();

        private int GetDeathAge()
        {
            return Math.Abs((int)Math.Round(ArenaEngine.Random.NextGaussian(Stats.MeanLifeSpan, Stats.MeanLifeSpan * .2)));
        }

        protected override Turn UserDefinedChooseAction()
        {
            if (IsPregnant && gestationElapsed >= Stats.GestationTime)
            {
                return new Turns.GiveBirth(this);
            }
            else
            {
                return EcologyChooseAction();
            }
        }

        abstract protected EcologyTurn EcologyChooseAction();

        protected override void UserDefinedBeginningOfTurn()
        {
            eatenToday = 0;
        }

        protected override bool DoTurn(Turn? turn)
        {
            if (turn == null)
            {
                return false;
            }
            EcologyTurn t = (EcologyTurn)turn;
            if (t == null)
            {
                return false;
            }

            double energy = t.EnergyConsumption();

            if (energy <= energyReserve)
            {
                bool result = t.DoTurn();

                if (result)
                {
                    RemoveEnergy(energy);
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        protected override void UserDefinedEndOfTurn()
        {
            ++Age;
            if (Age >= deathAge)
            {
                IsDead = true;
            }

            if (IsPregnant)
            {
                ++gestationElapsed;
            }

            energyReserve -= Stats.MetabolicConsumption;

            if (energyReserve <= 0)
            {
                IsDead = true;
            }

            if (IsDead)
            {
                Arena.RemoveObjectDelay(this);
            }
        }

        public double DailyEnergyLeft()
        {
            return Stats.DailyEnergyMax - eatenToday;
        }

        protected void AddEnergy(double energy)
        {
            double addedAmount = Math.Min(Stats.MaxEnergyStorage - energyReserve, energy);
            addedAmount = Math.Min(addedAmount, DailyEnergyLeft());

            energyReserve += addedAmount;
            eatenToday += addedAmount;
        }

        protected void RemoveEnergy(double energy)
        {
            energyReserve -= energy;
        }

        public bool IsSexuallyMature()
        {
            return Age >= Stats.AgeOfSexualMaturity;
        }

        public bool Eat(double food)
        {
            AddEnergy(food);

            return food > 0;
        }

        protected const double aniSize = .2;

        public List<EcologyAnimal> GiveBirth()
        {
            List<EcologyAnimal> copy = [.. fetus];

            fetus.Clear();
            gestationElapsed = 0;
            return copy;
        }

        public bool Mate(EcologyAnimal other)
        {
            if (!CheckMateability(other))
            {
                return false;
            }

            var female = IsMale ? other : this;

            int nKids = female.LitterSize();
            for (int i = 0; i < nKids; ++i)
            {
                female.fetus.Add(CreateOffspring(other));
            }

            return true;
        }

        private int LitterSize()
        {
            int response = (int)Math.Round(ArenaEngine.Random.NextGaussian(Stats.LitterSize, 1));
            if (response < 1)
            {
                return 1;
            }
            else
            {
                return response;
            }
        }

        protected bool CheckMateability(EcologyAnimal other)
        {
            var displacement = Position - other.Position;
            return IsSexuallyMature() && other.IsSexuallyMature() && !IsPregnant && !other.IsPregnant && IsMale != other.IsMale
                    && CheckType(other) && displacement.MagnitudeSquared <= UtilityFunctions.Square(Stats.DistanceToMate);
        }

        protected bool CheckType(EcologyAnimal other)
        {
            return other.GetType() == GetType();
        }

        protected EcologyAnimal CreateOffspring(EcologyAnimal other)
        {
            var gen = new Genotype(genotype.Meiosis(this), other.genotype.Meiosis(other));
            var ty = GetType();
            var constructor = ty.GetConstructor([typeof(Genotype)]) ?? throw new Exception($"No suitable constructor found for type {ty.Name}");
            return (EcologyAnimal)constructor.Invoke([gen]);
        }

        public override bool IsPassable(ArenaObject? mover = null)
        {
            return true;
        }

        public double GetGene(string name)
        {
            return genotype.GetGene(name, this);
        }

        abstract protected IList<GeneInfo> GetGeneList();

        static public GeneDictionary GeneDictionary { get; } = new GeneDictionary();
    }
}
