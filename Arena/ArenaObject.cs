using Geometry.Geometry2D;

namespace Arena
{
    /// <summary>
    /// This is the base class for all objects in the arena.
    /// </summary>
    abstract public class ArenaObject
    {
        /// <summary>
        /// The arena that the object belongs to
        /// </summary>
        public ArenaEngine Arena { get; set; }

        public Point Position
        {
            get
            {
                return Shape.Center;
            }
            set
            {
                Shape.Center = value;
            }
        }

        /// <summary>
        /// The shape of the object
        /// </summary>
        public Shape2D Shape { get; internal set; }

        abstract public string Name { get; }

        /// <summary>
        /// A counter used to assign unique codes to objects
        /// </summary>
        private static int counter = 0;

        private static readonly Lock locker = new();

        /// <summary>
        /// A numerical code for each object
        /// </summary>
        public int Code { get; }
        /// <summary>
        /// A numerical code for the bitmap that represents the object
        /// </summary>
        public int GraphicCode { get; protected set; }
        /// <summary>
        /// A numerical code for the visual layer that the object is on
        /// </summary>
        public int Layer { get; }

        /// <summary>
        /// Whether the object updates each round
        /// </summary>
        abstract public bool IsUpdating { get; }
        /// <summary>
        /// Whether other objects can pass through this object
        /// </summary>
        /// <param name="mover">The object trying to pass through - in case it matters</param>
        abstract public bool IsPassable(ArenaObject? mover = null);

        public ArenaObject(int graphicCode, int layer, double width, double height) :
            this(graphicCode, layer, new AlignedRectangle(new Point(0, 0), new Point(width, height)))
        {
        }

        public ArenaObject(int graphicCord, int layer, Shape2D shape)
        {
            Arena = null!;
            GraphicCode = graphicCord;
            Layer = layer;
            Shape = shape;
            lock (locker)
            {
                Code = counter++;
            }
        }

        /// <summary>
        /// Checks if a coordinate is inside the object. If the mover is null, it checks if the coordinate is inside the object.
        /// </summary>
        /// <param name="mover">The object that is attempting to move to the location</param>
        public bool Occupies(Point coordinate, ArenaObject? mover)
        {
            if (mover == null)
            {
                return Shape.Inside(coordinate);
            }
            if (!IsPassable(mover))
            {
                var newShape = mover.Shape.TranslateToPoint(coordinate);
                return Shape.Intersects(newShape);                
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the object overlaps with another object
        /// </summary>
        public bool Overlaps(ArenaObject other)
        {
            return Shape.Intersects(other.Shape);
        }
    }
}
