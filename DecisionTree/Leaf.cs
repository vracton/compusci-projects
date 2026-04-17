namespace DecisionTree
{
    /// <summary>
    /// A leaf or branch of a tree
    /// Does most of the work of decision trees
    /// </summary>
    internal class Leaf
    {
        /// <summary>
        /// A pointer to the next leaves, if this is a branch
        /// </summary>
        private Leaf? output1 = null;
        /// <summary>
        /// A pointer to the next leaves, if this is a branch
        /// </summary>
        private Leaf? output2 = null;

        public int NumChildren
        {
            get
            {
                int n = 0;
                if (output1 != null)
                {
                    n += 1 + output1.NumChildren;
                }
                if (output2 != null)
                {
                    n += 1 + output2.NumChildren;
                }
                return n;
            }
        }

        public int RemainingDepth
        {
            get
            {
                int d1 = output1?.RemainingDepth ?? 0;
                int d2 = output2?.RemainingDepth ?? 0;
                return 1 + Math.Max(d1, d2);
            }
        }

        /// <summary>
        /// The value of the cut that is applied at this branch (unneeded if it is a leaf)
        /// </summary>
        private double split;
        /// <summary>
        /// The index of the variable which is used to make the cut (unneeded if this is a leaf)
        /// </summary>
        private int variable;

        /// <summary>
        /// The number of background training events in this leaf
        /// </summary>
        private int nBackground = 0;
        /// <summary>
        /// The number of signal training events in this leaf
        /// </summary>
        private int nSignal = 0;

        /// <summary>
        /// A default constructor is needed for some applications, but it is generally not sensible
        /// </summary>
        internal Leaf() :
            this(-1, 0) // Default values will generate an error if used
        { }

        /// <param name="variable">The index of the variable used to make the cut</param>
        /// <param name="split">The value of the cut used for the branch</param>
        public Leaf(int variable, double split)
        {
            this.variable = variable;
            this.split = split;
        }

        /// <summary>
        /// Write the leaf to a binary file
        /// </summary>
        internal void Write(BinaryWriter bw)
        {
            bw.Write(variable);
            bw.Write(split);
            bw.Write(nSignal);
            bw.Write(nBackground);

            bw.Write(IsFinal);
            if (!IsFinal)
            {
                output1?.Write(bw);
                output2?.Write(bw);
            }
        }

        /// <summary>
        /// Construct a leaf from a binary file
        /// </summary>
        internal Leaf(BinaryReader br)
        {
            variable = br.ReadInt32();
            split = br.ReadDouble();
            nSignal = br.ReadInt32();
            nBackground = br.ReadInt32();

            bool fin = br.ReadBoolean();
            if (!fin)
            {
                output1 = new Leaf(br);
                output2 = new Leaf(br);
            }
        }

        /// <summary>
        /// Determines if it is a leaf or a branch (true for leaves)
        /// </summary>
        public bool IsFinal => output1 == null || output2 == null;

        /// <summary>
        /// The purity of the leaf
        /// </summary>
        public double Purity => (double)nSignal / (nSignal + nBackground);

        /// <summary>
        /// Calculates the return value for a single data point, forwarding it to other leaves as needed
        /// </summary>
        public double RunDataPoint(DataPoint dataPoint)
        {
            if (IsFinal)
            {
                return Purity;
            }

            if (DoSplit(dataPoint))
            {
                return output1?.RunDataPoint(dataPoint) ?? 0;
            }
            else
            {
                return output2?.RunDataPoint(dataPoint) ?? 0;
            }
        }

        /// <summary>
        /// Checks to see whether the DataPoint fails or passes the cut
        /// </summary>
        private bool DoSplit(DataPoint dataPoint)
        {
            return dataPoint.Variables[variable] <= split;
        }

        /// <summary>
        /// Trains this leaf based on input DataSets for signal and background
        /// </summary>
        public void Train(DataSet signal, DataSet background, List<double> weights, int remainingDepth = int.MaxValue)
        {
            nSignal = signal.Points.Count;
            nBackground = background.Points.Count;

            if (remainingDepth <= 0)
            {
                return;
            }

            // Determines whether this is a final leaf or if it branches
            bool branch = ChooseVariable(signal, background, weights);

            if (branch)
            {
                // Creates a branch
                output1 = new Leaf();
                output2 = new Leaf();

                DataSet signalLeft = new(signal.Names);
                DataSet signalRight = new(signal.Names);
                DataSet backgroundLeft = new(background.Names);
                DataSet backgroundRight = new(background.Names);
                List<double> weightsLeft = new();
                List<double> weightsRight = new();

                for (int i =0; i<nSignal; i++)
                {
                    if (DoSplit(signal.Points[i]))
                    {
                        signalLeft.AddDataPoint(signal.Points[i]);
                        weightsLeft.Add(weights[i]);
                    }
                    else
                    {
                        signalRight.AddDataPoint(signal.Points[i]);
                        weightsRight.Add(weights[i]);
                    }
                }

                for (int i = 0; i<nBackground; i++)
                {
                    if (DoSplit(background.Points[i]))
                    {
                        backgroundLeft.AddDataPoint(background.Points[i]);
                        weightsLeft.Add(weights[i + nSignal]);
                    }
                    else
                    {
                        backgroundRight.AddDataPoint(background.Points[i]);
                        weightsRight.Add(weights[i + nSignal]);
                    }
                }

                // Trains each of the resulting leaves
                output1.Train(signalLeft, backgroundLeft, weightsLeft, remainingDepth - 1);
                output2.Train(signalRight, backgroundRight, weightsRight, remainingDepth - 1);
            }
            // Do nothing more if it is not a branch
        }

        /// <summary>
        /// Chooses which variable and cut value to use
        /// </summary>
        /// <returns>True if a branch was created, false if this is a final leaf</returns>
        /// 
        //private double 

        private bool ChooseVariable(DataSet signal, DataSet background, List<double> weights)
        {
            const int minPoints = 50;
            if (signal.Points.Count <= minPoints || background.Points.Count <= minPoints) //arbitrary
            {
                return false;
            }

            CombinedData combined = new(signal, background);

            double highestGain = 0.0;

            for (int varInd=0; varInd<signal.Names.Length; varInd++)
            {
                var sorted = combined.SortedByWithWeights(varInd, weights);

                double totalWeight = weights.Sum(); //should be 1.0
                double signalWeight = weights.Take(signal.Points.Count).Sum();
                double bgWeight = totalWeight - signalWeight;

                double parPurity = 1.0 - (Math.Pow(signalWeight / totalWeight, 2) + Math.Pow(bgWeight / totalWeight, 2));

                double sR = signalWeight;
                double bR = bgWeight;
                double sL = 0.0;
                double bL = 0.0;

                for (int i=0; i<sorted.Count-1; i++)
                {
                    if (sorted[i].isSignal)
                    {
                        sR-=sorted[i].weight;
                        sL+=sorted[i].weight;

                    }
                    else
                    {
                        bR-=sorted[i].weight;
                        bL+=sorted[i].weight;
                    }

                    if (sorted[i].val == sorted[i+1].val)
                    {
                        continue;
                    }

                    double leftPurity = 1.0 - (Math.Pow((double)sL / (double)(sL + bL), 2) + Math.Pow((double)bL / (double)(sL + bL), 2));
                    double rightPurity = 1.0 - (Math.Pow((double)sR / (double)(sR + bR), 2) + Math.Pow((double)bR / (double)(sR + bR), 2));
                    double childPurity = (leftPurity * (sL + bL) + rightPurity * (sR + bR)) / (signalWeight + bgWeight);
                    double gain = parPurity - childPurity;

                    if (gain > highestGain)
                    {
                        highestGain = gain;
                        variable = varInd;
                        split = sorted[i].val;
                    }
                }
            }

            if (highestGain <= 1e-6)
            {
                return false;
            }

            return true;
        }

    }
}
