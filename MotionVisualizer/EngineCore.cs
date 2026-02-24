using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    /// <summary>
    /// The base class of a visualization engine
    /// </summary>
    /// <typeparam name="TVisualizer">The type of visualization used</typeparam>
    /// <typeparam name="TCommand">The type of the commands that will be sent to the visualizer</typeparam>
    internal abstract class EngineCore<TVisualizer, TCommand>
        where TCommand : ICommand<TVisualizer>
    {
        /// <summary>
        /// A set of all the graphs, histograms, etc. that are needed to initialize the visualization
        /// </summary>
        /// <param name="visualizer"></param>
        /// <returns></returns>
        public abstract IGraphDataInterface Initialize(TVisualizer visualizer);

        /// <summary>
        /// The next set of commands that are issued at a specific point in time
        /// </summary>
        public abstract PackagedCommands<TVisualizer>? NextCommand(double newTime);

        /// <summary>
        /// Returns true if the visualization should continue and false to end it
        /// </summary>
        public abstract bool Continue { get; }
    }
}
