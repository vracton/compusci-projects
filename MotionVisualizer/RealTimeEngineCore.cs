using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    /// <summary>
    /// A visualization engine that runs in real time
    /// </summary>
    /// <typeparam name="TVisualizer">The type of visualizer</typeparam>
    /// <typeparam name="TCommand">The type of commands to send to the visualizer</typeparam>
    internal class RealTimeEngineCore<TVisualizer, TCommand>(IEngine<TVisualizer, TCommand> engine, GraphDataManager manager) : EngineCore<TVisualizer, TCommand>
        where TCommand : ICommand<TVisualizer>
    {
        private RealTimeGraphDataInterface? graphInterface;

        public override bool Continue => engine.Continue;

        public double Time => engine.Time;

        public override IGraphDataInterface Initialize(TVisualizer visualizer)
        {
            engine.Initialization().ProcessAll(visualizer);
            graphInterface = new RealTimeGraphDataInterface(manager);
            return graphInterface;
        }

        public override PackagedCommands<TVisualizer> NextCommand(double newTime)
        {
            var commands = engine.Tick(newTime);
            if (graphInterface == null)
            {
                throw new InvalidOperationException("Engine not initialized");
            }
            var data = graphInterface.GetData();
            return new PackagedCommands<TVisualizer>(commands, data, newTime);
        }
    }
}
