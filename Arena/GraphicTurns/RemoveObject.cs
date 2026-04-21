namespace Arena.GraphicTurns
{
    /// <summary>
    /// Removes an object from the visualization.
    /// </summary>
    public class RemoveObject : GraphicTurn
    {
        private readonly int layer;
        private readonly int objCode;

        public RemoveObject(ArenaObject obj)
        {
            layer = obj.Layer;
            objCode = obj.Code;
        }

        public RemoveObject(int layer, int objCode)
        {
            this.layer = layer;
            this.objCode = objCode;
        }

        protected override GraphicTurnTypes GraphicType => GraphicTurnTypes.RemoveObject;

        public override void Do(IArenaDisplay display)
        {
            display.RemoveObject(layer, objCode);
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(layer);
            bw.Write(objCode);
        }

        internal RemoveObject(BinaryReader br)
        {
            layer = br.ReadInt32();
            objCode = br.ReadInt32();
        }
    }
}
