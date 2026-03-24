namespace GraphData
{
    /// <summary>
    /// A collection of graph data, used for updating graphs, histograms, or other updating objects
    /// One GraphDataPacket represents all graph data for many graphs or other updating objects at a single point in time
    /// </summary>
    public class GraphDataPacket
    {
        public GraphDataPacket()
        { } // Default constructor to distinguish from the BinaryReader constructor

        /// <summary>
        /// Adds a single number to the data packet
        /// </summary>
        public void AddData(double newdatum)
        {
            data.Enqueue(newdatum);
        }

        /// <summary>
        /// Adds a string to the packet (stored separately from the numerical data)
        /// </summary>
        public void AddTextData(string newString)
        {
            textData.Enqueue(newString);
        }

        /// <summary>
        /// Adds a set of numbers, as for a histogram, to the data packet
        /// </summary>
        public void AddSet(IEnumerable<double> list)
        {
            AddData(list.Count());
            foreach (var element in list)
            {
                AddData(element);
            }
        }

        /// <summary>
        /// Combines two data packets into one
        /// </summary>
        static public GraphDataPacket Combine(GraphDataPacket packet1, GraphDataPacket packet2)
        {
            var response = new GraphDataPacket();
            foreach (var item in packet1.data)
            {
                response.data.Enqueue(item);
            }
            foreach (var item in packet2.data)
            {
                response.data.Enqueue(item);
            }
            foreach (var item in packet1.textData)
            {
                response.textData.Enqueue(item);
            }
            foreach (var item in packet2.textData)
            {
                response.textData.Enqueue(item);
            }

            return response;
        }

        /// <summary>
        /// Retrieves a set of data (for a histogram)
        /// </summary>
        public IEnumerable<double> GetSet()
        {
            int size = (int)GetData();
            for (int i = 0; i < size; ++i)
            {
                yield return GetData();
            }
        }

        /// <summary>
        /// Peeks at the size of the set of data (for a histogram)
        /// </summary>
        public int GetSetSize()
        {
            return (int)(data.Peek());
        }

        /// <summary>
        /// Removes the first number from the dataset - used for discarding unused data
        /// </summary>
        public void RemoveFromFront()
        {
            data.Dequeue();
        }

        /// <summary>
        /// Discards a specificed number of numbers from the dataset
        /// </summary>
        /// <param name="number"></param>
        public void RemoveFromFront(int number)
        {
            for (int i = 0; i < number; ++i)
            {
                RemoveFromFront();
            }
        }

        /// <summary>
        /// Removes the next number from the dataset and returns it
        /// </summary>
        public double GetData()
        {
            return data.Dequeue();
        }

        /// <summary>
        /// Removes the next string from the dataset and returns it
        /// </summary>
        public string GetTextData()
        {
            return textData.Dequeue();
        }

        /// <summary>
        /// Returns a specified number of numbers from the dataset
        /// </summary>
        public IEnumerable<double> GetData(int number)
        {
            while (number-- > 0)
            {
                yield return GetData();
            }
        }

        /// <summary>
        /// Writes the data packet to a file.
        /// </summary>
        public void WriteData(BinaryWriter bw)
        {
            bw.Write(data.Count);
            foreach (double datum in data)
            {
                bw.Write(datum);
            }
            bw.Write(textData.Count);
            foreach (string textDatum in textData)
            {
                bw.Write(textDatum);
            }
        }

        /// <summary>
        /// Reads a data packet from a file
        /// </summary>
        public GraphDataPacket(BinaryReader br)
        {
            int dataSize = br.ReadInt32();
            for (int i = 0; i < dataSize; ++i)
            {
                data.Enqueue(br.ReadDouble());
            }
            int textSize = br.ReadInt32();
            for (int i = 0; i < textSize; ++i)
            {
                textData.Enqueue(br.ReadString());
            }
        }

        private readonly Queue<double> data = new();
        private readonly Queue<string> textData = new();
    }
}
