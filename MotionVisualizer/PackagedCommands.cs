using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    /// <summary>
    /// Includes all the information needed by the visualizer piece that the engine will pass it
    /// </summary>
    internal class PackagedCommands<TVisualizer>(CommandSet<TVisualizer> commands, GraphDataPacket data, double time)
    {

        /// <summary>
        /// The command set
        /// </summary>
        public CommandSet<TVisualizer> Commands { get; set; } = commands;
        /// <summary>
        /// Data, for graphing
        /// </summary>
        public GraphDataPacket Data { get; set; } = data;
        /// <summary>
        /// The time of the commands
        /// </summary>
        public double Time { get; set; } = time;

        /// <summary>
        /// Combines the commands of two packages together
        /// </summary>
        static public PackagedCommands<TVisualizer> Combine(PackagedCommands<TVisualizer> package1, PackagedCommands<TVisualizer> package2)
        {
            if (package1.Time != package2.Time)
            {
                throw new ArgumentException("Arguments with different times used in PackagedCommands.Combine()");
            }
            var combinedCommands = package1.Commands + package2.Commands;
            var combinedData = GraphDataPacket.Combine(package1.Data, package2.Data);
            return new PackagedCommands<TVisualizer>(combinedCommands, combinedData, package1.Time);
        }
    }
}
