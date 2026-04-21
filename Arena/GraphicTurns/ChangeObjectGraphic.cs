namespace Arena.GraphicTurns
{
    /// <summary>
    /// A command to change the graphic of an object in the visualization.
    /// </summary>
    public class ChangeObjectGraphic : GraphicTurn
    {
        private readonly int layer;
        private readonly int graphicCode;
        private readonly int objCode;

        public ChangeObjectGraphic(int layer, int objCode, int graphicCode)
        {
            this.layer = layer;
            this.graphicCode = graphicCode;
            this.objCode = objCode;
        }

        internal ChangeObjectGraphic(BinaryReader br)
        {
            layer = br.ReadInt32();
            graphicCode = br.ReadInt32();
            objCode = br.ReadInt32();
        }

        protected override GraphicTurnTypes GraphicType => GraphicTurnTypes.ChangeObjectGraphic;

        public override void Do(IArenaDisplay display)
        {
            display.ChangeObjectGraphic(layer, objCode, graphicCode);
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(layer);
            bw.Write(graphicCode);
            bw.Write(objCode);
        }
    }
}
