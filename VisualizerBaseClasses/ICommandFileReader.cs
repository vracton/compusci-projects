namespace VisualizerBaseClasses
{
    /// <summary>
    /// An interface for a class that reads commands from a file
    /// </summary>
    /// <typeparam name="TVisualizer"></typeparam>
    public interface ICommandFileReader<TVisualizer>
    {
        /// <summary>
        /// Read the next command from the file
        /// </summary>
        public ICommand<TVisualizer> ReadCommand(BinaryReader br);
    }
}
