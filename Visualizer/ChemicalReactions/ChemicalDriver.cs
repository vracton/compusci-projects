using System;
using GraphControl;
using GraphData;
using MotionVisualizer3D;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using Thermodynamics;
using VisualizerControl.Shapes;
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

            ParticleInfo[] molecules =
            [
                new ParticleInfo("Molecule", 1e-26, ConvertColor(Colors.NavajoWhite)),
                new ParticleInfo("NH3", 2.83e-23, ConvertColor(Colors.LightPink)),
                new ParticleInfo("HCl", 6.05e-23, ConvertColor(Colors.Crimson)),
                new ParticleInfo("NH4Cl", 8.88e-23, ConvertColor(Colors.MediumVioletRed))
            ];

            (string, double)[] equations =
            [
                ("NH3+HCl->NH4Cl", 2.9e-19)
            ];

            var container = new ReactingParticleContainer(molecules, equations, containerSize, reactionRadius, 0);

            var generator = new BoltzmannGenerator(container, temperature, container.Dictionary.Map["Molecule"]);

            //level 1
            //const int nParticles = 1000;
            //container.AddRandomParticles(generator, "Molecule", nParticles);

            //level 2 & 3
            container.AddRandomParticles(generator, "NH3", 500,
                new DongUtility.Range(0, containerSize / 2), new DongUtility.Range(0, containerSize), new DongUtility.Range(0, containerSize));
            container.AddRandomParticles(generator, "HCl", 500,
                new DongUtility.Range(containerSize / 2, containerSize), new DongUtility.Range(0, containerSize), new DongUtility.Range(0, containerSize));


            var visualization = new ChemicalVisualization(container)
            {
                BoxColor = Colors.IndianRed,
                StopCondition = (Func<bool>?)(() => container.GetNParticles("NH3") < 5)
            };
            
            var viz = new MotionVisualizer3DControl(visualization)
            {
                TimeIncrement = deltaTime,
                TimeScale = 1,
                SlowDraw = false
            };

            Timeline.MaximumPoints = 3000;

            AddChemicalGraphs(viz, container, visualization);
            //viz.Manager.AddHist(50, ConvertColor(Colors.BlueViolet), () => container.GetParticlePropertyList((Molecule part) => part.Velocity.Magnitude), "Speed (m/s)");
            viz.Manager.AddSingleGraph("Temperature", ConvertColor(Colors.CornflowerBlue), () => visualization.Time, () => container.Temperature, "Time (s)", "Temperature (K)");
            viz.Manager.AddText("Time elapsed (s)", ConvertColor(Colors.Crimson), () => TimeElapsed().ToString());
            //visualization.StopTime = 1;
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
