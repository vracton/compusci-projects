using Arena;

namespace NaturalSelection
{
    abstract public class EcologyTurn(EcologyAnimal owner) : Turn(owner)
    {
        protected new EcologyAnimal Owner => (EcologyAnimal)(base.Owner); 

        abstract public double EnergyConsumption();
    }
}
