using System.Drawing;

namespace DecisionTree
{
    internal static class Program
    {
        private static readonly string path = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName + '\\';
        static void Main()
        {
            double startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            //LevelI();
            //LevelII();
            LevelIII();
            //LevelIV();

            Console.WriteLine($"Finished in {(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime) / 1000.0} seconds");
        }

        static void LevelI()
        {
            // Load training samples
            var signal = DataSet.ReadDataSet(path + "signal.dat");
            var background = DataSet.ReadDataSet(path + "background.dat");

            // Load data sample
            var data = DataSet.ReadDataSet(path + "decisionTreeData.dat");

            int bestVariableIndex = -1;
            double bestSplitValue = 0;
            double bestAccuracy = 0;

            // TODO: Insert code here that calculates the proper values of bestVariableIndex and bestSplitValue
            CombinedData combined = new(signal, background);

            for (int i = 0; i < signal.Names.Length; i++)
            {
                var sorted = combined.SortedBy(i);
                var sR = signal.Points.Count;
                var bL = 0;

                for (int n=0; n<sorted.Count-1; n++)
                {
                    if (sorted[n].isSignal)
                    {
                        sR--;
                    }
                    else
                    {
                        bL++;
                    }

                    double acc = (double)(sR + bL) / sorted.Count;
                    if (acc > bestAccuracy)
                    {
                        bestAccuracy = acc;
                        bestVariableIndex = i;
                        bestSplitValue = sorted[n].val;
                    }
                }

                Console.WriteLine($"{i + 1} done");
            }
            Console.WriteLine($"Accuracy: {bestAccuracy}, Best Variable Index {bestVariableIndex}, Best Split Value {bestSplitValue}");

            using var file = File.CreateText(path + "decisionTreeResultsLevelI.txt");
            file.WriteLine("Event\tPurity");

            for (int i = 0; i < data.Points.Count; ++i)
            {
                // Note that you may have to change the order of the 1 and 0 here, depending on which one matches signal.
                // 1 means signal and 0 means background
                double output = data.Points[i].Variables[bestVariableIndex] > bestSplitValue ? 1 : 0;
                file.WriteLine(i + "\t" + output);
            }
        }

        static void LevelII()
        {
            // Load training samples
            var signal = DataSet.ReadDataSet(path + "signal.dat");
            var background = DataSet.ReadDataSet(path + "background.dat");

            // Load data sample
            var data = DataSet.ReadDataSet(path + "decisionTreeData.dat");

            var tree = new Tree();

            // Train the tree
            tree.Train(signal, background);

            CombinedData combined = new(signal, background);

            int correct = 0;

            foreach (var (p, isSignal) in combined.Points)
            {
                if ((tree.RunDataPoint(p) > 0.5) == isSignal)
                {
                    correct++;
                }
            }

            Console.WriteLine($"Accuracy: {((double)correct / combined.Count):F4}, # Leaves: {tree.NumLeaves}, Depth: {tree.Depth}");

            // Calculate output value for each event and write to file
            tree.MakeTextFile(path + "decisionTreeResultsLevelII.txt", data);
        }

        static void LevelIII()
        {
            // Load training samples
            var signal = DataSet.ReadDataSet(path + "signal.dat");
            var background = DataSet.ReadDataSet(path + "background.dat");

            // Load data sample
            var data = DataSet.ReadDataSet(path + "decisionTreeData.dat");

            var boostedTree = new BoostedTrees();
            boostedTree.MaxDepth = 4;
            boostedTree.MaxExtra = 4;

            boostedTree.Train(signal, background, 5);

            Console.WriteLine($"Accuracy: {boostedTree.GetAccuracy(signal, background):F4}");

            //// Calculate output value for each event and write to file
            boostedTree.MakeTextFile(path + "decisionTreeResultsLevelIII.txt", data);
        }

        static void LevelIV()
        {
            // Load training samples
            var signal = DataSet.ReadDataSet(path + "signal.dat");
            var background = DataSet.ReadDataSet(path + "background.dat");

            // Load data sample
            var data = DataSet.ReadDataSet(path + "decisionTreeData.dat");

            var boostedTree = new BoostedTrees();
            boostedTree.MaxDepth = 5;
            boostedTree.MaxExtra = 2;
            boostedTree.UsePruning = true;

            boostedTree.Train(signal, background, 5);

            Console.WriteLine($"Accuracy: {boostedTree.GetAccuracy(signal, background):F4}");

            //// Calculate output value for each event and write to file
            boostedTree.MakeTextFile(path + "decisionTreeResultsLevelIV.txt", data);
        }
    }
}
