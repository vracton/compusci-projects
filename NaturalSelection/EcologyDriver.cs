using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GraphData;
using GraphControl;
using static GraphData.GraphDataManager;
using static WPFUtility.UtilityFunctions;

namespace NaturalSelection
{
    static class EcologyDriver
    {
        private const int nHares = 100;
        private const int nLynx = 30;

        static internal void RunEcology()
        {
            var arena = new EcologyArena(30, 30, (x, _) => (x>15 ? 5 : 15))
            {
                MaxTime = 1825 //5 years
            };
            arena.AddAnimals<Hare>(nHares);
            arena.AddAnimals<Lynx>(nLynx);

            var sim = new EcologySim(arena)
            {
                TimePerTurn = .01
            };
            Timeline.MaximumPoints = 1800;

            sim.Arena.Manager.AddGraph(
            [ new(new TimelinePrototype("Hare", Color.SandyBrown), new BasicFunctionPair(() => arena.Time, () => arena.GetObjectsOfType<Hare>().Count())),
            new(new TimelinePrototype("Lynx", Color.PaleVioletRed), new BasicFunctionPair(() => arena.Time, () => arena.GetObjectsOfType<Lynx>().Count()))
            ], "Time (days)", "Population");

            foreach (var typePair in EcologyAnimal.GeneDictionary.Dictionary)
            {
                foreach (var gene in typePair.Value)
                {
                    int nBins = gene.Name == "Male" ? 2 : 10;
                    if (typePair.Key == typeof(Hare) && gene.Name == "Dark coat")
                    {
                        //sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key, true), "Male dark coat");
                        //sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key, false), "Female dark coat");
                        sim.Arena.Manager.AddText("Mean hare dark coat", ConvertColor(gene.Color), () => MeanGene(arena, gene.Name, typePair.Key).ToString("F2"));
                        sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key, null, false), "Dark coat x <= 15");
                        sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key, null, true), "Dark coat x > 15");
                    }
                    else
                    {
                        sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key), gene.Name);
                    }
                }
            }

            sim.Show();
        }

        static private List<double> ListGene(EcologyArena arena, string gene, Type type, bool? isMale = null, bool? isRightSide = null)
        {
            var response = new List<double>();
            foreach (var animal in arena.GetObjectsOfType<EcologyAnimal>())
            {
                if (animal.GetType() == type && (isMale == null || animal.IsMale == isMale) && IsOnSide(animal, isRightSide))
                {
                    response.Add(animal.GetGene(gene));
                }
            }
            return response;
        }

        static private bool IsOnSide(EcologyAnimal animal, bool? isRightSide)
        {
            if (isRightSide == null)
            {
                return true;
            }

            return isRightSide.Value ? animal.Position.X > 15 : animal.Position.X <= 15;
        }

        static private double MeanGene(EcologyArena arena, string gene, Type type)
        {
            var values = ListGene(arena, gene, type);
            if (values.Count == 0)
            {
                return 0;
            }

            return values.Average();
        }
    }




}
