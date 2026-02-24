using DongUtility;

namespace PhysicsUtility.GridUtility
{
    /// <summary>
    /// A grid that solves the heat equation
    /// </summary>
    /// <typeparam name="TCell"></typeparam>
    /// <param name="nx">Number of cells in the x direction</param>
    /// <param name="dx">The size of each cell in the x direction</param>
    /// <param name="speed">The speed of propagation</param>
    /// <param name="origin">The origin of the grid, which is the lower left corner</param>
    /// <param name="boundary">The boundary conditions which are applied</param>
    abstract public class HeatEquationGrid<TCell>(int nx, int ny, int nz, double dx, double dy, double dz, double speed, Vector origin,
        GridBoundaryConditions<TCell> boundary) : Grid<TCell>(nx, ny, nz, dx, dy, dz, origin, boundary) where TCell : HeatEquationCell
    {

        /// <summary>
        /// Multiplies the velocity by this factor every second
        /// </summary>
        public double DampingCoefficient { get; set; } = 1;

        /// <summary>
        /// The speed of propagation
        /// </summary>
        public double Speed { get; init; } = speed;

        private readonly double constant = speed * speed;

        // First derivatives store the difference between index i-1 and i
        // Each element stores the x, y, z first derivatives as a Vector
        private readonly Vector[,,] firstDerivatives = new Vector[nx + 1, ny + 1, nz + 1];

        /// <summary>
        /// Updates all cells in the grid
        /// </summary>
        public void UpdateAll(double timeIncrement)
        {
            // Do first derivatives
            CalculateFirstDerivatives();

            // Calculate accelerations for all cells
            //for (int ix = 0; ix < Cells.GetLength(0); ++ix)
            Parallel.For(0, Cells.GetLength(0), ix =>
            {
                Parallel.For(0, Cells.GetLength(1), iy =>
                {
                    //for (int iy = 0; iy < Cells.GetLength(1); ++iy)
                    for (int iz = 0; iz < Cells.GetLength(2); ++iz)
                    {
                        UpdateCell(ix, iy, iz, timeIncrement);
                    }
                });
            });

            // Now calculate new values
            foreach (var cell in Cells)
            {
                cell.UpdateValue(timeIncrement);
                cell.Update();
            }
        }

        /// <summary>
        /// Update a single cell
        /// </summary>
        private void UpdateCell(int ix, int iy, int iz, double timeIncrement)
        {
            double d2xdt2 = (firstDerivatives[ix + 1, iy, iz].X - firstDerivatives[ix, iy, iz].X) / Spacing.X;
            double d2ydt2 = (firstDerivatives[ix, iy + 1, iz].Y - firstDerivatives[ix, iy, iz].Y) / Spacing.Y;
            double d2zdt2 = (firstDerivatives[ix, iy, iz + 1].Z - firstDerivatives[ix, iy, iz].Z) / Spacing.Z;

            double laplacian = d2xdt2 + d2ydt2 + d2zdt2;

            var cell = Cells[ix, iy, iz];
            cell.ApplyAcceleration(constant * laplacian, timeIncrement, DampingCoefficient);
        }

        /// <summary>
        /// Finds the first derivatives of all cells
        /// </summary>
        private void CalculateFirstDerivatives()
        {
            // for (int ix = 0; ix <= Cells.GetLength(0); ++ix)
            Parallel.For(0, Cells.GetLength(0), ix =>
            {
                //for (int iy = 0; iy <= Cells.GetLength(1); ++iy)
                Parallel.For(0, Cells.GetLength(1), iy =>
                {
                    for (int iz = 0; iz <= Cells.GetLength(2); ++iz)
                    {
                        double value = GetCellForFirstDerivative(ix, iy, iz).Value;
                        firstDerivatives[ix, iy, iz] = new Vector(
                            (value - GetCellForFirstDerivative(ix - 1, iy, iz).Value) / Spacing.X,
                            (value - GetCellForFirstDerivative(ix, iy - 1, iz).Value) / Spacing.Y,
                            (value - GetCellForFirstDerivative(ix, iy, iz - 1).Value) / Spacing.Z
                            );
                    }
                });
            });
        }

        private HeatEquationCell GetCellForFirstDerivative(int coordX, int coordY, int coordZ)
        {
            // For now, assume zero first derivatives
            if (coordX < 0)
                coordX = 0;
            if (coordY < 0)
                coordY = 0;
            if (coordZ < 0)
                coordZ = 0;
            if (coordX >= Cells.GetLength(0))
                coordX = Cells.GetLength(0) - 1;
            if (coordY >= Cells.GetLength(1))
                coordY = Cells.GetLength(1) - 1;
            if (coordZ >= Cells.GetLength(2))
                coordZ = Cells.GetLength(2) - 1;

            return Cells[coordX, coordY, coordZ];
        }
    }
}
