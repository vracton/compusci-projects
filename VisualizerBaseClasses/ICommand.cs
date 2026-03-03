namespace VisualizerBaseClasses
{
    /// <summary>
    /// An interface for any command that can be sent to a visualizer
    /// </summary>
    /// <typeparam name="TVisualizer"></typeparam>
    public interface ICommand<TVisualizer>
    {
        /// <summary>
        /// This executes the command
        /// </summary>
        /// <param name="viz">The visualizer that receives the command</param>
        public void Do(TVisualizer viz);

        /// <summary>
        /// This writes the command to a binary file
        /// </summary>
        /// <param name="bw"></param>
        public void WriteToFile(BinaryWriter bw);
    }
}
