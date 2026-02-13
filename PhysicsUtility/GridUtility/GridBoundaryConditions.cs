using DongUtility;

namespace PhysicsUtility.GridUtility
{
    /// <summary>
    /// Abstract class for boundary conditions of a grid
    /// </summary>
    /// <typeparam name="TCell">The type of cell used in the grid</typeparam>
    abstract public class GridBoundaryConditions<TCell> where TCell : Cell
    {
        /// <summary>
        /// Calculates the value of a cell that is out of bounds of a grid, which will be called by the grid in question
        /// </summary>
        /// <param name="coord">The out-of-bounds coordinate</param>
        /// <param name="grid">The grid in question</param>
        /// <returns>The value to be used for that boundary condition</returns>
        abstract public double GetOutOfBoundsValue(Coordinate3D coord, Grid<TCell> grid);
    }
}
