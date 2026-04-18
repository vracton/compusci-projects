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

        public int NumChildren
        {
            get
            {
                return 1 + headnode.NumChildren;
            }
        }
        
        public int NumLeaves
        {
            get
            {
                return headnode.LeafCount;
            }
        }

        public int BranchCount
        {
            get
            {
                return headnode.BranchCount;
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

        private Tree(Leaf headnode)
        {
            this.headnode = headnode;
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
        public void Train(DataSet signal, DataSet background, int maxDepth = int.MaxValue, bool usePruning = false)
        {
            int numPoints = (signal.Points.Count + background.Points.Count);
            List<double> pointWeights = new List<double>(numPoints);
            for (int j = 0; j < numPoints; j++)
            {
                pointWeights.Add(1.0 / numPoints);
            }
            headnode.Train(signal, background, pointWeights, maxDepth, usePruning);
        }

        public void Train(DataSet signal, DataSet background, List<double> weights, int maxDepth = int.MaxValue, bool usePruning = false)
        {
            headnode.Train(signal, background, weights, maxDepth, usePruning);
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

        public Tree Clone()
        {
            return new Tree(headnode.Clone());
        }

        public void Prune(double alpha)
        {
            headnode.PruneAtAlpha(alpha);
        }

        public double PruneForValidation(CombinedData validation)
        {
            var candidateAlphas = new List<double>();
            headnode.CollectEffectiveAlphas(candidateAlphas);
            candidateAlphas = candidateAlphas
                .Where(alpha => alpha > 0.0)
                .Distinct()
                .OrderBy(alpha => alpha)
                .ToList();

            double bestAlpha = 0.0;
            double bestAccuracy = GetAccuracy(validation);

            foreach (double alpha in candidateAlphas)
            {
                var candidate = Clone();
                candidate.Prune(alpha);

                double accuracy = candidate.GetAccuracy(validation);
                if (accuracy > bestAccuracy)
                {
                    bestAccuracy = accuracy;
                    bestAlpha = alpha;
                }
            }

            Prune(bestAlpha);
            return bestAlpha;
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
