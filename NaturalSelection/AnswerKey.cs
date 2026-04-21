//using Arena;
//using DongUtility;
//using GraphData;
//using NeuralNetStudentVersion;
//using PredatorPreyVisualizer;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static GraphData.GraphDataManager;

//namespace NaturalSelection
//{
//    internal class AnswerKey
//    {
//        private const double xSize = 50;
//        private const double ySize = 50;
//        private const double timeStep = .01;

//        static private Perceptron bestPerceptron = new SingleLayerPerceptron(4, 4);

//        private const int nPerGeneration = 1000;
//        private const int nBest = 10;
//        private const int nInput = 4;
//        private const int nOutput = 4;
//        private static double standardDeviation = 10;

//        static private void Train()
//        {
//            var perceptrons = new List<Perceptron>();
//            double improvement = 0;
//            double previousBest = 0;
//            do
//            {
//                perceptrons = Generation(perceptrons, out double bestTime);
//                improvement = bestTime - previousBest;
//                previousBest = bestTime;
//                Console.WriteLine($"New best time: {bestTime}");
//            } while (improvement > 0);
//            bestPerceptron = perceptrons[0];
//        }

//        static private List<Perceptron> Generation(List<Perceptron> previousBest, out double bestTime)
//        {
//            var newPerceptrons = new List<Perceptron>();

//            if (previousBest.Count == 0)
//            {
//                for (int i = 0; i < nPerGeneration; ++i)
//                {
//                    var perceptron = new SingleLayerPerceptron(nInput, nOutput);
//                    perceptron.RandomWeights(standardDeviation);
//                    newPerceptrons.Add(perceptron);
//                }
//            }
//            else
//            {
//                int nEach = nPerGeneration / previousBest.Count;
//                foreach (var perceptron in previousBest)
//                {
//                    newPerceptrons.Add(perceptron);
//                    for (int i = 1; i < nEach; ++i)
//                    {
//                        standardDeviation /= 2;
//                        newPerceptrons.Add(perceptron.RandomClone(standardDeviation));
//                    }
//                }
//            }

//            var bestPeceptrons = new SortedList<double, Perceptron>();
//            foreach (var perceptron in newPerceptrons)
//            {
//                double score = Process(perceptron);
//                Console.WriteLine($"Score: {score}");
//                if (!bestPeceptrons.ContainsKey(score))
//                    bestPeceptrons.Add(score, perceptron);
//            }

//            var answer = new List<Perceptron>();
//            int counter = 0;
//            bestTime = 0;

//            foreach (var pair in bestPeceptrons.Reverse()) // Reverse to make highest first
//            {
//                if (counter == 0)
//                {
//                    bestTime = pair.Key;
//                }
//                answer.Add(pair.Value);
//                ++counter;
//                if (counter >= nBest)
//                {
//                    break;
//                }
//            }
//            return answer;
//        }

//        static private double Process(Perceptron perceptron)
//        {
//            var arena = SetupArena();

//            return RunArena(arena, perceptron);
//        }

//        private const int nHares = 100;
//        // private const int nLynx = 20;
//        private const int size = 30;
//        private const int target = 300;


//        static internal void Run()
//        {
//            WPFUtility.ConsoleManager.ShowConsoleWindow();
//            Train();
//            Display();
//        }

//        static private EcologyArena SetupArena()
//        {
//            var arena = new EcologyArena(size, size);
//            arena.AddAnimals<Hare>(nHares);
//            return arena;
//        }

//        static private double RunArena(EcologyArena arena, Perceptron harePerceptron)
//        {
//            AddPerceptrons(arena, harePerceptron);
//            bool keepRunning = true;

//            while (keepRunning && arena.CountObjects("Hare") > 0)
//            {
//                arena.Tick(arena.Time + 1);
//            }

//            return arena.Time;
//        }

//        static private void Display()
//        {
//            var arena = SetupArena();
//            AddPerceptrons(arena, bestPerceptron);
//            var sim = new EcologySim(arena);
//            sim.TimePerTurn = .1;

//            sim.Arena.Manager.AddGraph(
//            [ new(new TimelinePrototype("Hare", Color.SandyBrown), new BasicFunctionPair(() => arena.Time, () => arena.GetObjectsOfType<Hare>().Count())),
//            new(new TimelinePrototype("Lynx", Color.PaleVioletRed), new BasicFunctionPair(() => arena.Time, () => arena.GetObjectsOfType<Lynx>().Count()))
//            ], "Time (days)", "Population");

//            sim.Show();
//        }
//    }
//}
