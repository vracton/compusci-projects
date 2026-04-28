namespace PhysicsUtility.GridUtility
{
    /// <summary>
    /// A single unit of a generic grid
    /// Each unit has a value and the ability to store a nextValue for better updating
    /// </summary>
    public class Cell(double value)
    {
        virtual public double Value { get; set; } = value;

        // This is the value it will have once all updates are done
        private double nextValue;
        
        /// <summary>
        /// Set the next value, which will not be updated until we call Update()
        /// </summary>
        public virtual void SetNextValue(double val)
        {
            if (double.IsNaN(val) || double.IsInfinity(val))
            {
                throw new InvalidOperationException("Invalid number passed to nextValue!");
            }
            nextValue = val;
        }

        /// <summary>
        /// Changes the value of the cell to nextValue
        /// This separates the calculation from the updates
        /// </summary>
        public void Update()
        {
            Value = nextValue;
        }
    }
}
