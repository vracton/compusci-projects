using DongUtility;
using GraphControl;
using MotionVisualizer3D;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualizerControl;
using static GraphData.GraphDataManager;
using static WPFUtility.UtilityFunctions;

namespace Visualizer.Kinematics
{
    static internal class KinematicsDriver
    {
        private const double angle = Math.PI / 4;
        private const double mass = 3;
        private const double fieldStrength = 9.8;
        private const double airCoefficient = 0.5;
        private static readonly Vector initialPosition = new(2, 0, 0);
        private static readonly Vector initialVelocity = new(0, 0, 0);
        private const double springConstant = 9;
        private const double unstretchedSpringLength = 3;

        private static bool addConnectors = false;

        private static string path = FileUtilities.GetDocumentsFolder();

        static internal void RunText()
        {
            const double timeStep = .1;
            const double duration = 20;
            var engine = CreateEngineForText();
            var projectile = engine.Projectiles[0];
            Level1(engine, projectile);
            Run(engine, timeStep, duration, path + "Kinematics Level I.txt");

            engine = CreateEngineForText();
            projectile = engine.Projectiles[0];
            Level2(engine, projectile);
            Run(engine, timeStep, duration, path + "Kinematics Level II.txt");

            engine = CreateEngineForText();
            projectile = engine.Projectiles[0];
            Level3(engine, projectile);
            Run(engine, timeStep, duration, path + "Kinematics Level III.txt");

            engine = CreateEngineForText();
            projectile = engine.Projectiles[0];
            Challenge(engine, projectile);
            Run(engine, timeStep, duration, path + "Kinematics Challenge.txt");

            Shutdown();
        }

        static private void Run(KinematicsEngine engine, double increment, double duration, string filename)
        {
            using var output = File.CreateText(filename);

            while (engine.Time < duration)
            {
                engine.Increment(increment);
                output.WriteLine($"{engine.Time}\t{engine.Projectiles[0]}");
            }
        }

        static private KinematicsEngine CreateEngineForText()
        { 
            var engine = new KinematicsEngine();
            var projectile = new Projectile(initialPosition, initialVelocity, mass);
            engine.AddProjectile(projectile);
            return engine;
        }

        static internal void RunKinematics()
        {
            var engine = new KinematicsEngine();
            var projectile = new Projectile(initialPosition, initialVelocity, mass);
            engine.AddProjectile(projectile);

            //Level1(engine, projectile);
            //Level2(engine, projectile);
            //Level3(engine, projectile);
            //Challenge(engine, projectile);

            //Level2_1(engine, projectile);
            //Level2_2(engine, projectile);
            Level2_3(engine, projectile);
            //Level2_Challenge(engine, projectile);

            //SnLExample(engine, projectile);

            var adapter = new EngineAdapter(engine);
            var visualization = new KinematicsVisualization(adapter);
            var visualizer = MakeVisualizer(visualization);

            if (addConnectors)
            {
                visualization.ConnectorRadiusScale = .1;
                visualization.AddAnchoredConnector(0, new Vector3D(0, 0, 0), Colors.Silver);
                if (engine.Projectiles.Count > 1)
                    visualization.AddTwoParticleConnector(0, 1, Colors.Silver);
            }

            AddGraphs(visualizer, engine);

            visualizer.Show();
        }

        static private MotionVisualizer3DControl MakeVisualizer(IVisualization visualization)
        {
            Timeline.MaximumPoints = 10000;

            var fullViz = new MotionVisualizer3DControl(visualization)
            {
                TimeIncrement = .01
            };

            return fullViz;
        }

