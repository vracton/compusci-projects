using DongUtility;

namespace NaturalSelection.Turns
{
    public class CarnivoreEat(Lynx owner, EcologyAnimal prey) : Eat(owner)
    {
        public override bool DoTurn()
        {
            if (prey == null || prey.IsDead)
            {
                return false;
            }
            double distanceSquared = (Owner.Position - prey.Position).MagnitudeSquared;
            if (distanceSquared > UtilityFunctions.Square(Owner.Stats.DistanceToEat))
            {
                return false;
            }

            double nutrition = prey.Stats.EnergyAsFood;
            prey.IsDead = true;
            Owner.Eat(nutrition);
            if (prey is Alien)
            {
                owner.LoseEnergy();
            }
            return true;
        }
    }
}
