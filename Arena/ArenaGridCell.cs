namespace Arena
{
    /// <summary>
    /// A cell in the arena grid. This is used to speed up collision detection.
    /// </summary>
    internal class ArenaGridCell
    {
        public LinkedList<ArenaObject> Objects { get; } = new LinkedList<ArenaObject>();

        /// <summary>
        /// Add an object to the cell.
        /// </summary>
        public void AddObject(ArenaObject obj)
        {
            Objects.AddLast(obj);
        }

        /// <summary>
        /// Remove an object from the cell.
        /// </summary>
        public void RemoveObject(ArenaObject obj)
        {
            Objects.Remove(obj);
        }
    }
}