        static private void AddGraphs(MotionVisualizer3DControl fullViz, KinematicsEngine engine)
        {
            fullViz.Manager.Add3DGraph("Position", () => engine.Time, () => engine.Projectiles[0].Position, "Time (s)", "Position (m)");
            fullViz.Manager.Add3DGraph("Velocity", () => engine.Time, () => engine.Projectiles[0].Velocity, "Time (s)", "Velocity (m/s)");
            fullViz.Manager.Add3DGraph("Acceleration", () => engine.Time, () => engine.Projectiles[0].Acceleration, "Time (s)", "Acceleration (m/s^2)");
            //fullViz.Manager.Add3DGraph("Position", () => engine.Time, () => engine.CMPosition, "Time (s)", "CM Position (m)");
            //fullViz.Manager.Add3DGraph("Velocity", () => engine.Time, () => engine.CMVelocity, "Time (s)", "CM Velocity (m/s)");
            //fullViz.Manager.Add3DGraph("Acceleration", () => engine.Time, () => engine.CMAcceleration, "Time (s)", "CM Acceleration (m/s^2)");

            //fullViz.Manager.AddGraph([new(new GraphData.TimelinePrototype("Speed", ConvertColor(Colors.CadetBlue)), new BasicFunctionPair(()=> engine.Time, () => engine.Projectiles[0].Velocity.Magnitude)),
            //new(new GraphData.TimelinePrototype("1 m/s", ConvertColor(Colors.OrangeRed)), new BasicFunctionPair(() => engine.Time, () => 1))], "Time (s)", "Speed (m/s)");

            //fullViz.Manager.AddText("Last time to 1 m/s", ConvertColor(Colors.DarkOrchid), () => LastTimeTo1(engine));

            //fullViz.Manager.AddSingleGraph("Distance", ConvertColor(Colors.CornflowerBlue), () => engine.Time, () => Vector.Distance(engine.Projectiles[0].Position,
            //       engine.Projectiles[1].Position), "Time (s)", "Distance (m)");

            //fullViz.Manager.AddSingleGraph("Position", ConvertColor(Colors.Teal), () => engine.Projectiles[0].Position.X, () => engine.Projectiles[0].Position.Z, "X (m)", "Y (m)");

            //fullViz.AddSingleGraph("Phase space", (() => viz.getProjectile(0).Position().Z), (() => engine.Projectiles[0].Velocity.Z), Colors.Blue, "Position", "Velocity");
        }

        static double lastTime = 0;
        static private string LastTimeTo1(KinematicsEngine engine)
        {
            if (engine.Projectiles[0].Velocity.MagnitudeSquared >= 1)
            {
                lastTime = engine.Time;
            }
            return lastTime.ToString();
        }

        static private void Level1(KinematicsEngine engine, Projectile proj)
        {
            const double vi = 4;
            const double maxTime = 20;
            proj.Velocity = new Vector(0, 0, vi);
            engine.AddStopCondition(new TimeStopCondition(maxTime));
        }

        static private void Level2(KinematicsEngine engine, Projectile proj)
        {
            Level1(engine, proj);
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -fieldStrength)));
        }

        static private void Level3(KinematicsEngine engine, Projectile proj)
        {
            Level2(engine, proj);
            engine.AddForce(new AirResistanceForce(engine, airCoefficient));
        }

        static private void Challenge(KinematicsEngine engine, Projectile proj)
        {
            Level3(engine, proj);
            proj.Position = new Vector(0, 0, -4);
            proj.Velocity = new Vector(0, 0, 2);
            engine.AddForce(new FixedSpringForce(proj, springConstant, Vector.NullVector(), unstretchedSpringLength));
        }

        static private void Level2_1(KinematicsEngine engine, Projectile proj)
        {
            const double vi = 3;
            proj.Position = new Vector(0, 0, 0);
            proj.Velocity = new Vector(vi * Math.Cos(angle), 0, vi * Math.Sin(angle));
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -fieldStrength)));
            engine.AddStopCondition(new HitGroundStopCondition());
        }

        static private void Level2_2(KinematicsEngine engine, Projectile proj)
        {
            Level2_1(engine, proj);
            engine.AddForce(new AirResistanceForce(engine, airCoefficient));
        }

        static private void Level2_3(KinematicsEngine engine, Projectile proj)
        {
            Vector initialPosition = new(-1, 1, -3);
            Vector initialVelocity = new(-5, -1, -3);

            proj.Position = initialPosition;
            proj.Velocity = initialVelocity;
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -fieldStrength)));
            engine.AddForce(new AirResistanceForce(engine, airCoefficient));
            engine.AddForce(new FixedSpringForce(proj, springConstant, Vector.NullVector(), unstretchedSpringLength));
            addConnectors = true;
        }

        static private void Level2_Challenge(KinematicsEngine engine, Projectile proj)
        {
            Vector initialPosition = new(2, 2, 2);
            Vector initialVelocity = new(1, -3, 4);
            const double mass2 = 5;

            Level2_3(engine, proj);
            var proj2 = new Projectile(initialPosition, initialVelocity, mass2);
            engine.AddProjectile(proj2);
            engine.AddForce(new ProjectileBoundSpringForce(proj, proj2, springConstant, unstretchedSpringLength));
            engine.AddForce(new ProjectileBoundSpringForce(proj2, proj, springConstant, unstretchedSpringLength));
        }

        static private void SnLExample(KinematicsEngine engine, Projectile proj)
        {
            const double unstretchedLength = 1;
            const double mass = 5;
            const double springConstant = 30;
            Vector initialPosition = new(.3, 0, -unstretchedLength * 2);

            proj.Position = initialPosition;
            proj.Mass = mass;
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddForce(new FixedSpringForce(proj, springConstant, new Vector(0, 0, 0), unstretchedLength));
            addConnectors = true;
        }

    }
}
