using DongUtility;
using System;
using System.Windows.Media;
using MotionVisualizer3D;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;
using GraphData;
using static GraphData.GraphDataManager;
using System.Runtime.InteropServices;

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
            double[] level2Parameters = [-2.2, -0.5, -0.4];
            double[] bestLevel2Parameters = (double[])level2Parameters.Clone();
            double bestLevel2Time = RunOnceLevel2(level2Parameters);

            //const double startSlopeMin = -5;
            //const double startSlopeMax = 1;
            //const double middleSlopeMin = -5;
            //const double middleSlopeMax = 1;
            //const double endSlopeMin = -5;
            //const double endSlopeMax = 1;
            //const double slopeStep = 0.5;

            const double startSlopeMin = -3.0;
            const double startSlopeMax = -1.0;
            const double middleSlopeMin = -1;
            const double middleSlopeMax = 0;
            const double endSlopeMin = -1;
            const double endSlopeMax = 0;
            const double slopeStep = 0.1;

            for (double mStart = startSlopeMin; mStart <= startSlopeMax; mStart += slopeStep)
            {
                for (double mMid = middleSlopeMin; mMid <= middleSlopeMax; mMid += slopeStep)
                {
                    for (double mEnd = endSlopeMin; mEnd <= endSlopeMax; mEnd += slopeStep)
                    {
                        level2Parameters[0] = mStart;
                        level2Parameters[1] = mMid;
                        level2Parameters[2] = mEnd;

                        double time = RunOnceLevel2(level2Parameters);
                        if (time < bestLevel2Time)
                        {
                            bestLevel2Time = time;
                            bestLevel2Parameters = (double[])level2Parameters.Clone();
                        }
                    }
                }
            }

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

            // If you don't want to watch the results of your optimized parameters here,
            // comment this line out.

            Console.WriteLine($"Best level 2 time: {bestLevel2Time:F6} s at mStart={bestLevel2Parameters[0]}, mMid={bestLevel2Parameters[1]}, mEnd={bestLevel2Parameters[2]}");
            VisualizeFastestDescentLevel2(bestLevel2Parameters);
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

        private const double maxTime = 10;

        static private (KinematicsEngine, Path) SetupEngine(params double[] parameters)
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

        static private (KinematicsEngine, Path) SetupEngineLevel2(params double[] parameters)
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

            AddGraphs(engine, fullViz.Manager);
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

            AddGraphs(engine, fullViz.Manager);
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 4)).ToString() + " s");
            fullViz.Manager.AddText("Parameters", System.Drawing.Color.DarkSlateGray,
                () => $"mStart={Math.Round(parameters[0], 6)}, mMid={Math.Round(parameters[1], 6)}, mEnd={Math.Round(parameters[2], 6)}");

            fullViz.SlowDraw = true;
            fullViz.TimeIncrement = .01;

            fullViz.Show();
        }

        static private void AddGraphs(KinematicsEngine engine, GraphDataManager manager)
        {
            var xTimeline = new TimelineInfo(new TimelinePrototype("x Position", System.Drawing.Color.Red), new BasicFunctionPair(() => engine.Time, () => engine.Projectiles[0].Position.X));
            var yTimeline = new TimelineInfo(new TimelinePrototype("y Position", System.Drawing.Color.Green), new BasicFunctionPair(() => engine.Time, () => engine.Projectiles[0].Position.Y));
            var zTimeline = new TimelineInfo(new TimelinePrototype("z Position", System.Drawing.Color.Blue), new BasicFunctionPair(() => engine.Time, () => engine.Projectiles[0].Position.Z));
            var xTarget = new TimelineInfo(new TimelinePrototype("x Target", System.Drawing.Color.IndianRed), new BasicFunctionPair(() => engine.Time, () => 0));
            var yTarget = new TimelineInfo(new TimelinePrototype("y Target", System.Drawing.Color.LawnGreen), new BasicFunctionPair(() => engine.Time, () => 0));
            var zTarget = new TimelineInfo(new TimelinePrototype("z Target", System.Drawing.Color.DodgerBlue), new BasicFunctionPair(() => engine.Time, () => 0));
            manager.AddGraph([xTimeline, yTimeline, zTimeline, xTarget, yTarget, zTarget], "Time (s)", "Position (m)");
        }

    }
}
