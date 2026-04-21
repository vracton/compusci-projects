using NeuralNetStudentVersion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PredatorPreyVisualizer
{
    internal class AnswerKey
    {
        private const double xSize = 100;
        private const double ySize = 100;
        private const double timeStep = .01;
        private const int nHares = 1;

        static internal void Run()
        {
            WPFUtility.ConsoleManager.ShowConsoleWindow();
            Test();
            //Train();
            //GradientDescent();
            //Display();
        }

        static private void Test()
        {
            var perceptron = new SingleLayerPerceptron(4, 2);
            Process(perceptron);
            perceptron.InputNodes[0].Connectors[0].Weight = 2;
            Process(perceptron);
            perceptron.InputNodes[0].Connectors[0].Weight = 1.5;
            Process(perceptron);
        }

        static private Perceptron bestPerceptron = new SingleLayerPerceptron(nInput, nOutput);

        private const int nPerGeneration = 100;
        private const int nBest = 10;
        private const int nInput = 5;
        private const int nOutput = 2;
        private const double standardDeviation = .1;

        static private void Train()
        {
            var perceptrons = new List<Perceptron>();
            double previousBest = 0;
            double improvement;
            do
            {
                perceptrons = Generation(perceptrons, out double bestTime);
                improvement = bestTime - previousBest;
                previousBest = bestTime;
                Console.WriteLine($"New best time: {bestTime}");
            } while (improvement > 0);
            bestPerceptron = perceptrons[0];
        }

        // This doesn't work very well
        static private void GradientDescent()
        {
            var perceptronClone = bestPerceptron.Clone();
            perceptronClone.RandomWeights(5);
            double initialTime = Process(perceptronClone);

            bool improvement = true;
            while (improvement)
            {
                var newPerceptron = OneStep(initialTime, perceptronClone, out improvement);
                initialTime = Process(newPerceptron);
                bestPerceptron = newPerceptron;
            }
        }

        static private Perceptron OneStep(double baseline, Perceptron perceptron, out bool improvement)
        {
            var gradient = GetGradient(baseline, perceptron);
            var newPerceptron = ApplyGradient(baseline, perceptron, gradient, 1);

            double newTime = Process(newPerceptron);
            if (newTime > baseline)
            {
                improvement = true;
                return newPerceptron;
            }
            else
            {
                improvement = false;
                return perceptron;
            }
        }

        private const double MinShift = .1;

        static private List<double> GetGradient(double baseline, Perceptron perceptron)
        {
            Console.WriteLine("Calculating gradient:");
            List<double> gradient = [];

            foreach (var connector in perceptron.Connectors)
            {
                double derivative = 0;
                double smallShift = .001;
                do
                {

                    // Find the ideal derivative size
                    double difference = connector.Weight * smallShift;
                    connector.Weight += difference;
                    double newTime = Process(perceptron);
                    connector.Weight -= difference;
                    if (Math.Abs(newTime - baseline) < MinShift)
                    {
                        Console.WriteLine("Not big enough change!  Reattempting...");
                        smallShift *= 10;
                        if (smallShift > 1e6) // Nothing seems to work
                        {
                            Console.WriteLine("No change seemed to work!  Returning zero derivative");
                            break;
                        }
                    }
                    else
                    {
                        derivative = (newTime - baseline) / difference;
                    }
                } while (derivative == 0);

                gradient.Add(derivative);
            }
            return gradient;
        }

        static private Perceptron ApplyGradient(double baseline, Perceptron perceptron, List<double> gradient, double learningRate)
        {
            Console.WriteLine("Testing new perceptron:");
            // Apply the gradient
            var newPerceptron = ApplyWeights(perceptron, gradient, learningRate);

            double checkTime = Process(newPerceptron);
            while (checkTime < baseline && learningRate > 1e-9)
            {
                Console.WriteLine($"Learning rate too large!  Changing to {learningRate / 10}");
                learningRate /= 10;
                newPerceptron = ApplyWeights(perceptron, gradient, learningRate);
                checkTime = Process(newPerceptron);
            }

            return newPerceptron;
        }

        static private Perceptron ApplyWeights(Perceptron perceptron, List<double> gradient, double learningRate)
        {
            int index = 0;
            var newPerceptron = perceptron.Clone();
            foreach (var connector in newPerceptron.Connectors)
            {
                connector.Weight -= learningRate * gradient[index];
                ++index;
            }
            return newPerceptron;
        }

        static private List<Perceptron> Generation(List<Perceptron> previousBest, out double bestTime)
        {
            var newPerceptrons = new List<Perceptron>();

            if (previousBest.Count == 0)
            {
                for (int i = 0; i < nPerGeneration; ++i)
                {
                    var perceptron = new SingleLayerPerceptron(nInput, nOutput);
                    perceptron.RandomWeights(standardDeviation);
                    newPerceptrons.Add(perceptron);
                }
            }
            else
            {
                int nEach = nPerGeneration / previousBest.Count;
                foreach (var perceptron in previousBest)
                {
                    newPerceptrons.Add(perceptron);
                    for (int i = 1; i < nEach; ++i)
                    {
                        newPerceptrons.Add(perceptron.RandomClone(standardDeviation));
                    }
                }
            }

            var bestPeceptrons = new SortedList<double, Perceptron>();
            foreach (var perceptron in newPerceptrons)
            {
                double score = Process(perceptron);
                if (!bestPeceptrons.ContainsKey(score))
                    bestPeceptrons.Add(score, perceptron);
            }

            var answer = new List<Perceptron>();
            int counter = 0;
            bestTime = 0;

            foreach (var pair in bestPeceptrons.Reverse()) // Reverse to make highest first
            {
                if (counter == 0)
                {
                    bestTime = pair.Key;
                }
                answer.Add(pair.Value);
                ++counter;
                if (counter >= nBest)
                {
                    break;
                }
            }
            return answer;
        }

        static private double Process(Perceptron perceptron)
        {
            var arena = new PredatorPreyEngine(xSize, ySize, perceptron, nHares);

            bool keepRunning = true;
            while (keepRunning)
            {
                arena.Tick(arena.Time + timeStep);
                keepRunning = !arena.Hares[0].IsDead && arena.Time < 100;
            }
            Console.WriteLine($"Time: {arena.Time}");
            if (arena.Time > 100)
            {
                return arena.Time * arena.GetObjectsOfType<Hare>().Count();
            }
            return arena.Time;
        }

        static private void Display()
        {
            var window = new MainWindow(xSize, ySize, timeStep, bestPerceptron, nHares);
            window.Show();
        }
    }
}
