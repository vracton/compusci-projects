using System.Collections.Generic;
using System.Windows.Controls;
using GraphData;

namespace GraphControl
{
    /// <summary>
    /// A collection of Graphs, implemented as a vertical grid of graphs
    /// </summary>
    public partial class CompositeGraph : UserControl, IUpdating
    {
        public List<IUpdating> graphs = [];

        public CompositeGraph()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Get the graph that is at a specific index
        /// </summary>
        public IUpdating GetGraph(int index)
        {
            return graphs[index];
        }

        public int GetNGraphs()
        {
            return graphs.Count;
        }

        public void AddGraph(UserControl graph)
        {
            AddToGraphList(graph);
        }

       /// <summary>
       /// Adds the object to the list of graphs and adjusts things accordingly
       /// </summary>
        private void AddToGraphList(UserControl control)
        {
            GraphPanel.RowDefinitions.Add(new RowDefinition());
            GraphPanel.Children.Add(control);
            Grid.SetRow(control, GraphPanel.RowDefinitions.Count - 1);
            

            if (control is IUpdating updating)
            {
                graphs.Add(updating);
            }
        }

        /// <summary>
        /// Update all graphs in the set
        /// GraphDataPacket will have to match the expectations of the graph
        /// </summary>
        public void Update(GraphDataPacket data)
        {
            foreach (var graph in graphs)
            {
                graph.Update(data);
            }
        }

        /// <summary>
        /// Clear all graphs and empty the collection
        /// </summary>
        public void Clear()
        {
            graphs.Clear();
            GraphPanel.Children.Clear();
            GraphPanel.RowDefinitions.Clear();
        }
    }
}
