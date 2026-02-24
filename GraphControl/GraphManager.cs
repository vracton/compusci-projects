using GraphData;
using System;
using System.Windows.Controls;
using static WPFUtility.UtilityFunctions;

namespace GraphControl
{
    /// <summary>
    /// A class that handles getting data for graphs and updating them.
    /// </summary>
    /// <param name="dataManager">
    /// This is where the data comes from
    /// </param>
    public class GraphManager(IGraphDataInterface dataManager, CompositeGraph graphs)
    {

        /// <summary>
        /// Initializes the graphs based on the prototypes found in the data manager
        /// Prototypes are non-graphical abstract objects that define how to create a graph
        /// </summary>
        public void Initialize()
        {
            var prototypes = dataManager.Graphs;
            foreach (var prototype in prototypes)
            {
                var control = GetGraphFromPrototype(prototype);
                graphs.AddGraph(control);
            }
        }

        /// <summary>
        /// Generates the proper type of graph from a given prototype
        /// Has to be done with switch statements to avoid the graph data depending on the graphic libraries,
        /// and also to allow reading from a file
        /// </summary>
        private static UserControl GetGraphFromPrototype(IGraphPrototype prototype)
        {
            switch (prototype.GetGraphType())
            {
                case IGraphPrototype.GraphType.Graph:
                    if (prototype is not GraphPrototype protoGraph)
                    {
                        throw new ArgumentException("Invalid prototype passed to GetGraphFromPrototype");
                    }
                    var gu = new GraphUnderlying(protoGraph.XAxisTitle, protoGraph.YAxisTitle);
                    foreach (var timeline in protoGraph.Timelines)
                    {
                        var newTimeline = new Timeline(timeline.Name, ConvertColor(timeline.Color));
                        gu.AddTimeline(newTimeline);
                    }
                    return new Graph(gu);

                case IGraphPrototype.GraphType.Histogram:
                    if (prototype is not HistogramPrototype protoHist)
                    {
                        throw new ArgumentException("Invalid prototype passed to GetGraphFromPrototype");
                    }
                    var hist = new Histogram(protoHist.NBins, ConvertColor(protoHist.Color),
                        protoHist.XAxisTitle);
                    return new Graph(hist);

                case IGraphPrototype.GraphType.Text:
                    if (prototype is not TextPrototype protoText)
                    {
                        throw new ArgumentException("Invalid prototype passed to GetGraphFromPrototype");
                    }
                    var text = new UpdatingText
                    {
                        Title = protoText.Title,
                        Color = ConvertColor(protoText.Color)
                    };
                    return text;

                case IGraphPrototype.GraphType.LeaderBoard:
                    if (prototype is not LeaderBoardPrototype protoBoard)
                    {
                        throw new ArgumentException("Invalid prototype passed to GetGraphFromPrototype");
                    }
                    var leaderBoard = new LeaderBoardControl();
                    foreach (var leaderBar in protoBoard.Prototypes)
                    {
                        leaderBoard.AddEntry(leaderBar.Name, ConvertColor(leaderBar.Color));
                    }
                    return leaderBoard;

                default:
                    throw new NotImplementedException("Should never reach here");
            }
        }

        /// <summary>
        /// Adds all the data from the data manager to the graphs
        /// </summary>
        public void AddData()
        {
            graphs.Update(dataManager.GetData());
        }
    }
}
