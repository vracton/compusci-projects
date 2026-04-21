using Geometry.Geometry2D;

namespace Arena
{
    /// <summary>
    /// A non-moving arena object.
    /// </summary>
    abstract public class StationaryObject : ArenaObject
    {
        public StationaryObject(int graphicCode, int layer, double width, double height)
            : base(graphicCode, layer, width, height)
        { }

        public StationaryObject(int graphicCode, int layer, Shape2D shape)
            : base(graphicCode, layer, shape)
        { }

        public override bool IsUpdating => false;
    }
}
