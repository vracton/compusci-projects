namespace Arena.GraphicTurns
{
    /// <summary>
    /// Rotates an object in the visualization.
    /// </summary>
    public class RotateObject : GraphicTurn
    {
        private readonly int layer;
        private readonly int objCode;
        private readonly double angle;

        public RotateObject(ArenaObject obj, double angle) :
            this(obj.Layer, obj.Code, angle)
        { }

        public RotateObject(int layer, int objCode, double angle)
        {
            this.layer = layer;
            this.objCode = objCode;
            this.angle = angle;
        }

        protected override GraphicTurnTypes GraphicType => GraphicTurnTypes.MoveObject;

        public override void Do(IArenaDisplay display)
        {
            display.RotateObject(layer, objCode, angle);
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(layer);
            bw.Write(objCode);
            bw.Write(angle);
        }

        internal RotateObject(BinaryReader br)
        {
            layer = br.ReadInt32();
            objCode = br.ReadInt32();
            angle = br.ReadDouble();
        }
    }
}
