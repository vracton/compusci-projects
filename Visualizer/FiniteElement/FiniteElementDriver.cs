using DongUtility;
using GraphControl;
using System.Windows.Media;
using MotionVisualizer3D;
using PhysicsUtility.Kinematics;
using PhysicsUtility.Kinematics.Forces;
using Visualizer.Kinematics;

namespace Visualizer.FiniteElement
{
    static internal class FiniteElementDriver
    {
        static private Color ConnectorColor = Colors.Green;

        // In general, this script will not need changing, or only small changes
        static internal void RunFiniteElement()
        {
            var engine = new KinematicsEngine();
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddForce(new GroundForce(engine));
            engine.AddForce(new AirResistanceForce(engine, .01));

            var ps = new YourParticleStructure();
            AddParticleStructure(ps, engine);

            var adapter = new EngineAdapter(engine)
            {
                ParticleSize = .1
            };

            var visualization = new KinematicsVisualization(adapter)
            {
                Ground = true,
                GroundSize = 10,
                GroundColor = Colors.SaddleBrown,
                ConnectorRadiusScale = .1
            };

            AddConnectorsToVisualizer(ps, visualization);

            Timeline.MaximumPoints = 3000;

            var fullViz = new MotionVisualizer3DControl(visualization);

            fullViz.Manager.Add3DGraph("Position", () => engine.Time, () => engine.Projectiles[0].Position, "Time (s)", "Position (m)");
            fullViz.Manager.Add3DGraph("Velocity", () => engine.Time, () => engine.Projectiles[0].Velocity, "Time (s)", "Velocity (m/s)");
            fullViz.Manager.Add3DGraph("Acceleration", () => engine.Time, () => engine.Projectiles[0].Acceleration, "Time (s)", "Acceleration (m/s^2)");

            fullViz.Show();
        }

        static private void AddParticleStructure(ParticleStructure ps, KinematicsEngine engine)
        {
            // Add projectiles
            foreach (var projectile in ps.Projectiles)
            {
                engine.AddProjectile(projectile);
            }

            // Add connectors
            foreach (var connector in ps.Connectors)
            {
                // Remember to connect it both ways
                engine.AddForce(new ProjectileBoundSpringForce(connector.Projectile1, connector.Projectile2, connector.SpringConstant, connector.UnstretchedLength));
                engine.AddForce(new ProjectileBoundSpringForce(connector.Projectile2, connector.Projectile1, connector.SpringConstant, connector.UnstretchedLength));
            }
        }

        static private void AddConnectorsToVisualizer(ParticleStructure ps, KinematicsVisualization viz)
        {
            foreach (var connector in ps.Connectors)
            {
                var indices = ps.GetIndexOfProjectiles(connector);
                viz.AddTwoParticleConnector(indices.Item1, indices.Item2, ConnectorColor);
            }
        }

    }
}
