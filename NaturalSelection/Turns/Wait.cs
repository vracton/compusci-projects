namespace NaturalSelection.Turns
{
    public class Wait(EcologyAnimal animal) : EcologyTurn(animal)
    {
        public override bool DoTurn()
        { return true; }

        public override double EnergyConsumption()
        {
            return 0;
        }
    }
}
