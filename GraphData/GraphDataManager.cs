using DongUtility;
using System.Drawing;

namespace GraphData
{
    /// <summary>
    /// The central management class for handling and distributing graph data
    /// </summary>
    public class GraphDataManager
    {
        /// <summary>
        /// All functions that provide data updates
        /// </summary>
        private readonly UpdatingFunctions updatingFunctions = new();

        /// <summary>
        /// Retrives data from all updating functions
        /// </summary>
        public GraphDataPacket GetData() => updatingFunctions.GetData();

        private readonly List<IGraphPrototype> graphs = [];

        /// <summary>
        /// All graphs being tracked
        /// </summary>
        public IEnumerable<IGraphPrototype> Graphs => graphs;

        /// <summary>
        /// Writes the graph header to a binary file
        /// </summary>
        public void WriteGraphHeader(BinaryWriter bw)
        {
            bw.Write(graphs.Count);
            foreach (var graph in graphs)
            {
                var graphType = graph.GetGraphType();
                bw.Write((byte)graphType);
                graph.WriteToFile(bw);
            }
        }

        /// <summary>
        /// Copies all graphs and functions from another manager
        /// </summary>
        public void CopyGraphsFrom(GraphDataManager other)
        {
            graphs.AddRange(other.graphs);
            updatingFunctions.AddFunctions(other.updatingFunctions);
        }

        /// <summary>
        /// A simple function that returns a double, as for one axis of a graph
        /// </summary>
        public delegate double BasicFunction();
        /// <summary>
        /// A simple function that returns a list of doubles, as for a histogram
        /// </summary>
        public delegate List<double> ListFunction();
        /// <summary>
        /// A simple function that returns a vector, as for 3D graphing
        /// </summary>
        public delegate Vector VectorFunc();
        /// <summary>
        /// A pair of functions for x and y axes
        /// </summary>
        public struct BasicFunctionPair(BasicFunction xFunc, BasicFunction yFunc)
        {
            public BasicFunction XFunc { get; set; } = xFunc;
            public BasicFunction YFunc { get; set; } = yFunc;
        }

        /// <summary>
        /// Adds a single graph with a signle timeline with given x and y functions
        /// </summary>
        /// <param name="name">The name of the function (for the legend)</param>
        /// <param name="xAxis">The title of the horizontal axis</param>
        /// <param name="yAxis">The title of the vertical axis</param>
        public void AddSingleGraph(string name, Color color, BasicFunction xFunc, BasicFunction yFunc,
            string xAxis, string yAxis)
        {
            var info = new TimelineInfo(new TimelinePrototype(name, color),
                new BasicFunctionPair(xFunc, yFunc));

            AddGraph([info], xAxis, yAxis);
        }

        /// <summary>
        /// Adds a histogram with a given function to get all the data
        /// </summary>
        /// <param name="xAxis">The title of the horizontal axis</param>
        public void AddHist(int nBins, Color color, ListFunction allDataFunc, string xAxis)
        {
            graphs.Add(new HistogramPrototype(nBins, color, xAxis));

            void function(GraphDataPacket ds)
            {
                ds.AddSet(allDataFunc());
            }

            updatingFunctions.AddFunction(function);
        }

        /// <summary>
        /// A simple function that returns a string, as for text display
        /// </summary>
        public delegate string TextFunction();

        /// <summary>
        /// Adds an updating text display
        /// </summary>
        public void AddText(string title, Color color, TextFunction textFunc)
        {
            graphs.Add(new TextPrototype(title, color));

            void function(GraphDataPacket ds)
            {
                ds.AddTextData(textFunc());
            }

            updatingFunctions.AddFunction(function);
        }

        /// <summary>
        /// A container for a timeline and its associated functions
        /// </summary>
        public class TimelineInfo(TimelinePrototype timeline, BasicFunctionPair function)
        {
            public TimelinePrototype Timeline { get; set; } = timeline;
            public BasicFunctionPair Functions { get; set; } = function;
        }

        /// <summary>
        /// Adds a collection of timelines to a single graph with a common set of axes
        /// </summary>
        /// <param name="xAxis">The title of the horizontal axis</param>
        /// <param name="yAxis">The title of the vertical axis</param>
        public void AddGraph(IEnumerable<TimelineInfo> timelines, string xAxis, string yAxis)
        {
            var graph = new GraphPrototype(xAxis, yAxis);
            foreach (var timeline in timelines)
            {
                graph.AddTimeline(timeline.Timeline);
            }
            graphs.Add(graph);

            void function(GraphDataPacket ds)
            {
                foreach (var timeline in timelines)
                {
                    ds.AddData(timeline.Functions.XFunc());
                    ds.AddData(timeline.Functions.YFunc());
                }
            }
            updatingFunctions.AddFunction(function);
        }

        /// <summary>
        /// Adds a 3D graph with three timelines for x, y, and z
        /// </summary>
        /// <param name="name">The name of the overall graph</param>
        /// <param name="xAxis">The title on the horizontal axis</param>
        /// <param name="yAxis">The title on the vertical axis</param>
        public void Add3DGraph(string name, BasicFunction funcX, VectorFunc funcY, string xAxis, string yAxis)
        {
            var xVec = new TimelineInfo(new TimelinePrototype("x " + name, Color.Red),
                new BasicFunctionPair(funcX, () => funcY().X));

            var yVec = new TimelineInfo(new TimelinePrototype("y " + name, Color.Green),
                new BasicFunctionPair(funcX, () => funcY().Y));

            var zVec = new TimelineInfo(new TimelinePrototype("z " + name, Color.Blue),
                new BasicFunctionPair(funcX, () => funcY().Z));

            AddGraph([xVec, yVec, zVec], xAxis, yAxis);

        }

        /// <summary>
        /// Adds a leaderboard with given bars and a function to get the current values
        /// </summary>
        public void AddLeaderBoard(IEnumerable<LeaderBarPrototype> leaderBars, ListFunction function)
        {
            graphs.Add(new LeaderBoardPrototype(leaderBars));

            void newFunction(GraphDataPacket ds)
            {
                ds.AddSet(function());
            }

            updatingFunctions.AddFunction(newFunction);
        }
    }
}
