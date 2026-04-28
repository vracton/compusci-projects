using System;
using System.Collections.Generic;
using NaturalSelection.Turns;
using Arena;
using DongUtility;

namespace NaturalSelection
{
    abstract public class SimplifiedAnimal : EcologyAnimal
    {
        public SimplifiedAnimal(int graphicCode, double width, double height) :
            base(graphicCode, width, height)
        { }

        public SimplifiedAnimal(Genotype genotype, int graphicCode, double width, double height) :
            base(genotype, graphicCode, width, height)
        { }

        protected static Random GetRandom()
        {
            return ArenaEngine.Random;
        }

        protected FoodCell GetCell()
        {
            return Arena.CurrentCell(Position.PositionVector);
        }

        protected EcologyTurn Move(Vector2D newPosition)
        {
            return new Move(this, newPosition);
        }

        protected EcologyTurn HerbivoreEat()
        {
            if (this is Hare || this is Alien)
            {
                return new HerbivoreEat(this);
            }
            else
            {
                throw new ArgumentException("Only Hares and Aliens can eat grass.");
            }
        }

        protected EcologyTurn CarnivoreEat(EcologyAnimal prey)
        {
            if (this is Lynx lynx)
            {
                return new CarnivoreEat(lynx, prey);
            }
            else
            {
                throw new ArgumentException("Only Lynxes can eat Hares.");
            }
        }

        protected EcologyTurn Wait()
        {
            return new Wait(this);
        }

        protected new EcologyTurn Mate(EcologyAnimal mate)
        {
            return new Mate(this, mate);
        }

        protected EcologyTurn MoveRandom()
        {
            var randomVec = Vector2D.RandomDirection(Stats.MaxMovingDistance, GetRandom());
            var newPosition = Position + randomVec;
            return new Move(this, newPosition.PositionVector);
        }

        protected new EcologyArena Arena => (EcologyArena)(base.Arena);

        protected double FoodInCurrentCell()
        {
            return Arena.CurrentCell(Position.PositionVector).FoodAmount;
        }

        protected IEnumerable<T> GetNearby<T>(double radius) where T : ArenaObject
        {
            foreach (var obj in Arena.GetNearbyObjects<T>(Position, radius))
            {
                if (Geometry.Geometry2D.Point.DistanceSquared(Position, obj.Position) <= UtilityFunctions.Square(radius))
                {
                    yield return obj;
                }
            }
        }
    }
}
