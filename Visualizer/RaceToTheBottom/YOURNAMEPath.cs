using DecisionTree;
using DongUtility;
using System.Collections.Generic;
using System.IO;

namespace Visualizer.RaceToTheBottom
{
    public class YOURNAMEPath : MultiPath
    {
        private static readonly string filePath = FileUtilities.GetMainProjectDirectory() + "RaceToTheBottom/";

        public YOURNAMEPath()
        {
            Vector initialPosition = new(0, 0, 101); // You can change the x and y values
                                                     // to be anything you want
                                                     // z has to be 101
            using var file = File.OpenText(filePath + "dummySignal.txt");

            var ds = DataSet.ReadDataSet(filePath + "pathOnlyData.dat");
            var pointList = new List<Vector>();
            foreach (var dataPoint in ds.Points)
            {
                Vector position = new(dataPoint.Variables[0], dataPoint.Variables[1], dataPoint.Variables[2]);
                pointList.Add(position);
            }

            Vector previousPosition = initialPosition;
            while (!file.EndOfStream)
            {
                var line = file.ReadLine();
                if (line == null || line.Length == 0)
                    continue;

                int index = int.Parse(line);

                Vector location = pointList[index];

                var segment = new SimpleLinearPath(previousPosition, location);
                AddPath(segment);

                previousPosition = location;
            }
        }

        public YOURNAMEPath(int datasetNumber)
        {
            Vector initialPosition = new(0, 0, 101); // You can change the x and y values
                                                     // to be anything you want
                                                     // z has to be 101
            var signalData = DataSet.ReadDataSet(filePath + $"signal{datasetNumber}TrainingSample.dat");

            Vector previousPosition = initialPosition;
            foreach (var dataPoint in signalData.Points)
            {
                Vector location = new(dataPoint.Variables[0], dataPoint.Variables[1], dataPoint.Variables[2]);

                var segment = new SimpleLinearPath(previousPosition, location);
                AddPath(segment);

                previousPosition = location;
            }
        }
    }
}
