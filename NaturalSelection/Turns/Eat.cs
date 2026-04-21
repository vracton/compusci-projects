namespace NaturalSelection.Turns
{
    abstract public class Eat(EcologyAnimal owner) : EcologyTurn(owner)
    {
        public override double EnergyConsumption()
        {
            return Owner.Stats.EnergyToEat;
        }
    }
}
