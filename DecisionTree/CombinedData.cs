using System;
using System.Collections.Generic;
using System.Text;

namespace DecisionTree
{
    public class CombinedData
    {
        public List<(DataPoint p, bool isSignal)> Points { get; set; } = new();

        public int Count
        {
            get
            {
                return Points.Count;
            }
        }

        public CombinedData(DataSet signal, DataSet background)
        {
            foreach (DataPoint p in signal.Points)
            {
                Points.Add((p, true));
            }
            foreach (DataPoint p in background.Points)
            {
                Points.Add((p, false));
            }
        }
        public List<(double val, bool isSignal)> SortedBy(int varInd)
        {
            List<(double val, bool isSignal)> sorted = new();

            foreach (var (p, isSignal) in Points)
            {
                sorted.Add((p.Variables[varInd], isSignal));
            }

            sorted.Sort((a, b) => a.val.CompareTo(b.val));
            return sorted;
        }

        public List<(double val, bool isSignal, double weight)> SortedByWithWeights(int varInd, List<double> weights)
        {
            List<(double val, bool isSignal, double weight)> sorted = new();

            for (int i=0; i<Points.Count; i++)
            {
                var (p, isSignal) = Points[i];
                sorted.Add((p.Variables[varInd], isSignal, weights[i]));
            }

            sorted.Sort((a, b) => a.val.CompareTo(b.val));
            return sorted;
        }

        public void Shuffle()
        {
            Random rand = new();
            for (int i = Points.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                var temp = Points[i];
                Points[i] = Points[j];
                Points[j] = temp;
            }
        }
    }
}
