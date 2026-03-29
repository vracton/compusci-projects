using DongUtility;
using GraphControl;
using GraphData;
using MotionVisualizer3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            const double reactionRadius = 5;

            ParticleInfo[] molecules =
            [
                new ParticleInfo("IO_3^-", 174.903 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("HSO_3^-", 81.07 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("SO_4^2-", 96.06 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("H^p", 1.008 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("Hg^2p", 200.59 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("I^-", 126.9 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("HgI_2", 454.4 / 6.022e23, ConvertColor(Colors.Coral)),
                new ParticleInfo("I_2", 253.81 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("H_2O", 18.015 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("starch", 342.3 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("I_2-starch", (253.81 + 342.3) / 6.022e23, ConvertColor(Colors.Blue)),
            ];

            (string, double)[] equations =
            [
                ("IO_3^-+3HSO_3^-->I^-+3SO_4^2-+3H^p", 0),
                ("Hg^2p+2I^-->HgI_2", 0),
                ("6H^p+IO_3^-+5I^-->3I_2+3H_2O", 0),
                ("I_2+starch->I_2-starch", 0)
            ];

            var container = new ReactingParticleContainer(molecules, equations, containerSize, reactionRadius, 5);

            var generator = new BoltzmannGenerator(container, temperature, container.Dictionary.Map["H_2O"]);

            var recipe = CreateDefaultRecipe();
            SeedReactantsInLayers(container, generator, recipe);
            double? firstHgI2VisibleTime = null;
            double? firstI2StarchVisibleTime = null;

            var visualization = new ChemicalVisualization(container)
            {
                BoxColor = Colors.IndianRed,
            };

            var viz = new MotionVisualizer3DControl(visualization)
            {
                TimeIncrement = deltaTime,
                TimeScale = 1,
                SlowDraw = false
            };

            Timeline.MaximumPoints = 15000;
            AddChemicalGraphs(viz, container, visualization);
            viz.Manager.AddSingleGraph("Temperature", ConvertColor(Colors.CornflowerBlue), () => visualization.Time, () => container.Temperature, "Time (s)", "Temperature (K)");
            viz.Manager.AddText("Time elapsed (s)", ConvertColor(Colors.Crimson), () => TimeElapsed().ToString());
            viz.Manager.AddText("HgI_2 molarity (M)", ConvertColor(Colors.Coral),
                () => CalculateSpeciesMolarity(container, recipe, "HgI_2").ToString("G6"));
            viz.Manager.AddText("I_2-starch molarity (M)", ConvertColor(Colors.Blue),
                () => CalculateSpeciesMolarity(container, recipe, "I_2-starch").ToString("G6"));
            viz.Manager.AddText("First time [HgI_2] >= 1e-3 (s)", ConvertColor(Colors.Coral),
                () => FormatFirstThresholdTime(TrackFirstThresholdCrossing(container, recipe, visualization,
                    "HgI_2", 1e-3, ref firstHgI2VisibleTime)));
            viz.Manager.AddText("First time [I_2-starch] >= 1e-5 (s)", ConvertColor(Colors.Blue),
                () => FormatFirstThresholdTime(TrackFirstThresholdCrossing(container, recipe, visualization,
                    "I_2-starch", 1e-5, ref firstI2StarchVisibleTime)));

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

        private static SimulationRecipe CreateDefaultRecipe()
        {
            var solutions = new[]
            {
                new SolutionInput(
                    "NaHSO3",
                    VolumeMl: 10,
                    GramsPerLiter: 15.0 ,
                    MolarMassGPerMol: 104.06,
                    Species: [new SpeciesContribution("HSO_3^-", 1)]),
                new SolutionInput(
                    "Starch",
                    VolumeMl: 10,
                    GramsPerLiter: 5.0,
                    MolarMassGPerMol: 342.3,
                    Species: [new SpeciesContribution("starch", 1)]),
                new SolutionInput(
                    "HgCl2",
                    VolumeMl: 10,
                    GramsPerLiter: 3.0,
                    MolarMassGPerMol: 271.49,
                    Species: [new SpeciesContribution("Hg^2p", 1)]),
                new SolutionInput(
                    "KIO3",
                    VolumeMl: 20,
                    GramsPerLiter: 15.0,
                    MolarMassGPerMol: 214.0,
                    Species: [new SpeciesContribution("IO_3^-", 1)]),
            };

            return new SimulationRecipe(
                solutions,
                ParticlesPerMolar: 16000,
                MinimumParticleCount: 1);
        }

        private static void SeedReactantsInLayers(ReactingParticleContainer container, RandomGenerator generator,
            SimulationRecipe recipe)
        {
            double totalVolumeMl = recipe.Solutions.Sum(x => x.VolumeMl);
            if (totalVolumeMl <= 0)
            {
                throw new ArgumentException("Total solution volume must be positive.");
            }

            var xRange = new DongUtility.Range(0, container.Size.X);
            var yRange = new DongUtility.Range(0, container.Size.Y);
            double layerStart = 0;

            foreach (var solution in recipe.Solutions)
            {
                if (solution.VolumeMl <= 0)
                {
                    continue;
                }

                double layerFraction = solution.VolumeMl / totalVolumeMl;
                double layerEnd = layerStart + layerFraction * container.Size.Z;
                var zRange = new DongUtility.Range(layerStart, layerEnd);

                foreach (var contribution in solution.Species)
                {
                    int count = CalculateLayerParticleCount(solution, contribution, layerFraction, recipe);
                    if (count > 0)
                    {
                        container.AddRandomParticles(generator, contribution.SpeciesName, count, xRange, yRange, zRange);
                    }
                }

                layerStart = layerEnd;
            }
        }

        private static int CalculateLayerParticleCount(SolutionInput solution, SpeciesContribution contribution,
            double layerFraction, SimulationRecipe recipe)
        {
            double scaledConcentration = solution.ConcentrationM * layerFraction * contribution.StoichiometricFactor;
            int count = (int)Math.Round(scaledConcentration * recipe.ParticlesPerMolar);

            if (count < recipe.MinimumParticleCount && scaledConcentration > 0)
            {
                count = recipe.MinimumParticleCount;
            }

            return count;
        }

        private static double CalculateSpeciesMolarity(ParticleContainer container, SimulationRecipe recipe,
            string speciesName)
        {
            return container.GetNParticles(speciesName) / (double)recipe.ParticlesPerMolar;
        }

        private static double? TrackFirstThresholdCrossing(ParticleContainer container, SimulationRecipe recipe,
            ChemicalVisualization visualization, string speciesName, double thresholdM, ref double? firstTime)
        {
            if (firstTime is null && CalculateSpeciesMolarity(container, recipe, speciesName) >= thresholdM)
            {
                firstTime = visualization.Time;
            }

            return firstTime;
        }

        private static string FormatFirstThresholdTime(double? time)
        {
            return time?.ToString("G6") ?? "Not reached";
        }

        private sealed record SpeciesContribution(string SpeciesName, double StoichiometricFactor);

        private sealed record SolutionInput(string Name, double VolumeMl, double GramsPerLiter,
            double MolarMassGPerMol, IReadOnlyList<SpeciesContribution> Species)
        {
            public double ConcentrationM => GramsPerLiter / MolarMassGPerMol;
        }

        private sealed record SimulationRecipe(IReadOnlyList<SolutionInput> Solutions,
            int ParticlesPerMolar, int MinimumParticleCount);
    }
}
