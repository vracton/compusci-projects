using Arena;
using System;
using System.Collections.Generic;
using DongUtility;

namespace NaturalSelection
{
    public class Genotype
    {
        private readonly List<double> fatherGenes;
        private readonly List<double> motherGenes;

        public Genotype(List<double> fatherGenes, List<double> motherGenes)
        {
            if (fatherGenes.Count != motherGenes.Count)
            {
                throw new ArgumentException("Father and mother genes must have the same length");
            }

            this.fatherGenes = fatherGenes;
            this.motherGenes = motherGenes;
        }

        public double GetGene(int index)
        {
            return fatherGenes[index] + motherGenes[index];
        }

        public double GetGene(string name, EcologyAnimal ani)
        {
            return GetGene(EcologyAnimal.GeneDictionary.GetIndex(ani, name));
        }

        public List<double> Meiosis(EcologyAnimal ani)
        {
            int size = fatherGenes.Count;
            List<double> finalList = new(size);

            for (int i = 0; i < size; ++i)
            {
                bool father = ArenaEngine.Random.NextBool();
                double gene = father ? fatherGenes[i] : motherGenes[i];
                
                finalList.Add(MutateGene(ani, i, gene));
            }

            return [.. finalList];
        }

        private static double MutateGene(EcologyAnimal ani, int geneIndex, double geneVal)
        {
            var info = EcologyAnimal.GeneDictionary.GetGene(ani, geneIndex);
            if (info.SD <= 0)
            {
                return geneVal;
            }
            return ArenaEngine.Random.NextGaussian(geneVal, info.SD);
        }

    }
}
