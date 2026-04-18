using System;
using System.Collections.Generic;
using System.Text;

namespace DecisionTree
{
    public class BoostedTrees
    {
        public List<Tree> Trees { get; private set; } = new List<Tree>();
        public List<double> Weights { get; private set; } = new List<double>();
        public bool UsePruning { get; set; } = false;

        public int MaxExtra { get; set; } = 5;
        public int MaxDepth { get; set; } = 5;
        public int MaxTrees { get; set; } = int.MaxValue;
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
            List<double> pruneAlphas = new();
            List<double> bestPruneAlphas = new();
            List<double> validationHistory = new();
            bool finishing = false;
            int extraDone = 0;
            int iters = 0;
            while (extraDone < MaxExtra && iters < MaxTrees)
            {
                iters++;
                double avgAcc = 0.0;
                double minAcc = double.MaxValue;
                double maxAcc = double.MinValue;
                double alphaSum = 0.0;

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
                    if (UsePruning)
                    {
                        alphaSum += t.PruneForValidation(new CombinedData(sValid, bValid));
                    }

                    allRuns[i].trees.Add(t);
                    (List<double> newWeights, double treeWeight) = t.GetWeighted(new CombinedData(sTrain, bTrain), allRuns[i].pointWeights);
                    allRuns[i].weights.Add(treeWeight);
                    allRuns[i].pointWeights = newWeights;

                    double foldAcc = GetAccuracy(new CombinedData(sValid, bValid), allRuns[i].trees, allRuns[i].weights);
                    avgAcc += foldAcc;
                    if (foldAcc < minAcc)
                    {
                        minAcc = foldAcc;
                    }
                    if (foldAcc > maxAcc)
                    {
                        maxAcc = foldAcc;
                    }
                }

                avgAcc /= numSplits;
                validationHistory.Add(avgAcc);
                if (UsePruning)
                {
                    pruneAlphas.Add(alphaSum / numSplits);
                }
                
                if (avgAcc > bestAcc)
                {
                    bestAcc = avgAcc;
                    bestNumTrees = allRuns[0].trees.Count;
                    bestPruneAlphas = [.. pruneAlphas];
                }

                if (!finishing && validationHistory.Count > MaxExtra)
                {
                    double priorAccuracy = validationHistory[validationHistory.Count - 1 - MaxExtra];
                    if (avgAcc < priorAccuracy + 0.02)
                    {
                        finishing = true;
                        Console.WriteLine("Finishing...");
                    }
                }

                Console.WriteLine($"{iters} done, min accuracy = {minAcc}, max accuracy = {maxAcc}");
            }


            List<double> pointWeights = new();
            int tNumPoints = signal.Points.Count + background.Points.Count;
            for (int i = 0; i < tNumPoints; i++)
            {
                pointWeights.Add(1.0 / tNumPoints);
            }

            //Weights.Add(1.0);

            double avgLeavesBefore = 0.0;
            double avgLeavesAfter = 0.0;
            double avgDepthBefore = 0.0;
            double avgDepthAfter = 0.0;
            int totalLeavesBefore = 0;
            int totalLeavesAfter = 0;
            int totalBranchesBefore = 0;
            int totalBranchesAfter = 0;

            for (int i = 0; i < bestNumTrees; i++)
            {
                Tree t = new Tree();
                t.Train(signal, background, pointWeights, MaxDepth);
                if (UsePruning)
                {
                    avgLeavesBefore += t.NumLeaves;
                    avgDepthBefore += t.Depth;
                    totalLeavesBefore += t.NumLeaves;
                    totalBranchesBefore += t.BranchCount;
                }
                if (UsePruning && i < bestPruneAlphas.Count)
                {
                    t.Prune(bestPruneAlphas[i]);
                    avgLeavesAfter += t.NumLeaves;
                    avgDepthAfter += t.Depth;
                    totalLeavesAfter += t.NumLeaves;
                    totalBranchesAfter += t.BranchCount;
                }

                (List<double> newWeights, double treeWeight) = t.GetWeighted(new CombinedData(signal, background), pointWeights);

                Trees.Add(t);
                Weights.Add(treeWeight);
                pointWeights = newWeights;
            }

            if (UsePruning && bestNumTrees > 0)
            {
                Console.WriteLine($"Final model avg leaves before/after = {avgLeavesBefore / bestNumTrees:F1}/{avgLeavesAfter / bestNumTrees:F1}, avg depth before/after = {avgDepthBefore / bestNumTrees:F1}/{avgDepthAfter / bestNumTrees:F1}");
                Console.WriteLine($"Final model total leaves before/after = {totalLeavesBefore}/{totalLeavesAfter}, total branches before/after = {totalBranchesBefore}/{totalBranchesAfter}");
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

        public void MakeTextFile(string filename, DataSet data)
        {
            using var file = File.CreateText(filename);
            file.WriteLine("Event\tScore");

            int a = 0;
            for (int i = 0; i < data.Points.Count; ++i)
            {
                double output = 0.0;
                for (int j = 0; j < Trees.Count; j++)
                {
                    output += Math.Log(Weights[j]) * (Trees[j].RunDataPoint(data.Points[i]) > 0.5 ? 1 : 0);
                }
                //if ((output > 0.5 * Weights.Sum(w => Math.Log(w))))
                //{
                //    a++;
                //}
                file.WriteLine(i + "\t" + output);
            }
        }
    }
}
