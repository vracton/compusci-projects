namespace DecisionTree
{
    /// <summary>
    /// A class to represent a decision tree
    /// The Leaf class does all the work
    /// </summary>
    public class Tree
    {
        /// <summary>
        /// The top of the tree, which is all that is needed - all other leaves are accessed from here
        /// </summary>
        private readonly Leaf headnode;

        public int NumLeaves
        {
            get
            {
                return 1 + headnode.NumChildren;
            }
        }

        public int Depth
        {
            get
            {
                return 1 + headnode.RemainingDepth;
            }
        }

        public Tree()
        {
            headnode = new Leaf();
        }

        /// <summary>
        /// Writes the tree to a binary file
        /// </summary>
        public void WriteToFile(string filename)
        {
            using var bw = new BinaryWriter(File.Create(filename));
            headnode.Write(bw);
        }

        /// <summary>
        /// Constructs the tree from a binary file
        /// </summary>
        public Tree(string filename)
        {
            using var br = new BinaryReader(File.OpenRead(filename));
            headnode = new Leaf(br);
        }

        /// <summary>
        /// Trains the tree on signal and background samples
        /// </summary>
        public void Train(DataSet signal, DataSet background)
        {
            int numPoints = (signal.Points.Count + background.Points.Count);
            List<double> pointWeights = new List<double>(numPoints);
            for (int j = 0; j < numPoints; j++)
            {
                pointWeights.Add(1.0 / numPoints);
            }
            headnode.Train(signal, background, pointWeights);
        }

        public void Train(DataSet signal, DataSet background, List<double> weights)
        {
            headnode.Train(signal, background, weights);
        }

        public double GetAccuracy(CombinedData data)
        {
            int correct = 0;
            foreach (var (p, isSignal) in data.Points)
            {
                double prob = RunDataPoint(p);
                if ((prob > 0.5) == isSignal)
                {
                    correct++;
                }
            }
            return (double)correct / data.Count;
        }

        public (List<double>, double) GetWeighted(CombinedData data, List<double> weights)
        {
            double wrong = 0;
            List<int> wrongInd = new();
            for (int i = 0; i < data.Points.Count; i++)
            {
                var (p, isSignal) = data.Points[i];
                double prob = RunDataPoint(p);

                if ((prob > 0.5) != isSignal)
                {
                    wrong += weights[i];
                    wrongInd.Add(i);
                }
            }
            
            double treeWeight = (1 - wrong) / wrong;
            List<double> newWeights = new();
            Console.WriteLine(treeWeight);
            double sum = 0.0;
            for (int i = 0; i < data.Points.Count; i++)
            {
                if (wrongInd.Contains(i))
                {
                    newWeights.Add(weights[i] * treeWeight);
                    sum += weights[i] * treeWeight;
                }
                else
                {
                    newWeights.Add(weights[i]);
                    sum += weights[i];
                }
            }

            for (int i = 0; i < newWeights.Count; i++)
            {
                newWeights[i] /= sum;
            }

            return (newWeights, treeWeight);
        }

        /// <summary>
        /// Calculates the value for a single DataPoint
        /// Assumes the tree is trained already
        /// </summary>
        public double RunDataPoint(DataPoint dp)
        {
            return headnode.RunDataPoint(dp);
        }

        public void MakeTextFile(string filename, DataSet data)
        {
            using var file = File.CreateText(filename);
            file.WriteLine("Event\tPurity");

            for (int i = 0; i < data.Points.Count; ++i)
            {
                double output = RunDataPoint(data.Points[i]);
                file.WriteLine(i + "\t" + output);
            }
        }
    }
}
