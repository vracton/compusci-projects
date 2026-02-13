using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    /// <summary>
    /// A visualization engine that reads its data from a file
    /// </summary>
    /// <typeparam name="TVisualizer">The type of visualizer that is used</typeparam>
    /// <typeparam name="TCommand">The type of command to send to the visualizer</typeparam>
    /// <param name="factory">An object that can read the commands being sent</param>
    internal class FromFileEngineCore<TVisualizer, TCommand>(string filename, ICommandFileReader<TVisualizer> factory) : EngineCore<TVisualizer, TCommand>
        where TCommand : ICommand<TVisualizer>
    {
        private readonly BinaryReader br = new(File.OpenRead(filename));
        private FileGraphDataInterface? graphInterface;
        private bool shouldContinue = true;
        public override bool Continue => shouldContinue;

        public override IGraphDataInterface Initialize(TVisualizer visualizer)
        {
            var initialSet = new CommandSet<TVisualizer>(br, factory);
            initialSet.ProcessAll(visualizer);

            graphInterface = new FileGraphDataInterface(br);
            return graphInterface;
        }

        public override PackagedCommands<TVisualizer>? NextCommand(double newTime)
        {
            if (DongUtility.FileUtilities.IsEndOfFile(br))
            {
                return null;
            }
            var commands = new CommandSet<TVisualizer>(br, factory);
            var graphData = graphInterface?.GetData();
            double time = br.ReadDouble();
            shouldContinue = br.ReadBoolean();
            return new PackagedCommands<TVisualizer>(commands, graphData, time);
        }
    }
}
