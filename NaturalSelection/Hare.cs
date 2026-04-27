using System.Collections.Generic;
using System.Windows.Media;

namespace NaturalSelection
{
    public class Hare : SimplifiedAnimal
    {
        private const string filename = "rabbit.png";

        public Hare() : base(0, aniSize, aniSize)
        {
            GraphicCode = 6;
        }

        public Hare(Genotype genotype) :
            base(genotype, 0, aniSize, aniSize)
        {
            GraphicCode = 6;
        }

        public override Color Color { get { return Colors.DodgerBlue; } }
        public override string Name { get { return "Hare"; } }

        protected override void FillStats()
        {
            Stats.AgeOfSexualMaturity = 40;
            Stats.DailyEnergyMax = 10;
            Stats.DistanceToMate = 1;
            Stats.DistanceToEat = 0;
            Stats.EnergyAsFood = 50;
            Stats.EnergyToEat = 2;
            Stats.EnergyToMate = 0;
            Stats.EnergyToMove = 1;
            Stats.GestationTime = 4;
            Stats.InitialEnergy = 50;
            Stats.LitterSize = 3;
            Stats.MaxEnergyStorage = 50;
            Stats.MaxMovingDistance = 1;
            Stats.MetabolicConsumption = 1;
            Stats.MeanLifeSpan = 80;
        }

        protected override EcologyTurn EcologyChooseAction()
        {
            var others = GetNearby<Hare>(Stats.DistanceToMate);

            foreach (var ani in others)
            {
                var male = ani.IsMale ? ani : this;
                var female = ani.IsMale ? this : ani;

                //bool extraMateabilityCheck = true; //1a
                //bool extraMateabilityCheck = male.GetGene("Dark coat") > female.GetGene("Dark coat"); //1b
                bool extraMateabilityCheck = male.GetGene("Dark coat") > female.GetGene("Dark coat"); //1c

                if (CheckMateability(ani) && extraMateabilityCheck)
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
                new("Male", 0, 0, Colors.Chartreuse, GetType()),
                new("Dark coat", 5, 1, Colors.Brown, GetType())
            ];
        }
    }
}

