using DongUtility;
using Geometry.Geometry2D;

namespace Arena
{
    /// <summary>
    /// A grid implementation for the arena. This is used to speed up collision detection.
    /// </summary>
    internal class ArenaGrid
    {
        private readonly ArenaGridCell[,] grid;

        private readonly int width;
        private readonly int height;

        private readonly double xdiv;
        private readonly double ydiv;

        public ArenaGrid(ArenaEngine arena, int xDivs, int yDivs)
        {
            width = xDivs;
            height = yDivs;

            grid = new ArenaGridCell[xDivs, yDivs];

            xdiv = arena.Width / xDivs;
            ydiv = arena.Height / yDivs;

            for (int ix = 0; ix < xDivs; ++ix)
                for (int iy = 0; iy < yDivs; ++iy)
                {
                    grid[ix, iy] = new ArenaGridCell();
                }
        }

        /// <summary>
        /// Locates all cells that an object touches.
        /// </summary>
        private IEnumerable<ArenaGridCell> LocateCells(ArenaObject obj)
        {
            return LocateCells(obj.Shape);
        }

        /// <summary>
        /// Locates all cells that a shape touches.
        /// </summary>
        private IEnumerable<ArenaGridCell> LocateCells(Shape2D shape)
        {
            var upperLeftCoord = new Point(shape.Range.X.Min, shape.Range.Y.Min);
            var lowerRightCoord = new Point(shape.Range.X.Max, shape.Range.Y.Max);

            var upperLeft = GetGridCell(upperLeftCoord);
            var lowerRight = GetGridCell(lowerRightCoord);

            int minX = upperLeft.X;
            int minY = upperLeft.Y;
            int maxX = lowerRight.X;
            int maxY = lowerRight.Y;

            for (int ix = minX; ix <= maxX; ++ix)
                for (int iy = minY; iy <= maxY; ++iy)
                {
                    yield return grid[ix, iy];
                }
        }

        /// <summary>
        /// Locates the cell that contains a point.
        /// </summary>
        private ArenaGridCell LocateCell(Point position)
        {
            var coord = GetGridCell(position);

            return grid[coord.X, coord.Y];
        }

        /// <summary>
        /// Gets the grid cell that contains a point.
        /// </summary>
        private Coordinate2D GetGridCell(Point position)
        {
            int xCoord = (int)(position.X / xdiv);
            int yCoord = (int)(position.Y / ydiv);
            xCoord = Math.Clamp(xCoord, 0, width - 1);
            yCoord = Math.Clamp(yCoord, 0, height - 1);
            return new Coordinate2D(xCoord, yCoord);
        }

        /// <summary>
        /// Adds an object to the grid. This should be called when an object is created or moved.
        /// </summary>
        public void AddObject(ArenaObject obj)
        {
            foreach (var cell in LocateCells(obj))
            {
                cell.AddObject(obj);
            }
        }

        /// <summary>
        /// Removes an object from the grid. This should be called when an object is destroyed or moved.
        /// </summary>
        public void RemoveObject(ArenaObject obj)
        {
            foreach (var cell in LocateCells(obj))
            {
                cell.RemoveObject(obj);
            }
        }

        /// <summary>
        /// Moves an object - do this instead of updating Position directly!
        /// </summary>
        public void MoveObject(ArenaObject obj, Point newPosition)
        {
            var oldCells = LocateCells(obj);
            var newRect = obj.Shape.TranslateToPoint(newPosition);
            var newCells = LocateCells(newRect);

            // This seems easier
            foreach (var cell in oldCells)
            {
                cell.RemoveObject(obj);
            }
            foreach (var cell in newCells)
            {
                cell.AddObject(obj);
            }

            obj.Position = newPosition;
        }

        /// <summary>
        /// Get all objects of a given type near a specific point in the grid
        /// </summary>
        public IEnumerable<T> GetNearby<T>(Point position, double radius) where T : ArenaObject
        {
            var lookRectangle = new AlignedRectangle(position, radius * 2, radius * 2);

            var alreadyDone = new HashSet<ArenaObject>();

            var cells = LocateCells(lookRectangle);
            foreach (var cell in cells)
                foreach (var obj in cell.Objects)
                {
                    if (obj is T resultObject && !alreadyDone.Contains(obj))
                    {
                        alreadyDone.Add(obj);
                        yield return resultObject;
                    }
                }
        }
    }
}
