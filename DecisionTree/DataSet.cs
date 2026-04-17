namespace DecisionTree
{
    /// <summary>
    /// A collection of DataPoints used for training and evaluating machine learning algorithms
    /// </summary>
    /// <param name="names">An array of the names of each variable</param>
    public class DataSet(string[] names)
    {
        /// <summary>
        /// All the points in the set
        /// </summary>
        public List<DataPoint> Points { get; } = [];

        /// <summary>
        /// The name of each variable used, in case you are interested
        /// </summary>
        public string[] Names { get; } = names;

        /// <summary>
        /// Add a DataPoint to the Points list
        /// </summary>
        public void AddDataPoint(DataPoint point)
        {
            Points.Add(point);
        }

        public void AddDataPoints(IEnumerable<DataPoint> points)
        {
            Points.AddRange(points);
        }

        public DataSet RangeFrom(int start, int end)
        {
            var ds = new DataSet(Names);
            ds.AddDataPoints(Points.GetRange(start, end - start));
            return ds;
        }

        private const string header = "DecisionTreeDataSet";

        /// <summary>
        /// Write the entire DataSet to a binary file
        /// </summary>
        public void WriteToFile(string filename)
        {
            using var file = new BinaryWriter(File.Create(filename));
            file.Write(header);
            file.Write(this);
        }

        /// <summary>
        /// Read a DataSet from a saved file
        /// </summary>
        static public DataSet ReadDataSet(string filename)
        {
            using var file = new BinaryReader(File.OpenRead(filename));
            string fileHeader = file.ReadString();
            if (fileHeader != header)
                throw new FormatException("Header does not match DataSet header!");

            return file.ReadDataSet();
        }
    }
}
