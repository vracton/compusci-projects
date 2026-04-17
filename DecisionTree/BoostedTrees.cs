using System;
using System.Collections.Generic;
using System.Text;

namespace DecisionTree
{
    public class BoostedTrees
    {
        public List<Tree> Trees { get; private set; } = new List<Tree>();
        public List<double> Weights { get; private set; } = new List<double>();

        public int MaxExtra { get; set; } = 5;
        public int MaxDepth { get; set; } = 5;
        public void Train(DataSet signal, DataSet background, int numSplits)
        {
            (List<Tree> trees, List<double> weights, List<double> pointWeights)[] allRuns = new (List<Tree>, List<double>, List<double>)[numSplits];
            for (int i = 0; i < numSplits; i++)
            {
                allRuns[i].trees = new List<Tree>();
                allRuns[i].weights = new List<double>();
                //allRuns[i].weights.Add(1.0);
                allRuns[i].pointWeights = new List<double>();

                int numPoints = (int)((1.0 - 1.0/numSplits) * (signal.Points.Count + background.Points.Count));
                for (int j = 0; j < numPoints; j++)
                {
                    allRuns[i].pointWeights.Add(1.0 / numPoints);
                }
            }

            double bestAcc = 0.0;
            int bestNumTrees = 0;
            bool finishing = false;
            int extraDone = 0;
            int iters = 0;
            while (extraDone < MaxExtra)
            {
                iters++;
                double avgAcc = 0.0;

                extraDone += finishing ? 1 : 0;

                for (int i = 0; i < numSplits; i++)
                {
                    DataSet sTrain = signal.RangeFrom(0, i * signal.Points.Count / numSplits);
                    sTrain.AddDataPoints(signal.RangeFrom((i + 1) * signal.Points.Count / numSplits, signal.Points.Count).Points);
                    DataSet bTrain = background.RangeFrom(0, i * background.Points.Count / numSplits);
                    bTrain.AddDataPoints(background.RangeFrom((i + 1) * background.Points.Count / numSplits, background.Points.Count).Points);
                    DataSet sValid = signal.RangeFrom(i * signal.Points.Count / numSplits, (i + 1) * signal.Points.Count / numSplits);
                    DataSet bValid = background.RangeFrom(i * background.Points.Count / numSplits, (i + 1) * background.Points.Count / numSplits);

                    Tree t = new Tree();
                    t.Train(sTrain, bTrain, allRuns[i].pointWeights, MaxDepth);

                    allRuns[i].trees.Add(t);
                    (List<double> newWeights, double treeWeight) = t.GetWeighted(new CombinedData(sTrain, bTrain), allRuns[i].pointWeights);
                    allRuns[i].weights.Add(treeWeight);
                    allRuns[i].pointWeights = newWeights;

                    avgAcc += GetAccuracy(new CombinedData(sValid, bValid), allRuns[i].trees, allRuns[i].weights);
                }

                avgAcc /= numSplits;
                
                if (avgAcc > bestAcc)
                {
                    bestAcc = avgAcc;
                    bestNumTrees = allRuns[0].trees.Count;
                } 
                else if (!finishing)
                {
                    finishing = true;
                    Console.WriteLine("Finishing...");
                }

                Console.WriteLine($"{iters} done, accuracy = {avgAcc}");
            }


            List<double> pointWeights = new();
            int tNumPoints = signal.Points.Count + background.Points.Count;
            for (int i = 0; i < tNumPoints; i++)
            {
                pointWeights.Add(1.0 / tNumPoints);
            }

            //Weights.Add(1.0);

            for (int i = 0; i < bestNumTrees; i++)
            {
                Tree t = new Tree();
                t.Train(signal, background, pointWeights, MaxDepth);

                (List<double> newWeights, double treeWeight) = t.GetWeighted(new CombinedData(signal, background), pointWeights);

                Trees.Add(t);
                Weights.Add(treeWeight);
                pointWeights = newWeights;
            }
        }

        public double GetAccuracy(DataSet signal, DataSet background)
        {
            CombinedData combined = new(signal, background);
            return GetAccuracy(combined, Trees, Weights);
        }

        private double GetAccuracy(CombinedData combined, List<Tree> trees, List<double> weights)
        {
            //double maxProb = 0.0;
            //for (int i=0; i < trees.Count; i++)
            //{
            //    maxProb += Math.Log(weights[i]);
            //}

            int correct = 0;
            foreach (var (p, isSignal) in combined.Points)
            {
                double prob = 0.0;
                for (int i = 0; i < trees.Count; i++)
                {
                    prob += Math.Log(weights[i]) * (trees[i].RunDataPoint(p) > 0.5 ? 1 : 0);
                }

                if ((prob > 0.5 * weights.Sum(w => Math.Log(w))) == isSignal)
                {
                    correct++;
                }
            }

            return (double)correct / combined.Count;
        }
    }
}
