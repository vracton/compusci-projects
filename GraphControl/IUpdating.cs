using GraphData;

namespace GraphControl
{
    /// <summary>
    /// Interface for objects that can be updated with new data
    /// </summary>
    public interface IUpdating
    {
        /// <summary>
        /// Updates the object with new data
        /// </summary>
        void Update(GraphDataPacket data);
    }
}
