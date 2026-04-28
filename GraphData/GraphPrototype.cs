namespace GraphData
{
    /// <summary>
    /// The information about a graph that needs to be saved, separated from its graphical implementation
    /// Contains the information needed to make a graph but does not draw the graph itself
    /// </summary>
    public class GraphPrototype : IGraphPrototype
    {
        public string XAxisTitle { get; }
        public string YAxisTitle { get; }
        /// <summary>
        /// All the individual graphs (timelines) plotted on a single set of axes
        /// </summary>
        public List<TimelinePrototype> Timelines { get; } = [];

        public GraphPrototype(string xAxisTitle, string yAxisTitle)
        {
            XAxisTitle = xAxisTitle;
            YAxisTitle = yAxisTitle;
        }

        /// <summary>
        /// Adds a new graph (timeline) to the axes
        /// </summary>
        public void AddTimeline(TimelinePrototype timeline)
        {
            Timelines.Add(timeline);
        }

        IGraphPrototype.GraphType IGraphPrototype.GetGraphType()
        {
            return IGraphPrototype.GraphType.Graph;
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(XAxisTitle);
            bw.Write(YAxisTitle);
            bw.Write(Timelines.Count);
            foreach (var timeline in Timelines)
            {
                timeline.WriteToFile(bw);
            }
        }

        internal GraphPrototype(BinaryReader br)
        {
            XAxisTitle = br.ReadString();
            YAxisTitle = br.ReadString();
            int nTimelines = br.ReadInt32();
            for (int i = 0; i < nTimelines; ++i)
            {
                var timeline = new TimelinePrototype(br);
                Timelines.Add(timeline);
            }
        }
    }
}
