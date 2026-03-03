using System.IO;
using VisualizerBaseClasses;

namespace VisualizerControl
{
    /// <summary>
    /// A class to read Visualizer commands from a binary file
    /// </summary>
    public class VisualizerCommandFileReader : ICommandFileReader<Visualizer>
    {
        public ICommand<Visualizer> ReadCommand(BinaryReader br)
        {
            return VisualizerCommand.ReadFromFile(br);
        }
    }
}
