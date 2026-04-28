using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    /// <summary>
    /// An engine that has some parts calculated real-time and others that are read from file
    /// </summary>
    /// <typeparam name="TVisualizer">The type of visualizer to use</typeparam>
    /// <typeparam name="TCommand">The type of commands to be sent to the visualizer</typeparam>
    class HybridEngineCore<TVisualizer, TCommand>(string filename, ICommandFileReader<TVisualizer> factory, 
        IEngine<TVisualizer, TCommand> engine, GraphDataManager manager) : EngineCore<TVisualizer, TCommand>
            where TCommand : ICommand<TVisualizer>
    {
        private readonly FromFileEngineCore<TVisualizer, TCommand> fileCore = new(filename, factory);
        private readonly RealTimeEngineCore<TVisualizer, TCommand> realTimeCore = new(engine, manager);

        public override bool Continue => shouldContinue;
        private bool shouldContinue = true;

        public override IGraphDataInterface Initialize(TVisualizer visualizer)
        {
            var fileResponse = fileCore.Initialize(visualizer);
            var realTimeResponse = realTimeCore.Initialize(visualizer);
            return new HybridGraphDataInterface(fileResponse, realTimeResponse);
        }

        public override PackagedCommands<TVisualizer>? NextCommand(double newTime)
        {
            // Note that we ignore newTime, since the time comes from the file

            var fileCommandSet = fileCore.NextCommand(newTime);
            if (fileCommandSet == null)
            {
                shouldContinue = false;
                return null;
            }
            double currentTime = fileCommandSet.Time;
            var realTimeCommandSet = realTimeCore.NextCommand(currentTime);
            return PackagedCommands<TVisualizer>.Combine(fileCommandSet, realTimeCommandSet);
        }
    }
}
