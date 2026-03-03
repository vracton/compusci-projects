namespace GraphData
{
    /// <summary>
    /// A graph data interface for real-time graph creation.  Just uses the GraphDataManager to do all the dirty work
    /// </summary>
    /// <param name="manager"></param>
    public class RealTimeGraphDataInterface(GraphDataManager manager) : IGraphDataInterface
    {
        /// <summary>
        /// The GraphDataManager that actually handles the work
        /// </summary>
        public GraphDataManager Manager { get; } = manager;
        public IEnumerable<IGraphPrototype> Graphs => Manager.Graphs;

        public GraphDataPacket GetData() => Manager.GetData();
    }
}
