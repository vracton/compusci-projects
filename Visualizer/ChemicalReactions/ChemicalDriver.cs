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
                    ConcentrationM: 0.144,
                    Species: [new SpeciesContribution("HSO_3^-", 1)]),
                new SolutionInput(
                    "Starch",
                    VolumeMl: 10,
                    ConcentrationM: 5.0 / 342.3,
                    Species: [new SpeciesContribution("starch", 1)]),
                new SolutionInput(
                    "HgCl2",
                    VolumeMl: 10,
                    ConcentrationM: 0.011,
                    Species: [new SpeciesContribution("Hg^2p", 1)]),
                new SolutionInput(
                    "KIO3",
                    VolumeMl: 20,
                    ConcentrationM: 0.0701,
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

        private sealed record SpeciesContribution(string SpeciesName, double StoichiometricFactor);

        private sealed record SolutionInput(string Name, double VolumeMl, double ConcentrationM,
            IReadOnlyList<SpeciesContribution> Species);

        private sealed record SimulationRecipe(IReadOnlyList<SolutionInput> Solutions,
            int ParticlesPerMolar, int MinimumParticleCount);
    }
}
