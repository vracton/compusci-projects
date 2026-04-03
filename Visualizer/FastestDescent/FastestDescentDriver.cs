using DongUtility;
using System;
using System.Windows.Media;
using MotionVisualizer3D;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;
using GraphData;
using static GraphData.GraphDataManager;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Visualizer.FastestDescent
{
    class FastestDescentDriver
    {
        static internal void RunFastestDescent()
        {
            //level 1
            ////a must be 0, and b=10c-1 - we optimize c to minimize time
            //double[] parameters = [0, 0, .1];
            //double[] bestParams = (double[])parameters.Clone();
            //double bestTime = RunOnce(parameters);

            ////1d grid search for c
            //double start = -5;
            //double end = 5;
            //double eps = 1e-2;
            //for (double c = start; c <= end; c += eps)
            //{
            //    parameters[2] = c;
            //    parameters[1] = 10 * c - 1;
            //    double time = RunOnce(parameters);
            //    if (time < bestTime)
            //    {
            //        bestTime = time;
            //        bestParams = (double[])parameters.Clone();
            //    }
            //}

            //level 2
            //(-10,0,10) -> (-6, 0, 8) -> (0, 0, 0)
            //double[] level2Parameters = [-2.2, -0.5, -0.4];
            //double[] bestLevel2Parameters = (double[])level2Parameters.Clone();
            //double bestLevel2Time = RunOnceLevel2(level2Parameters);

            //const double startSlopeMin = -5;
            //const double startSlopeMax = 1;
            //const double middleSlopeMin = -5;
            //const double middleSlopeMax = 1;
            //const double endSlopeMin = -5;
            //const double endSlopeMax = 1;
            //const double slopeStep = 0.5;

            //const double startSlopeMin = -3.0;
            //const double startSlopeMax = -1.0;
            //const double middleSlopeMin = -1;
            //const double middleSlopeMax = 0;
            //const double endSlopeMin = -1;
            //const double endSlopeMax = 0;
            //const double slopeStep = 0.1;

            //for (double mStart = startSlopeMin; mStart <= startSlopeMax; mStart += slopeStep)
            //{
            //    for (double mMid = middleSlopeMin; mMid <= middleSlopeMax; mMid += slopeStep)
            //    {
            //        for (double mEnd = endSlopeMin; mEnd <= endSlopeMax; mEnd += slopeStep)
            //        {
            //            level2Parameters[0] = mStart;
            //            level2Parameters[1] = mMid;
            //            level2Parameters[2] = mEnd;

            //            double time = RunOnceLevel2(level2Parameters);
            //            if (time < bestLevel2Time)
            //            {
            //                bestLevel2Time = time;
            //                bestLevel2Parameters = (double[])level2Parameters.Clone();
            //            }
            //        }
            //    }
            //}

            //const double iters = 100;
            //const double alpha = 1e-3;
            //double eps = 1e-1;

            //for (int i = 0; i < iters; i++)
            //{
            //    double[] gradient = new double[level2Parameters.Length];

            //    for (int j = 0; j < level2Parameters.Length; j++)
            //    {
            //        double[] plusEp = (double[])level2Parameters.Clone();
            //        double[] minusEp = (double[])level2Parameters.Clone();
            //        plusEp[j] += eps;
            //        minusEp[j] -= eps;

            //        double timeP = RunOnceLevel2(plusEp);
            //        double timeM = RunOnceLevel2(minusEp);
            //        gradient[j] = (timeP - timeM) / (2 * eps);
            //    }

            //    for (int j = 0; j < level2Parameters.Length; j++)
            //    {
            //        level2Parameters[j] -= alpha * gradient[j];
            //    }

            //    double time = RunOnceLevel2(level2Parameters);
            //    if (time < bestLevel2Time)
            //    {
            //        bestLevel2Time = time;
            //        bestLevel2Parameters = (double[])level2Parameters.Clone();
            //    }
            //}

            //level 3
            ////(-10, -10, 10) -> (-6, -2, 8) -> (0, 0, 0)
            ////double[] level3Parameters = [0.5, 0.5, -3.5, -0.5, 0.5, -0.5];
            //double[] level3Parameters = [1.75, 0.25, -4.75, -0.45, 0.35, -0.35]; //didnt change after 3rd
            //double[] bestLevel3Parameters = (double[])level3Parameters.Clone();
            //double bestLevel3Time = RunOnceLevel3(level3Parameters);

            ////const double myStartMin = -3.5;
            ////const double myStartMax = 0.5;
            ////const double myMidMin = -3.5;
            ////const double myMidMax = 0.5;
            ////const double mzStartMin = -3.5;
            ////const double mzStartMax = 0.5;
            ////const double mzMidMin = -3.5;
            ////const double mzMidMax = 0.5;
            ////const double myEndMin = -3.5;
            ////const double myEndMax = 0.5;
            ////const double mzEndMin = -3.5;
            ////const double mzEndMax = 0.5;
            ////const double level3Step = 1.0;

            ////const double myStartMin = 0.0;
            ////const double myStartMax = 2.0;
            ////const double myMidMin = 0.0;
            ////const double myMidMax = 2.0;
            ////const double mzStartMin = -5;
            ////const double mzStartMax = -3;
            ////const double mzMidMin = -0.5;
            ////const double mzMidMax = -0.5;
            ////const double myEndMin = 0.0;
            ////const double myEndMax = 2.0;
            ////const double mzEndMin = -0.5;
            ////const double mzEndMax = -0.5;
            ////const double level3Step = 1.0;

            ////const double myStartMin = 1.5;
            ////const double myStartMax = 5.5;
            ////const double myMidMin = 0.5;
            ////const double myMidMax = 0.5;
            ////const double mzStartMin = -8;
            ////const double mzStartMax = -4;
            ////const double mzMidMin = -0.5;
            ////const double mzMidMax = -0.5;
            ////const double myEndMin = 0.5;
            ////const double myEndMax = 0.5;
            ////const double mzEndMin = -0.5;
            ////const double mzEndMax = -0.5;
            ////const double level3Step = 1.0;

            //const double myStartMin = 1.75;
            //const double myStartMax = 2.25;
            //const double myMidMin = 0.25;
            //const double myMidMax = 0.75;
            //const double mzStartMin = -5.25;
            //const double mzStartMax = -4.74;
            //const double mzMidMin = -0.75;
            //const double mzMidMax = -0.25;
            //const double myEndMin = 0.25;
            //const double myEndMax = 0.75;
            //const double mzEndMin = -0.75;
            //const double mzEndMax = -0.25;
            //const double level3Step = 0.1;

            //string level3ProgressPath = "level3_progress.txt";
            //int level3Iteration = 0;
            //int level3TotalIterations =
            //    ((int)Math.Round((myStartMax - myStartMin) / level3Step) + 1) *
            //    ((int)Math.Round((myMidMax - myMidMin) / level3Step) + 1) *
            //    ((int)Math.Round((mzStartMax - mzStartMin) / level3Step) + 1) *
            //    ((int)Math.Round((mzMidMax - mzMidMin) / level3Step) + 1) *
            //    ((int)Math.Round((myEndMax - myEndMin) / level3Step) + 1) *
            //    ((int)Math.Round((mzEndMax - mzEndMin) / level3Step) + 1);
            //File.WriteAllText(level3ProgressPath, $"Level 3 iterations: 0/{level3TotalIterations}");

            //for (double myStart = myStartMin; myStart <= myStartMax; myStart += level3Step)
            //{
            //    for (double myMid = myMidMin; myMid <= myMidMax; myMid += level3Step)
            //    {
            //        for (double mzStart = mzStartMin; mzStart <= mzStartMax; mzStart += level3Step)
            //        {
            //            for (double mzMid = mzMidMin; mzMid <= mzMidMax; mzMid += level3Step)
            //            {
            //                for (double myEnd = myEndMin; myEnd <= myEndMax; myEnd += level3Step)
            //                {
            //                    for (double mzEnd = mzEndMin; mzEnd <= mzEndMax; mzEnd += level3Step)
            //                    {
            //                        level3Parameters[0] = myStart;
            //                        level3Parameters[1] = myMid;
            //                        level3Parameters[2] = mzStart;
            //                        level3Parameters[3] = mzMid;
            //                        level3Parameters[4] = myEnd;
            //                        level3Parameters[5] = mzEnd;
            //                        level3Iteration++;
            //                        File.WriteAllText(level3ProgressPath,
            //                            $"Level 3 iterations: {level3Iteration}/{level3TotalIterations}\n" +
            //                            $"myStart={myStart}, myMid={myMid}, mzStart={mzStart}, mzMid={mzMid}, myEnd={myEnd}, mzEnd={mzEnd}");

            //                        double time = RunOnceLevel3(level3Parameters);
            //                        if (time < bestLevel3Time)
            //                        {
            //                            bestLevel3Time = time;
            //                            bestLevel3Parameters = (double[])level3Parameters.Clone();
            //                            File.WriteAllText(level3ProgressPath,
            //                                $"Level 3 iterations: {level3Iteration}/{level3TotalIterations}\n" +
            //                                $"myStart={myStart}, myMid={myMid}, mzStart={mzStart}, mzMid={mzMid}, myEnd={myEnd}, mzEnd={mzEnd}\n" +
            //                                $"bestTime={bestLevel3Time}");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //File.WriteAllText(level3ProgressPath,
            //    $"Level 3 complete: {level3Iteration}/{level3TotalIterations}\n" +
            //    $"bestTime={bestLevel3Time}\n" +
            //    $"myStart={bestLevel3Parameters[0]}, myMid={bestLevel3Parameters[1]}, mzStart={bestLevel3Parameters[2]}, mzMid={bestLevel3Parameters[3]}, myEnd={bestLevel3Parameters[4]}, mzEnd={bestLevel3Parameters[5]}");

            //level 4
            //(-10, -10, 10), (-9, -9, -4), (-8, -5, -9), (-3, -8, -8), (0, 0, 0) in any order
            //(-10, -10, 10) should probably start beacuse highest

            Vector startPoint = new(-10, -10, 10);
            Vector[] remainingPoints =
            [
                new Vector(-9, -9, -4),
                new Vector(-8, -5, -9),
                new Vector(-3, -8, -8),
                new Vector(0, 0, 0)
            ];

            double[] bestLevel4Parameters = new double[10];
            Vector[] bestLevel4Order = [startPoint, remainingPoints[0], remainingPoints[1], remainingPoints[2], remainingPoints[3]];
            double bestLevel4Time = double.MaxValue;
            bool foundValidLevel4 = false;

            const int level4Iters = 10;
            const double level4Alpha = 5e-2;
            const double level4Eps = 2.5e-1;
            string level4ProgressPath = "level4_progress.txt";
            int permutationIndex = 0;
            const int totalPermutations = 24;
            const int level4ParameterCount = 100;
            int runsPerPermutation = 1 + level4Iters * (2 * level4ParameterCount + 1);
            int totalRuns = totalPermutations * runsPerPermutation;
            int currentRun = 0;

            foreach (var remainingOrder in GetPermutations(remainingPoints))
            {
                permutationIndex++;
                Vector[] currentOrder = [startPoint, remainingOrder[0], remainingOrder[1], remainingOrder[2], remainingOrder[3]];
                double[] currentParameters = InitializeLevel4Parameters(currentOrder);
                double[] bestOrderParameters = new double[10];
                double bestOrderTime = double.MaxValue;
                currentRun++;
                double currentTime = RunOnceLevel4(currentOrder, currentParameters);
                if (!double.IsFinite(currentTime) || currentTime == double.MaxValue)
                {
                    File.WriteAllText(level4ProgressPath,
                        $"Level 4 permutation: {permutationIndex}/{totalPermutations}\n" +
                        $"runs tested: {currentRun}/{totalRuns}\n" +
                        $"order={FormatOrder(currentOrder)}\n" +
                        $"iteration=0/{level4Iters}\n" +
                        $"status=invalid random start\n" +
                        $"randomStartParameters={string.Join(", ", currentParameters)}\n" +
                        $"bestOrderTime={bestOrderTime}\n" +
                        $"bestOverallTime={(foundValidLevel4 ? bestLevel4Time : "N/A")}\n" +
                        $"bestOverallOrder={(foundValidLevel4 ? FormatOrder(bestLevel4Order) : "N/A")}\n" +
                        $"bestOverallParameters={(foundValidLevel4 ? string.Join(", ", bestLevel4Parameters) : "N/A")}");
                    continue;
                }

                if (currentTime < bestOrderTime)
                {
                    bestOrderTime = currentTime;
                    bestOrderParameters = (double[])currentParameters.Clone();
                }

                File.WriteAllText(level4ProgressPath,
                    $"Level 4 permutation: {permutationIndex}/{totalPermutations}\n" +
                    $"runs tested: {currentRun}/{totalRuns}\n" +
                    $"order={FormatOrder(currentOrder)}\n" +
                    $"iteration=0/{level4Iters}\n" +
                    $"currentTime={currentTime}\n" +
                    $"bestOrderTime={bestOrderTime}\n" +
                    $"bestOverallTime={(foundValidLevel4 ? bestLevel4Time : "N/A")}\n" +
                    $"bestOverallOrder={(foundValidLevel4 ? FormatOrder(bestLevel4Order) : "N/A")}\n" +
                    $"bestOverallParameters={(foundValidLevel4 ? string.Join(", ", bestLevel4Parameters) : "N/A")}");

                for (int i = 0; i < level4Iters; i++)
                {
                    double[] gradient = new double[currentParameters.Length];
                    for (int j = 0; j < currentParameters.Length; j++)
                    {
                        double[] plusEp = (double[])currentParameters.Clone();
                        double[] minusEp = (double[])currentParameters.Clone();
                        plusEp[j] += level4Eps;
                        minusEp[j] -= level4Eps;

                        currentRun++;
                        double timeP = RunOnceLevel4(currentOrder, plusEp);
                        currentRun++;
                        double timeM = RunOnceLevel4(currentOrder, minusEp);
                        if (!double.IsFinite(timeP) || !double.IsFinite(timeM) ||
                            timeP == double.MaxValue || timeM == double.MaxValue)
                        {
                            gradient[j] = 0;
                        }
                        else
                        {
                            gradient[j] = (timeP - timeM) / (2 * level4Eps);
                        }
                    }

                    for (int j = 0; j < currentParameters.Length; j++)
                    {
                        currentParameters[j] -= level4Alpha * gradient[j];
                    }

                    currentRun++;
                    currentTime = RunOnceLevel4(currentOrder, currentParameters);
                    if (!double.IsFinite(currentTime))
                    {
                        currentTime = double.MaxValue;
                    }
                    if (currentTime < bestOrderTime)
                    {
                        bestOrderTime = currentTime;
                        bestOrderParameters = (double[])currentParameters.Clone();
                    }

                    File.WriteAllText(level4ProgressPath,
                        $"Level 4 permutation: {permutationIndex}/{totalPermutations}\n" +
                        $"runs tested: {currentRun}/{totalRuns}\n" +
                        $"order={FormatOrder(currentOrder)}\n" +
                        $"iteration={i + 1}/{level4Iters}\n" +
                        $"currentTime={currentTime}\n" +
                        $"bestOrderTime={bestOrderTime}\n" +
                        $"bestOverallTime={(foundValidLevel4 ? bestLevel4Time : "N/A")}\n" +
                        $"bestOverallOrder={(foundValidLevel4 ? FormatOrder(bestLevel4Order) : "N/A")}\n" +
                        $"bestOverallParameters={(foundValidLevel4 ? string.Join(", ", bestLevel4Parameters) : "N/A")}");
                }

                if (bestOrderTime < bestLevel4Time)
                {
                    bestLevel4Time = bestOrderTime;
                    bestLevel4Parameters = bestOrderParameters;
                    bestLevel4Order = currentOrder;
                    foundValidLevel4 = true;
                }
            }

            if (!foundValidLevel4)
            {
                File.WriteAllText(level4ProgressPath,
                    $"Level 4 complete\n" +
                    $"runs tested: {currentRun}/{totalRuns}\n" +
                    $"status=no valid paths found\n" +
                    $"bestTime=N/A\n" +
                    $"order=N/A\n" +
                    $"parameters=N/A");
                Console.WriteLine("Level 4 failed to find a valid path.");
                return;
            }

            File.WriteAllText(level4ProgressPath,
                $"Level 4 complete\n" +
                $"runs tested: {currentRun}/{totalRuns}\n" +
                $"bestTime={bestLevel4Time}\n" +
                $"order={FormatOrder(bestLevel4Order)}\n" +
                $"parameters={string.Join(", ", bestLevel4Parameters)}");

            Console.WriteLine($"Best level 4 time: {bestLevel4Time:F6} s");
            Console.WriteLine($"Best level 4 order: {FormatOrder(bestLevel4Order)}");
            VisualizeFastestDescentLevel4(bestLevel4Order, bestLevel4Parameters);
        }

        static private double RunOnce(params double[] parameters)
        {
            var (engine, _) = SetupEngine(parameters);
            const double timeStep = 1e-3;
            const double maxTime = 1000;
            while (engine.Increment(timeStep))
            {
                if (engine.Time > maxTime)
                    return double.MaxValue;
            }
            return engine.Time;
        }

        static private double RunOnceLevel2(params double[] parameters)
        {
            var (engine, _) = SetupEngineLevel2(parameters);
            const double timeStep = 1e-3;
            const double maxTime = 1000;
            while (engine.Increment(timeStep))
            {
                if (engine.Time > maxTime)
                    return double.MaxValue;
            }
            return engine.Time;
        }

        static private double RunOnceLevel3(params double[] parameters)
        {
            var (engine, _) = SetupEngineLevel3(parameters);
            const double timeStep = 1e-3;
            const double maxTime = 5;
            while (engine.Increment(timeStep))
            {
                if (engine.Time > maxTime)
                    return double.MaxValue;
            }
            return engine.Time;
        }

        static private double RunOnceLevel4(Vector[] points, params double[] parameters)
        {
            var (engine, _) = SetupEngineLevel4(points, parameters);
            const double timeStep = 1e-2;
            const double maxTime = 8.67;
            while (engine.Increment(timeStep))
            {
                if (engine.Time > maxTime)
                    return double.MaxValue;
            }
            return engine.Time;
        }

        private const double maxTime = 10;

        static private (KinematicsEngine, DongUtility.Path) SetupEngine(params double[] parameters)
        {
            const double initial = -10;
            const double final = 0;

            var path = new Simple2DQuadratic(initial, final, parameters);
            var projectile = new ConstrainedProjectile(path.GetPosition(path.InitialParameter), Vector.NullVector(), 1, path); // The mass does not matter

            var engine = new KinematicsEngine();
            engine.AddProjectile(projectile);
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddStopCondition(new EndOfPathStopCondition());
            engine.AddStopCondition(new TimeStopCondition(maxTime));
            return (engine, path);
        }

        static private (KinematicsEngine, DongUtility.Path) SetupEngineLevel2(params double[] parameters)
        {
            var path = new MultiPath();
            path.AddPath(new CubicPath(-10, 10, -6, 8, parameters[0], parameters[1]));
            path.AddPath(new CubicPath(-6, 8, 0, 0, parameters[1], parameters[2]));

            var projectile = new ConstrainedProjectile(path.GetPosition(path.InitialParameter), Vector.NullVector(), 1, path); // The mass does not matter

            var engine = new KinematicsEngine();
            engine.AddProjectile(projectile);
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddStopCondition(new EndOfPathStopCondition());
            engine.AddStopCondition(new TimeStopCondition(maxTime));
            return (engine, path);
        }

        static private (KinematicsEngine, DongUtility.Path) SetupEngineLevel3(params double[] parameters)
        {
            var path = new MultiPath();
            path.AddPath(new Cubic3DPath(new Vector(-10, -10, 10), new Vector(-6, -2, 8), parameters[0], parameters[1], parameters[2], parameters[3]));
            path.AddPath(new Cubic3DPath(new Vector(-6, -2, 8), new Vector(0, 0, 0), parameters[1], parameters[4], parameters[3], parameters[5]));

            var projectile = new ConstrainedProjectile(path.GetPosition(path.InitialParameter), Vector.NullVector(), 1, path); // The mass does not matter

            var engine = new KinematicsEngine();
            engine.AddProjectile(projectile);
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddStopCondition(new EndOfPathStopCondition());
            engine.AddStopCondition(new TimeStopCondition(maxTime));
            return (engine, path);
        }

        static private (KinematicsEngine, DongUtility.Path) SetupEngineLevel4(Vector[] points, params double[] parameters)
        {
            var path = new MultiPath();
            for (int i = 0; i < points.Length - 1; i++)
            {
                int startSlopeIndex = 2 * i;
                int endSlopeIndex = 2 * (i + 1);
                path.AddPath(new Cubic3DPath(
                    points[i],
                    points[i + 1],
                    parameters[startSlopeIndex],
                    parameters[endSlopeIndex],
                    parameters[startSlopeIndex + 1],
                    parameters[endSlopeIndex + 1]));
            }

            var projectile = new ConstrainedProjectile(path.GetPosition(path.InitialParameter), Vector.NullVector(), 1, path);

            var engine = new KinematicsEngine();
            engine.AddProjectile(projectile);
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddStopCondition(new EndOfPathStopCondition());
            engine.AddStopCondition(new TimeStopCondition(maxTime));
            return (engine, path);
        }

        static private double[] InitializeLevel4Parameters(Vector[] points)
        {
            double[] parameters = new double[2 * points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector prevPoint;
                Vector nextPoint;

                if (i == 0)
                {
                    prevPoint = points[0];
                    nextPoint = points[1];
                }
                else if (i == points.Length - 1)
                {
                    prevPoint = points[i - 1];
                    nextPoint = points[i];
                }
                else
                {
                    prevPoint = points[i - 1];
                    nextPoint = points[i + 1];
                }

                double dx = nextPoint.X - prevPoint.X;
                if (dx == 0)
                {
                    parameters[2 * i] = 0;
                    parameters[2 * i + 1] = 0;
                    continue;
                }

                parameters[2 * i] = (nextPoint.Y - prevPoint.Y) / dx;
                parameters[2 * i + 1] = (nextPoint.Z - prevPoint.Z) / dx;
            }

            return parameters;
        }

        static internal void VisualizeFastestDescent(params double[] parameters)
        {
            var (engine, path) = SetupEngine(parameters);

            var visualization = new DescentVisualization(engine, path)
            {
                PathThickness = .5,
                PathColor = Colors.IndianRed,
                ProjectileSize = 1,
                ProjectileColor = Colors.NavajoWhite
            };

            var fullViz = new MotionVisualizer3DControl(visualization);

            AddGraphs(engine, fullViz.Manager, path.GetPosition(path.FinalParameter));
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 4)).ToString() + " s");
            fullViz.Manager.AddText("Parameters", System.Drawing.Color.DarkSlateGray,
                () => $"a={Math.Round(parameters[0], 6)}, b={Math.Round(parameters[1], 6)}, c={Math.Round(parameters[2], 6)}");

            fullViz.SlowDraw = true;
            fullViz.TimeIncrement = .01;

            fullViz.Show();
        }

        static internal void VisualizeFastestDescentLevel2(params double[] parameters)
        {
            var (engine, path) = SetupEngineLevel2(parameters);

            var visualization = new DescentVisualization(engine, path)
            {
                PathThickness = .5,
                PathColor = Colors.IndianRed,
                ProjectileSize = 1,
                ProjectileColor = Colors.NavajoWhite
            };

            var fullViz = new MotionVisualizer3DControl(visualization);

            AddGraphs(engine, fullViz.Manager, path.GetPosition(path.FinalParameter));
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 4)).ToString() + " s");
            fullViz.Manager.AddText("Parameters", System.Drawing.Color.DarkSlateGray,
                () => $"mStart={Math.Round(parameters[0], 6)}, mMid={Math.Round(parameters[1], 6)}, mEnd={Math.Round(parameters[2], 6)}");

            fullViz.SlowDraw = true;
            fullViz.TimeIncrement = .01;

            fullViz.Show();
        }

        static internal void VisualizeFastestDescentLevel3(params double[] parameters)
        {
            var (engine, path) = SetupEngineLevel3(parameters);

            var visualization = new DescentVisualization(engine, path)
            {
                PathThickness = .5,
                PathColor = Colors.IndianRed,
                ProjectileSize = 1,
                ProjectileColor = Colors.NavajoWhite
            };

            var fullViz = new MotionVisualizer3DControl(visualization);

            AddGraphs(engine, fullViz.Manager, path.GetPosition(path.FinalParameter));
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 4)).ToString() + " s");
            fullViz.Manager.AddText("Parameters", System.Drawing.Color.DarkSlateGray,
                () => $"myS={Math.Round(parameters[0], 4)}, myM={Math.Round(parameters[1], 4)}, mzS={Math.Round(parameters[2], 4)}, mzM={Math.Round(parameters[3], 4)}, myE={Math.Round(parameters[4], 4)}, mzE={Math.Round(parameters[5], 4)}");

            fullViz.SlowDraw = true;
            fullViz.TimeIncrement = .01;

            fullViz.Show();
        }

        static internal void VisualizeFastestDescentLevel4(Vector[] points, params double[] parameters)
        {
            var (engine, path) = SetupEngineLevel4(points, parameters);

            var visualization = new DescentVisualization(engine, path)
            {
                PathThickness = .5,
                PathColor = Colors.IndianRed,
                ProjectileSize = 1,
                ProjectileColor = Colors.NavajoWhite
            };

            var fullViz = new MotionVisualizer3DControl(visualization);

            AddGraphs(engine, fullViz.Manager, path.GetPosition(path.FinalParameter));
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 4)).ToString() + " s");
            fullViz.Manager.AddText("Order", System.Drawing.Color.DarkSlateGray, () => FormatOrder(points));
            fullViz.Manager.AddText("Parameters", System.Drawing.Color.DarkSlateGray,
                () => string.Join(", ", parameters.Select(value => Math.Round(value, 4).ToString())));

            fullViz.SlowDraw = true;
            fullViz.TimeIncrement = .01;

            fullViz.Show();
        }

        static private void AddGraphs(KinematicsEngine engine, GraphDataManager manager, Vector target)
        {
            var xTimeline = new TimelineInfo(new TimelinePrototype("x Position", System.Drawing.Color.Red), new BasicFunctionPair(() => engine.Time, () => engine.Projectiles[0].Position.X));
            var yTimeline = new TimelineInfo(new TimelinePrototype("y Position", System.Drawing.Color.Green), new BasicFunctionPair(() => engine.Time, () => engine.Projectiles[0].Position.Y));
            var zTimeline = new TimelineInfo(new TimelinePrototype("z Position", System.Drawing.Color.Blue), new BasicFunctionPair(() => engine.Time, () => engine.Projectiles[0].Position.Z));
            var xTarget = new TimelineInfo(new TimelinePrototype("x Target", System.Drawing.Color.IndianRed), new BasicFunctionPair(() => engine.Time, () => target.X));
            var yTarget = new TimelineInfo(new TimelinePrototype("y Target", System.Drawing.Color.LawnGreen), new BasicFunctionPair(() => engine.Time, () => target.Y));
            var zTarget = new TimelineInfo(new TimelinePrototype("z Target", System.Drawing.Color.DodgerBlue), new BasicFunctionPair(() => engine.Time, () => target.Z));
            manager.AddGraph([xTimeline, yTimeline, zTimeline, xTarget, yTarget, zTarget], "Time (s)", "Position (m)");
        }

        static private IEnumerable<Vector[]> GetPermutations(Vector[] points)
        {
            if (points.Length == 0)
            {
                yield return [];
                yield break;
            }

            for (int i = 0; i < points.Length; i++)
            {
                Vector current = points[i];
                Vector[] remaining = new Vector[points.Length - 1];
                int remainingIndex = 0;
                for (int j = 0; j < points.Length; j++)
                {
                    if (j == i)
                        continue;
                    remaining[remainingIndex++] = points[j];
                }

                foreach (var suffix in GetPermutations(remaining))
                {
                    Vector[] permutation = new Vector[points.Length];
                    permutation[0] = current;
                    for (int j = 0; j < suffix.Length; j++)
                    {
                        permutation[j + 1] = suffix[j];
                    }
                    yield return permutation;
                }
            }
        }

        static private string FormatOrder(IEnumerable<Vector> points)
        {
            return string.Join(" -> ", points.Select(point => point.ToString()));
        }

    }
}
