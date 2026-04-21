using Arena;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// A simple obstacle class that is not passable by any other object.
    /// </summary>
    internal class Obstacle : StationaryObject
    {
        private const int obstacleLayer = 1;
        private const int graphicCode = 3;

        public Obstacle(PredatorPreyEngine arena, double width, double height) :
            base(graphicCode, obstacleLayer, width, height)
        {
            Arena = arena;
        }

        public override string Name => "Obstacle";

        public override bool IsPassable(ArenaObject? mover = null)
        {
            return false;
        }
    }
}
