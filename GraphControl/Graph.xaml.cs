using GraphData;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : UserControl, IUpdating
    {
        /// <summary>
        /// The thing that actually does the drawing, via an interface
        /// </summary>
        public IGraphInterface InternalGraph { get; private set; }

        public Graph(IGraphInterface graph)
        {
            InitializeComponent();
            InternalGraph = graph;
        }

        /// <summary>
        /// Update the graph using the given data
        /// </summary>
        public void Update(GraphDataPacket data)
        {
            InternalGraph.Update(data);
            InvalidateVisual();
        }

        /// <summary>
        /// The fraction of space to leave on the inside margin of the graph
        /// </summary>
        public double InnerMargin { get; set; } = .15;
     
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // Adjust for the size of the new window
            InternalGraph.UpdateTransform(ActualWidth * (1 - 2 * InnerMargin), ActualHeight * (1 - 2 * InnerMargin), 
                ActualWidth * InnerMargin, ActualHeight * InnerMargin);
            drawingContext.DrawDrawing(InternalGraph.Drawing);
        }
    }
}
