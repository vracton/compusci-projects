using MotionVisualizer3D;
using PhysicsUtility.Kinematics.Forces;
using PhysicsUtility.Kinematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Visualizer.Kinematics;
using DongUtility;
using GraphControl;

namespace Visualizer.FiniteElement.AnswerKey
{
    static internal class FiniteElementDriverAnswer
    {
        static private Color ConnectorColor = Colors.Green;
        private const double springDampingCoefficient = 1;

        static internal void RunFiniteElement()
        {
            var ps = new Cube();

            //var engine = new FiniteElementEngine(ps, springDampingCoefficient);
            var engine = new AdaptiveEngine();
            engine.Tolerance = 1;
            engine.AddForce(new ConstantGravitationForce(engine, new Vector(0, 0, -9.8)));
            engine.AddForce(new GroundForce(engine));
            engine.AddForce(new AirResistanceForce(engine, .01));
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

            fullViz.Manager.Add3DGraph("CM Position", () => engine.Time, () => engine.CMPosition, "Time (s)", "Position (m)");
            fullViz.Manager.Add3DGraph("CM Velocity", () => engine.Time, () => engine.CMVelocity, "Time (s)", "Velocity (m/s)");
            fullViz.Manager.Add3DGraph("CM Acceleration", () => engine.Time, () => engine.CMAcceleration, "Time (s)", "Acceleration (m/s^2)");
            fullViz.Manager.AddSingleGraph("Number of divisions", WPFUtility.UtilityFunctions.ConvertColor(Colors.Peru), () => engine.Time, () => engine.NDivisions, "Time (s)", "Number of divisions");

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
