namespace NaturalSelection.Turns
{
    public class GiveBirth(EcologyAnimal owner) : EcologyTurn(owner)
    {
        public override bool DoTurn()
        {
            var litter = Owner.GiveBirth();

            foreach (var ani in litter)
            {
                ani.Position = Owner.Position;
                Owner.Arena.AddObjectDelay(ani);
            }
            return true;
        }

        public override double EnergyConsumption()
        {
            return 0;
        }
    }
}
