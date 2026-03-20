using GraphControl;
using GraphData;
using MotionVisualizer3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Windows.Media;
using Thermodynamics;
using VisualizerControl.Shapes;
using static GraphData.GraphDataManager;
using static System.Net.WebRequestMethods;
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

            //level 1,2,3
            //ParticleInfo[] molecules =
            //[
            //    new ParticleInfo("Molecule", 1e-26, ConvertColor(Colors.NavajoWhite)),
            //    new ParticleInfo("NH3", 2.83e-23, ConvertColor(Colors.LightPink)),
            //    new ParticleInfo("HCl", 6.05e-23, ConvertColor(Colors.Crimson)),
            //    new ParticleInfo("NH4Cl", 8.88e-23, ConvertColor(Colors.MediumVioletRed))
            //];

            //(string, double)[] equations =
            //[
            //    ("NH3+HCl->NH4Cl", 2.9e-19)
            //];

            //level 4
            ParticleInfo[] molecules =
            [
                new ParticleInfo("Acetyl_CoA", 1.34e-21, ConvertColor(Colors.Goldenrod)),
                new ParticleInfo("Oxaloacetate", 2.19e-22, ConvertColor(Colors.SteelBlue)),
                new ParticleInfo("H2O", 2.99e-23, ConvertColor(Colors.DeepSkyBlue)),
                new ParticleInfo("Citrate", 3.19e-22, ConvertColor(Colors.LimeGreen)),
                new ParticleInfo("CoA_SH", 1.27e-21, ConvertColor(Colors.SaddleBrown)),
                new ParticleInfo("cis_Aconitate", 2.89e-22, ConvertColor(Colors.DarkOrange)),
                new ParticleInfo("Isocitrate", 3.19e-22, ConvertColor(Colors.MediumSeaGreen)),
                new ParticleInfo("NAD", 1.10e-21, ConvertColor(Colors.MediumPurple)),
                new ParticleInfo("alpha_Ketoglutarate", 2.43e-22, ConvertColor(Colors.Coral)),
                new ParticleInfo("CO2", 7.31e-23, ConvertColor(Colors.DimGray)),
                new ParticleInfo("NADH", 1.10e-21, ConvertColor(Colors.Indigo)),
                new ParticleInfo("H", 1.67e-24, ConvertColor(Colors.HotPink)),
                new ParticleInfo("Succinyl_CoA", 1.44e-21, ConvertColor(Colors.Firebrick)),
                new ParticleInfo("GDP", 7.36e-22, ConvertColor(Colors.Khaki)),
                new ParticleInfo("Pi", 1.63e-22, ConvertColor(Colors.Turquoise)),
                new ParticleInfo("Succinate", 1.96e-22, ConvertColor(Colors.ForestGreen)),
                new ParticleInfo("GTP", 8.69e-22, ConvertColor(Colors.Gold)),
                new ParticleInfo("FAD", 1.30e-21, ConvertColor(Colors.MediumOrchid)),
                new ParticleInfo("Fumarate", 1.93e-22, ConvertColor(Colors.OrangeRed)),
                new ParticleInfo("FADH2", 1.31e-21, ConvertColor(Colors.DarkViolet)),
                new ParticleInfo("Malate", 2.23e-22, ConvertColor(Colors.Teal)),
            ];

            //could not find reliable enthalpies for all reactions, so set to 0
            (string, double)[] equations =
            [
                ("Oxaloacetate+Acetyl_CoA+H2O->Citrate+CoA_SH", 0.0),
                ("Citrate->cis_Aconitate+H2O", 0.0),
                ("cis_Aconitate+H2O->Isocitrate", 0.0),
                ("Isocitrate+NAD->alpha_Ketoglutarate+CO2+NADH+H", 0.0),
                ("alpha_Ketoglutarate+CoA_SH+NAD->Succinyl_CoA+CO2+NADH+H", 0.0),
                ("Succinyl_CoA+GDP+Pi->Succinate+CoA_SH+GTP", 0.0),
                ("Succinate+FAD->Fumarate+FADH2", 0.0),
                ("Fumarate+H2O->Malate", 0.0),
                ("Malate+NAD->Oxaloacetate+NADH+H", 0.0)
            ];

            var container = new ReactingParticleContainer(molecules, equations, containerSize, reactionRadius, 3);

            //level 1
            //var generator = new BoltzmannGenerator(container, temperature, container.Dictionary.Map["Molecule"]);

            //level 1,2,3
            //const int nParticles = 1000;

            //level 1
            //container.AddRandomParticles(generator, "Molecule", nParticles);

            //level 2 & 3
            //container.AddRandomParticles(generator, "NH3", nParticles / 2,
            //    new DongUtility.Range(0, containerSize / 2), new DongUtility.Range(0, containerSize), new DongUtility.Range(0, containerSize));
            //container.AddRandomParticles(generator, "HCl", nParticles / 2,
            //    new DongUtility.Range(containerSize / 2, containerSize), new DongUtility.Range(0, containerSize), new DongUtility.Range(0, containerSize));

            //level 4
            var generator = new BoltzmannGenerator(container, temperature, container.Dictionary.Map["H2O"]);

            container.AddRandomParticles(generator, "Acetyl_CoA", 120);
            container.AddRandomParticles(generator, "Oxaloacetate", 30);
            container.AddRandomParticles(generator, "H2O", 250);
            container.AddRandomParticles(generator, "NAD", 180);
            container.AddRandomParticles(generator, "FAD", 40);
            container.AddRandomParticles(generator, "GDP", 40);
            container.AddRandomParticles(generator, "Pi", 80);
            container.AddRandomParticles(generator, "CoA_SH", 10);
            container.AddRandomParticles(generator, "Citrate", 6);
            container.AddRandomParticles(generator, "cis_Aconitate", 4);
            container.AddRandomParticles(generator, "Isocitrate", 6);
            container.AddRandomParticles(generator, "alpha_Ketoglutarate", 6);
            container.AddRandomParticles(generator, "Succinyl_CoA", 4);
            container.AddRandomParticles(generator, "Succinate", 6);
            container.AddRandomParticles(generator, "Fumarate", 6);
            container.AddRandomParticles(generator, "Malate", 6);


            var visualization = new ChemicalVisualization(container)
            {
                BoxColor = Colors.IndianRed,

                //level 3
                //StopCondition = (Func<bool>?)(() => container.GetNParticles("NH3") < 10)
            };
            
            var viz = new MotionVisualizer3DControl(visualization)
            {
                TimeIncrement = deltaTime,
                TimeScale = 1,
                SlowDraw = false
            };

            Timeline.MaximumPoints = 15000;

            //level 1,2,3
            //AddChemicalGraphs(viz, container, visualization);

            //level 2
            //viz.Manager.AddHist(50, ConvertColor(Colors.BlueViolet), () => container.GetParticlePropertyList((Molecule part) => part.Velocity.Magnitude), "Speed (m/s)");
            
            //level 1,2,3
            //viz.Manager.AddSingleGraph("Temperature", ConvertColor(Colors.CornflowerBlue), () => visualization.Time, () => container.Temperature, "Time (s)", "Temperature (K)");

            //level 4
            AddLevel4Graphs(viz, container, visualization);

            //level 3
            //bool hasFailed = false;
            //viz.Manager.AddText("Molecule Count Checker", ConvertColor(Colors.Plum), () =>
            //{
            //    int nNH3 = container.GetNParticles("NH3");
            //    int nHCl = container.GetNParticles("HCl");
            //    int nNH4Cl = container.GetNParticles("NH4Cl");

            //    if (nNH3 != nHCl || nNH3 + nHCl + 2 * nNH4Cl != nParticles)
            //    {
            //        hasFailed = true;
            //    }

            //    return hasFailed ? "count mismatch" : "count as expected";
            //});

            viz.Manager.AddText("Time elapsed (s)", ConvertColor(Colors.Crimson), () => TimeElapsed().ToString());

            //level 1
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

        static private void AddLevel4Graphs(MotionVisualizer3DControl viz, ParticleContainer container,
            ChemicalVisualization visualization)
        {
            AddMoleculeGraph(viz, container, visualization, "Krebs Intermediates",
            [
                "Oxaloacetate",
                "Citrate",
                "cis_Aconitate",
                "Isocitrate",
                "alpha_Ketoglutarate",
                "Succinyl_CoA",
                "Succinate",
                "Fumarate",
                "Malate",
                "Acetyl_CoA",
            ]);

            AddMoleculeGraph(viz, container, visualization, "Krebs Cofactors",
            [
                "CoA_SH",
                "H2O",
                "NAD",
                "FAD",
                "FADH2",
                "GDP",
                "Pi"
            ]);

            AddMoleculeGraph(viz, container, visualization, "Krebs Byproducts",
            [
                "CO2",
                "H",
                "GTP",
                "NADH",
            ]);
        }

        static private void AddMoleculeGraph(MotionVisualizer3DControl viz, ParticleContainer container,
            ChemicalVisualization visualization, string title, string[] moleculeNames)
        {
            var timelineInfo = new List<TimelineInfo>();

            foreach (string moleculeName in moleculeNames)
            {
                var info = container.Dictionary.Map[moleculeName];
                var prototype = new TimelinePrototype(info.Name, info.Color);
                timelineInfo.Add(new TimelineInfo(prototype,
                    new BasicFunctionPair(() => visualization.Time, () => container.GetNParticles(info.Name))));
            }

            viz.Manager.AddGraph(timelineInfo, "Time (s)", title);
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
