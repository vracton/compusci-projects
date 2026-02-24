namespace GraphData
{
    /// <summary>
    /// A set of functions that is called to get data to update a graph, histogram, etc. real-time
    /// </summary>
    public class UpdatingFunctions
    {
        /// <summary>
        /// A function that puts data into a GraphDataPacket
        /// </summary>
        public delegate void GetFunction(GraphDataPacket packet);

        private readonly List<GetFunction> functions = [];

        /// <summary>
        /// Adds a new function to the list of functions
        /// </summary>
        public void AddFunction(GetFunction function)
        {
            functions.Add(function);
        }

        /// <summary>
        /// Adds all the functions in another UpdatingFunctions object
        /// </summary>
        public void AddFunctions(UpdatingFunctions other)
        {
            functions.AddRange(other.functions);
        }

        /// <summary>
        /// Gets all the data from all the functions at a given point in time and return it for use in graphs
        /// </summary>
        /// <returns></returns>
        public GraphDataPacket GetData()
        {
            var data = new GraphDataPacket();
            foreach (var func in functions)
            {
                func(data);
            }
            return data;
        }
    }
}
