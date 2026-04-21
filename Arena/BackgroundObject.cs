namespace Arena
{
    public class BackgroundObject(int graphicCode) : StationaryObject(graphicCode, backLayer, 0, 0)
    {
        public override bool IsPassable(ArenaObject? mover) => true;

        /// <summary>
        /// The background layer is always 0
        /// </summary>
        private const int backLayer = 0;

        public override string Name => "Background";
    }
}
