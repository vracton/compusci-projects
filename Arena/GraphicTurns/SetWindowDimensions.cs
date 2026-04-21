namespace Arena.GraphicTurns
{
    /// <summary>
    /// Sets the dimensions of the window and arena in the visualization.
    /// </summary>
    public class SetWindowDimensions : GraphicTurn
    {
        private readonly double windowWidth;
        private readonly double windowHeight;
        private readonly double arenaWidth;
        private readonly double arenaHeight;
        protected override GraphicTurnTypes GraphicType => GraphicTurnTypes.SetWindowDimensions;

        public override void Do(IArenaDisplay display)
        {
            display.SetWindowDimensions(windowWidth, windowHeight, arenaWidth, arenaHeight);
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(windowWidth);
            bw.Write(windowHeight);
            bw.Write(arenaWidth);
            bw.Write(arenaHeight);
        }

        internal SetWindowDimensions(BinaryReader br)
        {
            windowWidth = br.ReadDouble();
            windowHeight = br.ReadDouble();
            arenaWidth = br.ReadDouble();
            arenaHeight = br.ReadDouble();
        }
    }
}
