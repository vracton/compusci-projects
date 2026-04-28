using Arena;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace NaturalSelection
{
    public class Lynx : SimplifiedAnimal
    {
        private const string filename = "lynx.png";

        public Lynx() :
            base(0, aniSize, aniSize)
        {
            GraphicCode = 7;
        }

        public Lynx(Genotype genotype) :
            base(genotype, 0, aniSize, aniSize)
        {
            GraphicCode = 7;
        }

        public override Color Color { get { return Colors.IndianRed; } }
        public override string Name { get { return "Lynx"; } }

        protected override void FillStats()
        {
            Stats.AgeOfSexualMaturity = 20;
            Stats.DailyEnergyMax = 50;
            Stats.DistanceToEat = 4;
            Stats.DistanceToMate = 3;
            Stats.EnergyAsFood = 10;
            Stats.EnergyToEat = 12;
            Stats.EnergyToMate = 10;
            Stats.EnergyToMove = 1;
            Stats.GestationTime = 4;
            Stats.InitialEnergy = 100;
            Stats.LitterSize = 1;
            Stats.MaxEnergyStorage = 200;
            Stats.MaxMovingDistance = 2;
            Stats.MeanLifeSpan = 50;
            Stats.MetabolicConsumption = 3;
        }

        protected override EcologyTurn EcologyChooseAction()
        {
            var others = GetNearby<Lynx>(Stats.DistanceToMate);
            if (others.Count() < 5)
            {
                foreach (var ani in others)
                {
                    if (CheckMateability(ani))
                    {
                        return Mate(ani);
                    }
                }
            }

            // const double huntingSuccessFactor = .01;

            if (Energy < 100)
            {
                var prey = new List<EcologyAnimal>();
                prey.AddRange(GetNearby<Hare>(Stats.DistanceToEat));
                prey.AddRange(GetNearby<Alien>(Stats.DistanceToEat));

                //prey.Sort((first, second) => second.GetGene("Dark coat").CompareTo(first.GetGene("Dark coat")));
                prey.Sort((first, second) => HuntingSuccess(second).CompareTo(HuntingSuccess(first)));

                //if (Arena.GetObjectsOfType<Hare>().Count() > 100)
                if (prey.Count > 0)
                {
                    foreach (var ani in prey)
                    {
                        if (ArenaEngine.Random.NextDouble() < HuntingSuccess(ani))
                            return CarnivoreEat(ani);
                    }
                }
            }
            return MoveRandom();
        }

        private double HuntingSuccess(Hare hare)
        {
            var cell = Arena.CurrentCell(hare.Position.PositionVector);
            return Math.Abs(hare.GetGene("Dark coat") - cell.CamouflageColor) * .0005 + .005; //abs will prob return [0, 20], so this takes to [.005, .015]
        }

        private double HuntingSuccess(EcologyAnimal prey)
        {
            const double baseHuntingSuccess = .01;
            double appearance = prey is Hare hare ? hare.GetGene("Likeness") : 1;
            return baseHuntingSuccess / (1 + GetGene("Aversion") * appearance);
        }

        public void LoseEnergy()
        {
            RemoveEnergy(Energy * .2);
        }

        protected override IList<GeneInfo> GetGeneList()
        {
            return [
                new("Male", 0, 0, Colors.Aquamarine, GetType()),
                new("Aversion", .5, .05, Colors.Purple, GetType())
            ];
        }
    }
}

