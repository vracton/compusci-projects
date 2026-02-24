using DongUtility;

namespace PhysicsUtility.GridUtility
{
    /// <summary>
    /// A general grid class for solving differential equations
    /// </summary>
    /// <typeparam name="TCell">The type of cell used in this grid, where the calculations take place</typeparam>
    abstract public class Grid<TCell> where TCell : Cell
    {
        /// <summary>
        /// A three-dimensional array of cells
        /// </summary>
        public TCell[,,] Cells { get; init; }
        /// <summary>
        /// The spacing between consecutive cells, stored as a vector for convenience
        /// </summary>
        public Vector Spacing { get; init; }
        /// <summary>
        /// The origin of the grid, i.e. the coordinate of the corner of cell [0, 0, 0]
        /// Note that this is the lower corner, not the center
        /// </summary>
        public Vector Origin { get; init; }

        /// <summary>
        /// The volume occupied by each cell
        /// </summary>
        public double VolumePerCell => Spacing.X * Spacing.Y * Spacing.Z;
        /// <summary>
        /// The boundary conditions of the grid
        /// </summary>
        public GridBoundaryConditions<TCell>? BoundaryConditions { get; set; }

        /// <summary>
        /// Number of cells and size of cells in three dimensions
        /// </summary>
        /// <param name="dx">The size of each cell in the x direction</param>
        /// <param name="nx"The number of cells in the x direction</param>
        /// <param name="origin">The coordinate of the corner of cell [0, 0, 0]</param>
        public Grid(int nx, int ny, int nz, double dx, double dy, double dz, 
            Vector origin, GridBoundaryConditions<TCell>? boundary = null)
        {
            Cells = new TCell[nx, ny, nz];
            Spacing = new Vector(dx, dy, dz);
            Origin = origin;
            BoundaryConditions = boundary;
            InitializeCells();
        }

        /// <summary>
        /// Initialization procedure for the cells
        /// The use of a separate TCell avoids the inherited constructor initialization problem
        /// </summary>
        private void InitializeCells()
        {
            for (int ix = 0; ix < Cells.GetLength(0); ++ix)
                for (int iy = 0; iy < Cells.GetLength(1); ++iy)
                    for (int iz = 0; iz < Cells.GetLength(2); ++iz)
                    {
                        Cells[ix, iy, iz] = InitializeCell(ix, iy, iz);
                    }
        }

        /// <summary>
        /// An intialization function for each cell, called by the constructor
        /// </summary>
        abstract protected TCell InitializeCell(int ix, int iy, int iz);

        /// <summary>
        /// Transforms a coordinate in space to a coordinate in the grid
        /// </summary>
        public Coordinate3D GetCellLocation(Vector vec)
        {
            return GetCellLocation(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Transforms a coordinate in space to a coordinate in the grid
        /// </summary>
        public Coordinate3D GetCellLocation(double x, double y, double z)
        {
            return new Coordinate3D(
                    GetLocation1D(Origin.X, Cells.GetLength(0), Spacing.X, x),
                    GetLocation1D(Origin.Y, Cells.GetLength(1), Spacing.Y, y),
                    GetLocation1D(Origin.Z, Cells.GetLength(2), Spacing.Z, z)
                );
        }

        /// <summary>
        /// Gets the cell at a given coordinate
        /// </summary>
        public TCell GetCell(Coordinate3D coord)
        {
            if (!IsValid(coord))
            {
                throw new ArgumentOutOfRangeException(nameof(coord));
            }
            return Cells[coord.X, coord.Y, coord.Z];
        }

        /// <summary>
        /// Gets the cell at a given coordinate
        /// </summary>
        public TCell GetCell(int ix, int iy, int iz)
        {
            return GetCell(new Coordinate3D(ix, iy, iz));
        }

        /// <summary>
        /// Gets the cell at a given spatial position
        /// </summary>
        public TCell GetCellByLocation(double x, double y, double z)
        {
            return GetCell(GetCellLocation(x, y, z));
        }

        /// <summary>
        /// Returns the value in a given cell
        /// </summary>
        public double GetValue(int ix, int iy, int iz)
        {
            var coord = new Coordinate3D(ix, iy, iz);
            if (IsValid(coord))
            {
                return GetCell(coord).Value;
            }
            else
            {
                if (BoundaryConditions == null)
                {
                    throw new NullReferenceException("Boundary conditions have not been set");
                }
                return BoundaryConditions.GetOutOfBoundsValue(coord, this);
            }
        }

        /// <summary>
        /// Checks whether a given coordinate points to a valid part of the grid
        /// </summary>
        public bool IsValid(Coordinate3D coord)
        {
            return coord.X >= 0 && coord.X < Cells.GetLength(0)
                && coord.Y >= 0 && coord.Y < Cells.GetLength(1)
                && coord.Z >= 0 && coord.Z < Cells.GetLength(2);
        }

        /// <summary>
        /// Finds the cell number for a given position along a single axis
        /// </summary>
        /// <param name="origin">The origin of the axis</param>
        /// <param name="nCells">The number of cells along the axis</param>
        /// <param name="cellSize">The size of each cell in this dimension</param>
        /// <param name="location">The location in question that we wish to find the cell for</param>
        /// <returns>The index of the cell in this dimension</returns>
        private static int GetLocation1D(double origin, int nCells, double cellSize, double location)
        {
            double position = (location - origin) / cellSize;
            int cellNumber = (int)Math.Floor(position);
            if (cellNumber < 0 || cellNumber >= nCells)
            {
                throw new ArgumentOutOfRangeException(nameof(location));
            }
            return cellNumber;
        }
    }
}
