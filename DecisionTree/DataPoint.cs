namespace DecisionTree
{
    /// <summary>
    /// Class holding information for a single event
    /// </summary>
    /// <param name="nvar">The number of different variables in the point</param>
    public class DataPoint(int nvar)
    {
        // An array of doubles, representing the relevant variables for the event
        public double[] Variables { get; set; } = new double[nvar];
    }
}
