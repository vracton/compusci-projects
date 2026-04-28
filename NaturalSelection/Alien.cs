using System.Collections.Generic;
using System.Windows.Media;

namespace NaturalSelection
{
    public class Alien : SimplifiedAnimal
    {
        public Alien() : base(0, aniSize, aniSize)
        {
            GraphicCode = 8;
        }

        public Alien(Genotype genotype) :
            base(genotype, 0, aniSize, aniSize)
        {
            GraphicCode = 8;
        }

        public override Color Color { get { return Colors.MediumPurple; } }
        public override string Name { get { return "Alien"; } }

        protected override void FillStats()
        {
            Stats.AgeOfSexualMaturity = 40;
            Stats.DailyEnergyMax = 10;
            Stats.DistanceToMate = 2;
            Stats.DistanceToEat = 0;
            Stats.EnergyAsFood = 35;
            Stats.EnergyToEat = 2;
            Stats.EnergyToMate = 0;
            Stats.EnergyToMove = 1;
            Stats.GestationTime = 8;
            Stats.InitialEnergy = 50;
            Stats.LitterSize = 3;
            Stats.MaxEnergyStorage = 50;
            Stats.MaxMovingDistance = 1;
            Stats.MetabolicConsumption = 1;
            Stats.MeanLifeSpan = 100;
        }

        protected override EcologyTurn EcologyChooseAction()
        {
            var others = new List<Alien>(GetNearby<Alien>(Stats.DistanceToMate));

            foreach (var ani in others)
            {
                if (CheckMateability(ani))
                {
                    return Mate(ani);
                }
            }

            if (FoodInCurrentCell() > 5)
            {
                return HerbivoreEat();
            }
            else
            {
                return MoveRandom();
            }
        }

        protected override IList<GeneInfo> GetGeneList()
        {
            return [
                new("Male", 0, 0, Colors.MediumPurple, GetType())
            ];
        }
    }
}
