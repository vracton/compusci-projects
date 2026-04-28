using DongUtility;
using GraphData;
using MotionVisualizer3D;
using PhysicsUtility.Kinematics.Forces;
using PhysicsUtility.Kinematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using Visualizer.FastestDescent;
using static WPFUtility.UtilityFunctions;
using Path = DongUtility.Path;

namespace Visualizer.RaceToTheBottom

{
    class RaceToTheBottomDriver
    {
        private static readonly string filePath = FileUtilities.GetMainProjectDirectory() + "RaceToTheBottom/";

        public const double maxTime = 1000;

        private class TeamSet(string names, Color color, Path path)
        {
            public string Names { get; set; } = names;
            public Color Color { get; set; } = color;
            public Path Path { get; set; } = path;

            public Color InverseColor
            {
                get
                {
                    return ConvertColor(UtilityFunctions.InvertColor(ConvertColor(Color)));
                }
            }
        }

        static internal void Run()
        {
            double[] parameters = [0, 0, .1];
            // You will want to do your optimization here,
            // calling RunOnce() many times
            //double time = RunOnce(parameters);

            // If you don't want to display your result with optimized parameters here,
            // comment this line out.

            VisualizeFastestDescent(parameters);
        }

        static internal void VisualizeFastestDescent(params double[] parameters)
        {
            var engine = SetupEngine(parameters);

            var visualization = new RaceToTheBottomVisualization(engine)
            {
                PathThickness = .5,
                ProjectileSize = 1
            };

            var fullViz = new MotionVisualizer3DControl(visualization);

            fullViz.Manager.Add3DGraph("Position", () => engine.Time, () => engine.ProjectilesAndPaths[0].Projectile.Position, "Time (s)", "Position (m)");
            fullViz.Manager.AddText("Time", System.Drawing.Color.MidnightBlue, () => (Math.Round(engine.Time, 3)).ToString() + " s");
            fullViz.Manager.AddSingleGraph("Score", System.Drawing.Color.Firebrick, () => engine.Time, () => engine.ProjectilesAndPaths[0].Score, "Time (s)", "Score");

            //fullViz.SlowDraw = true;
            fullViz.TimeIncrement = .01;

            fullViz.Show();
        }

        static private double RunOnce(params double[] parameters)
        {
            var engine = SetupEngine(parameters);
            const double timeStep = .01;
            while (engine.Increment(timeStep))
            {
                if (engine.Time > maxTime)
                    return double.MaxValue;
            }
            return engine.Time;
        }

        static private RaceToTheBottomEngine SetupEngine(params double[] parameters)
        {
            var path = new YOURNAMEPath();
            var projectile = new ConstrainedProjectile(path.GetPosition(path.InitialParameter), Vector.NullVector(), 1, path); // The mass does not matter

            var engine = new RaceToTheBottomEngine(filePath + "pathOnlyData.dat");
            engine.AddSignalList(filePath + "dummySignal.txt");
            var pnp = new RaceToTheBottomEngine.ProjectileAndPath("My path", projectile, Colors.IndianRed, Colors.NavajoWhite);
            engine.AddProjectileAndPath(pnp);
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddStopCondition(new EndOfPathStopCondition());
            engine.AddStopCondition(new TimeStopCondition(maxTime));
            return engine;
        }


    }
}
