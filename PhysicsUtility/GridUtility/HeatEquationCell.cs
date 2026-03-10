namespace PhysicsUtility.GridUtility
{
    /// <summary>
    /// A cell that can be used in a heat equation simulation
    /// It needs to store a "velocity" as well as a value to use for the second time derivative
    /// </summary>
    public class HeatEquationCell(double value) : Cell(value)
    {
        private double velocity = 0;
        /// <summary>
        /// Applies an acceleration to the cell
        /// </summary>
        /// <param name="damping">A damping coefficient, which will damp the velocity by that percentage in one second</param>
        public void ApplyAcceleration(double acceleration, double timeIncrement, double damping = 1)
        {
            velocity += acceleration * timeIncrement;
            velocity *= Math.Pow(damping, timeIncrement);
        }

        /// <summary>
        /// Updates the value of the cell, which needs to happen after acceleration is applied.
        /// </summary>
        public void UpdateValue(double timeIncrement)
        {
            SetNextValue(Value + velocity * timeIncrement);
        }
    }
}
