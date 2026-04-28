using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GraphData;
using static GraphData.GraphDataManager;
using static WPFUtility.UtilityFunctions;

namespace NaturalSelection
{
    static class EcologyDriver
    {
        private const int nHares = 100;
        private const int nLynx = 40;

        static internal void RunEcology()
        {
            var arena = new EcologyArena(30, 30)
            {
                MaxTime = 365
            };
            arena.AddAnimals<Hare>(nHares);
            arena.AddAnimals<Lynx>(nLynx);

            var sim = new EcologySim(arena)
            {
                TimePerTurn = .1
            };

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
                        sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key, true), "Male dark coat");
                        sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key, false), "Female dark coat");
                    }
                    else
                    {
                        sim.Arena.Manager.AddHist(nBins, ConvertColor(gene.Color), () => ListGene(arena, gene.Name, typePair.Key), gene.Name);
                    }
                }
            }

            sim.Show();
        }

        static private List<double> ListGene(EcologyArena arena, string gene, Type type, bool? isMale = null)
        {
            var response = new List<double>();
            foreach (var animal in arena.GetObjectsOfType<EcologyAnimal>())
            {
                if (animal.GetType() == type && (isMale == null || animal.IsMale == isMale))
                {
                    response.Add(animal.GetGene(gene));
                }
            }
            return response;
        }
    }




}
