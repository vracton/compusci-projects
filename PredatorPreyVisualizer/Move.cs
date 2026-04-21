using Arena;
using Geometry.Geometry2D;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// A basic move command for the predator-prey simulation.
    /// </summary>
    class Move(PredatorPreyOrganism org, Point newPosition) : Turn(org)
    {
        public override bool DoTurn()
        {
            if (Owner.Arena.TestPoint(newPosition))
            {
                Owner.Arena.MoveObject(Owner, newPosition);
                var angle = ((PredatorPreyOrganism)Owner).Velocity.Azimuthal;
                //Owner.Arena.RotateObject(Owner, angle);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
