
namespace GraphData
{
    /// <summary>
    /// An interface for any updating object that can be used to read and write graph data to files as well as for real-time interfaces
    /// </summary>
    public interface IGraphPrototype
    {
        public enum GraphType : byte { Graph, Histogram, Text, LeaderBoard }

        public GraphType GetGraphType();

        public void WriteToFile(BinaryWriter bw);

        static public IGraphPrototype ReadFromFile(BinaryReader br)
        {
            byte typeCode = br.ReadByte();
            var graphType = (GraphType)typeCode;

            // Need to do it this way for reading from files - no way to make it fully general
            return graphType switch
            {
                GraphType.Graph => new GraphPrototype(br),
                GraphType.Histogram => new HistogramPrototype(br),
                GraphType.Text => new TextPrototype(br),
                GraphType.LeaderBoard => new LeaderBoardPrototype(br),
                _ => throw new NotImplementedException("Should never reach here")
            };
                
        }
    }
}
