namespace GraphData
{
    /// <summary>
    /// A data interface that combines reading some graphs from files and creating other graphs in real time
    /// </summary>
    public class HybridGraphDataInterface(IGraphDataInterface fileInterface, IGraphDataInterface realTimeInterface) : IGraphDataInterface
    {
        public IEnumerable<IGraphPrototype> Graphs
        {
            get
            {
                foreach (var graph in fileInterface.Graphs)
                {
                    yield return graph;
                }
                foreach (var graph in realTimeInterface.Graphs)
                {
                    yield return graph;
                }
            }
        }

        public GraphDataPacket GetData()
        {
            return GraphDataPacket.Combine(fileInterface.GetData(), realTimeInterface.GetData());
        }
    }
}
