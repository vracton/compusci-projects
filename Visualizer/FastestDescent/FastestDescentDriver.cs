using DongUtility;
using System;
using System.Windows.Media;
using MotionVisualizer3D;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;
using GraphData;
using static GraphData.GraphDataManager;

namespace Visualizer.FastestDescent
{
    class FastestDescentDriver
    {
        static internal void RunFastestDescent()
        {
            double[] parameters = [0, 0, .1];
            // You will want to do your optimization here,
            // calling RunOnce() many times
            double time = RunOnce(parameters);

            // If you don't want to watch the results of your optimized parameters here,
            // comment this line out.

            VisualizeFastestDescent(parameters);
        }

        static private double RunOnce(params double[] parameters)
        {
            var (engine, _) = SetupEngine(parameters);
            const double timeStep = .01;
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
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 3)).ToString() + " s");

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
