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
        private const double InitialHydrogenFloorM = 1.0e-7;
        private const double Kw = 1.0e-14;
        private const double Ka1H2So3 = 1.54881661891248e-2;
        private const double Ka2Hso3 = 6.30957344480193e-8;

        static internal void Run()
        {
            const double containerSize = 25;
            const double deltaTime = .001;
            const double k1 = 650.0;
            const double k2 = 9.2e7;
            const double k3 = 30000.0;
            const double chemistryRandomness = 0.03;

            ParticleInfo[] molecules =
            [
                new ParticleInfo("IO_3^-", 174.903 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("HSO_3^-", 81.07 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("H^p", 1.008 / 6.022e23, ConvertColor(Colors.LightGoldenrodYellow)),
                new ParticleInfo("Hg^2p", 200.59 / 6.022e23, ConvertColor(Colors.AntiqueWhite)),
                new ParticleInfo("I^-", 126.9 / 6.022e23, ConvertColor(Colors.Goldenrod)),
                new ParticleInfo("HgI_2", 454.4 / 6.022e23, ConvertColor(Colors.Coral)),
                new ParticleInfo("I_2", 253.81 / 6.022e23, ConvertColor(Colors.MediumBlue)),
                new ParticleInfo("H_2O", 18.015 / 6.022e23, ConvertColor(Colors.Transparent)),
            ];

            var recipe = CreateDefaultRecipe();
            var initialState = BuildInitialBulkState(recipe);
            var container = new OldNassauParticleContainer(
                molecules,
                containerSize,
                initialState,
                [
                    "IO_3^-",
                    "HSO_3^-",
                    "H^p",
                    "Hg^2p",
                    "I^-",
                    "HgI_2",
                    "I_2"
                ],
                tracerBudget: 1200,
                baseK1: k1,
                baseK2: k2,
                baseK3: k3,
                chemistryRandomness: chemistryRandomness);

            double? firstHgI2VisibleTime = null;
            double? firstI2VisibleTime = null;

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
            viz.Manager.AddText("I_2 molarity (M)", ConvertColor(Colors.MediumBlue),
                () => CalculateSpeciesMolarity(container, recipe, "I_2").ToString("G6"));
            viz.Manager.AddText("First time [HgI_2] >= 1e-3 (s)", ConvertColor(Colors.Coral),
                () => FormatFirstThresholdTime(TrackFirstThresholdCrossing(container, recipe, visualization,
                    "HgI_2", 1e-3, ref firstHgI2VisibleTime)));
            viz.Manager.AddText("First time [I_2] >= 1e-5 (s)", ConvertColor(Colors.MediumBlue),
                () => FormatFirstThresholdTime(TrackFirstThresholdCrossing(container, recipe, visualization,
                    "I_2", 1e-5, ref firstI2VisibleTime)));

            viz.Show();
        }

        static private void AddChemicalGraphs(MotionVisualizer3DControl viz, OldNassauParticleContainer container,
            ChemicalVisualization visualization)
        {
            var timelineInfo = new List<TimelineInfo>();
            foreach (string speciesName in container.VisibleSpecies)
            {
                var info = container.Dictionary.Map[speciesName];
                var prototype = new TimelinePrototype(info.Name, info.Color);
                timelineInfo.Add(new TimelineInfo(prototype,
                    new BasicFunctionPair(() => visualization.Time, () => container.GetSpeciesMolarity(info.Name))));
            }

            viz.Manager.AddGraph(timelineInfo, "Time (s)", "Molarity (M)");
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
                    GramsPerLiter: 15.0,
                    MolarMassGPerMol: 104.06,
                    Species: [new SpeciesContribution("HSO_3^-", 1)]),
                new SolutionInput(
                    "Starch",
                    VolumeMl: 10,
                    GramsPerLiter: 5.0,
                    MolarMassGPerMol: 342.3,
                    Species: []),
                new SolutionInput(
                    "HgCl2",
                    VolumeMl: 10,
                    GramsPerLiter: 4.6,
                    MolarMassGPerMol: 271.49,
                    Species: [new SpeciesContribution("Hg^2p", 1)]),
                new SolutionInput(
                    "KIO3",
                    VolumeMl: 20,
                    GramsPerLiter: 11.53,
                    MolarMassGPerMol: 214.0,
                    Species: [new SpeciesContribution("IO_3^-", 1)]),
            };

            return new SimulationRecipe(
                solutions,
                ParticlesPerMolar: 250000,
                MinimumParticleCount: 1);
        }

        private static Dictionary<string, double> BuildInitialBulkState(SimulationRecipe recipe)
        {
            double totalVolumeMl = recipe.Solutions.Sum(x => x.VolumeMl);
            if (totalVolumeMl <= 0)
            {
                throw new ArgumentException("Total solution volume must be positive.");
            }

            var state = new Dictionary<string, double>
            {
                ["IO_3^-"] = 0.0,
                ["HSO_3^-"] = 0.0,
                ["I^-"] = 0.0,
                ["H^p"] = 0.0,
                ["I_2"] = 0.0,
                ["Hg^2p"] = 0.0,
                ["HgI_2"] = 0.0
            };

            foreach (var solution in recipe.Solutions)
            {
                double dilutionFactor = solution.VolumeMl / totalVolumeMl;
                foreach (var contribution in solution.Species)
                {
                    state[contribution.SpeciesName] += solution.ConcentrationM * dilutionFactor * contribution.StoichiometricFactor;
                }
            }

            state["H^p"] = EstimateInitialHydrogenFromBisulfite(state["HSO_3^-"]);
            return state;
        }

        private static double EstimateInitialHydrogenFromBisulfite(double totalBisulfiteMolarity)
        {
            if (totalBisulfiteMolarity <= 0)
            {
                return InitialHydrogenFloorM;
            }

            double sodium = totalBisulfiteMolarity;

            double ChargeBalance(double hydrogen)
            {
                double denom = hydrogen * hydrogen + Ka1H2So3 * hydrogen + Ka1H2So3 * Ka2Hso3;
                double hso3 = totalBisulfiteMolarity * Ka1H2So3 * hydrogen / denom;
                double so3 = totalBisulfiteMolarity * Ka1H2So3 * Ka2Hso3 / denom;
                double hydroxide = Kw / hydrogen;
                return (sodium + hydrogen) - (hso3 + 2.0 * so3 + hydroxide);
            }

            double low = 1.0e-9;
            double high = 1.0;
            double fLow = ChargeBalance(low);
            double fHigh = ChargeBalance(high);

            for (int i = 0; i < 20 && fLow * fHigh > 0; ++i)
            {
                low /= 10.0;
                high *= 10.0;
                fLow = ChargeBalance(low);
                fHigh = ChargeBalance(high);
            }

            for (int i = 0; i < 200; ++i)
            {
                double mid = 0.5 * (low + high);
                double fMid = ChargeBalance(mid);
                if (fLow * fMid <= 0)
                {
                    high = mid;
                    fHigh = fMid;
                }
                else
                {
                    low = mid;
                    fLow = fMid;
                }
            }

            return Math.Max(InitialHydrogenFloorM, 0.5 * (low + high));
        }

        private static double CalculateSpeciesMolarity(OldNassauParticleContainer container, SimulationRecipe recipe,
            string speciesName)
        {
            _ = recipe;
            return container.GetSpeciesMolarity(speciesName);
        }

        private static double? TrackFirstThresholdCrossing(OldNassauParticleContainer container, SimulationRecipe recipe,
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
