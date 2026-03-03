namespace GraphData
{
    /// <summary>
    /// An interface for a class that accesses data for drawing graphs, whether real time or from a file
    /// </summary>
    public interface IGraphDataInterface
    {
        /// <summary>
        /// All the data from a particular point in time
        /// </summary>
        public GraphDataPacket GetData();

        /// <summary>
        /// A list of the prototypes of all the updating objects that the data will be written to
        /// </summary>
        public IEnumerable<IGraphPrototype> Graphs { get; }
    }
}
