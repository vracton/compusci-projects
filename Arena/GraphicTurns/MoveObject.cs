using DongUtility;
using Geometry.Geometry2D;

namespace Arena.GraphicTurns
{
    /// <summary>
    /// Move an existing object to another space
    /// </summary>
    public class MoveObject : GraphicTurn
    {
        private readonly int layer;
        private readonly int objCode;
        private readonly Point coord;

        public MoveObject(ArenaObject obj, Point newLocation) :
            this(obj.Layer, obj.Code, newLocation)
        { }

        public MoveObject(int layer, int objCode, Point coord)
        {
            this.layer = layer;
            this.objCode = objCode;
            this.coord = coord;
        }

        protected override GraphicTurnTypes GraphicType => GraphicTurnTypes.MoveObject;

        public override void Do(IArenaDisplay display)
        { 
            display.MoveObject(layer, objCode, coord);
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(layer);
            bw.Write(objCode);
            bw.Write(coord);
        }
        internal MoveObject(BinaryReader br)
        {
            layer = br.ReadInt32();
            objCode = br.ReadInt32();
            coord = br.ReadPoint2D();
        }
    }
}
