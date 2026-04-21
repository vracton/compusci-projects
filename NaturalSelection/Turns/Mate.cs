namespace NaturalSelection.Turns
{
    public class Mate(EcologyAnimal owner, EcologyAnimal mate) : EcologyTurn(owner)
    {
        public override bool DoTurn()
        {
            return Owner.Mate(mate);
        }

        public override double EnergyConsumption()
        {
            return Owner.Stats.EnergyToMate;
        }
    }
}
