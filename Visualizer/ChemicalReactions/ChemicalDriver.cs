using GraphControl;
using GraphData;
using MotionVisualizer3D;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using Thermodynamics;
using static GraphData.GraphDataManager;
using static WPFUtility.UtilityFunctions;

namespace Visualizer.ChemicalReactions
{
    class ChemicalReactionsDriver
    {
        static internal void Run()
        {
            const double containerSize = 25;

            const double deltaTime = .001;
            const double temperature = 293.17;
            const double reactionRadius = 2;

            //init variables
            string[] equations = 
            [
                "NH3+HCl->NH4Cl"
            ];

            (string name, double mass, System.Drawing.Color color)[] molecules = 
            [
                ("Molecule", 1e-26, System.Drawing.Color.NavajoWhite),
                ("NH3", 2.83e-23, System.Drawing.Color.LightPink),
                ("HCl", 6.05e-23, System.Drawing.Color.Crimson),
                ("NH4Cl", 8.88e-23, System.Drawing.Color.IndianRed)
            ];

            var container = new ReactingParticleContainer(containerSize, reactionRadius, 0);

            const double mass = 1e-26;

            container.RegisterParticleType("Molecule", mass, ConvertColor(Colors.NavajoWhite));

            var generator = new BoltzmannGenerator(container, temperature, container.Dictionary.Map["Molecule"]);

            const int nParticles = 1000;
            container.AddRandomParticles(generator, "Molecule", nParticles);


            var visualization = new ChemicalVisualization(container)
            {
                BoxColor = Colors.IndianRed
            };

            var viz = new MotionVisualizer3DControl(visualization)
            {
                TimeIncrement = deltaTime,
                TimeScale = 1,
                SlowDraw = false
            };

            Timeline.MaximumPoints = 3000;

            AddChemicalGraphs(viz, container, visualization);
            viz.Manager.AddHist(50, ConvertColor(Colors.BlueViolet), () => container.GetParticlePropertyList((Molecule part) => part.Velocity.Magnitude), "Speed (m/s)");
            viz.Manager.AddText("Time elapsed (s)", ConvertColor(Colors.Crimson), () => TimeElapsed().ToString());
            visualization.StopTime = 1;

            viz.Show();
        } 

        static private void AddChemicalGraphs(MotionVisualizer3DControl viz, ParticleContainer container,
            ChemicalVisualization visualization)
        {
            var timelineInfo = new List<TimelineInfo>();
            foreach (var info in container.Dictionary.Map.Values)
            {
                var prototype = new TimelinePrototype(info.Name, info.Color);
                timelineInfo.Add(new TimelineInfo(prototype,
                    new BasicFunctionPair(() => visualization.Time, () => container.GetNParticles(info.Name))));
            }

            viz.Manager.AddGraph(timelineInfo, "Time (s)", "Number of particles");
        }

        private static readonly Stopwatch watch = new();
        static private double TimeElapsed()
        {
            if (!watch.IsRunning)
            {
                watch.Start();
            }
            return watch.ElapsedMilliseconds / 1000.0;
        }
    }
}
