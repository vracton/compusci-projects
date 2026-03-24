using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// Interface that all graphs need to implement
    /// </summary>
    public interface IGraphInterface : IUpdating
    {
        /// <summary>
        /// Changes the graph's size and location based on these parameters
        /// </summary>
        void UpdateTransform(double width, double height, double widthOffset, double heightOffset);

        /// <summary>
        /// The Drawing that contains the graph
        /// </summary>
        Drawing Drawing { get; }
    }
}
