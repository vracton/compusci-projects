using DongUtility;
using Geometry.Geometry2D;

namespace NaturalSelection.Turns
{
    public class Move(EcologyAnimal owner, Vector2D newPosition) : EcologyTurn(owner)
    {
        public override bool DoTurn()
        {
            if (!Owner.Arena.TestPoint(newPosition.ToPoint()))
            {
                return false;
            }

            var displacement = newPosition.ToPoint() - Owner.Position;
            double distanceSquared = displacement.MagnitudeSquared;
            if (distanceSquared > UtilityFunctions.Square(Owner.Stats.MaxMovingDistance))
            {
                displacement = displacement.UnitVector() * Owner.Stats.MaxMovingDistance;
                newPosition = Owner.Position.PositionVector + displacement;
            }

            Owner.Arena.MoveObject(Owner, newPosition.ToPoint());
            return true;
        }

        public override double EnergyConsumption()
        {
            double distance = Point.Distance(Owner.Position, newPosition.ToPoint());
            double factor = distance / Owner.Stats.MaxMovingDistance;
            return Owner.Stats.EnergyToMove * factor;
        }
    }
}
