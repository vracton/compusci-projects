using System;
using Arena;
using DongUtility;
using Arena.GraphicTurns;

namespace NaturalSelection
{
    public class FoodCell : StationaryObject
    {
        public double FoodAmount { get; set; } = 0;
        public int CamouflageColor { get; }

        public override string Name => "Food cell";

        private readonly double foodMax;

        private readonly int currentPicture = 0; // -1 is the empty case

        private const double foodMaxMean = 10;
        private const double foodMaxSD = 1;
        private const double growthMean = 1;
        private const double growthSD = .1;
        private const double displayMax = foodMaxMean;

        private const int layer = 1;

        public FoodCell(int camouflageColor) :
            base(0, layer, 1, 1)
        {
            CamouflageColor = camouflageColor;
            foodMax = ArenaEngine.Random.NextGaussian(foodMaxMean, foodMaxSD);
            if (foodMax < 0)
            {
                foodMax = 0;
            }
            FoodAmount = ArenaEngine.Random.NextDouble() * foodMax;

            GraphicCode = GetPictureState();
        }

        public double EatFood(double amount)
        {
            double eaten = Math.Min(amount, FoodAmount);

            FoodAmount -= eaten;

            return eaten;
        }

        public void BeginningOfTurnGrowth()
        {
            double growth = ArenaEngine.Random.NextGaussian(growthMean, growthSD);
            if (growth < 0)
            {
                growth = 0;
            }
            FoodAmount += growth;
            if (FoodAmount > foodMax)
            {
                FoodAmount = foodMax;
            }

            SetColor();
        }

        private void SetColor()
        {
            var newState = GetPictureState();
            if (newState != currentPicture)
            {
                Arena.TurnSet.AddCommand(new ChangeObjectGraphic(layer, Code, newState));
            }                        
        }

        private int GetPictureState()
        {
            if (FoodAmount > displayMax * .75)
                return 5;
            else if (FoodAmount > displayMax * .5)
                return 4;
            else if (FoodAmount > displayMax * .25)
                return 3;
            else if (FoodAmount > 0)
                return 2;
            else
                return 1;
        }

        public override bool IsPassable(ArenaObject? mover = null)
        {
            return true;
        }
    }
}

