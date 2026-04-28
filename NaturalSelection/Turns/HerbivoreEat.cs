namespace NaturalSelection.Turns
{
    public class HerbivoreEat(EcologyAnimal owner) : Eat(owner)
    {
        public override bool DoTurn()
        {
            var cell = ((EcologyArena)(Owner.Arena)).CurrentCell(Owner.Position.PositionVector);

            double food = Owner.DailyEnergyLeft();
            food = cell.EatFood(food);
            return Owner.Eat(food);
        }
    }
}
