using System;
using System.Collections.Generic;

namespace NaturalSelection
{
    public class GeneDictionary
    {
        private readonly Dictionary<Type, IList<GeneInfo>> dictionary = [];

        public void AddGene(EcologyAnimal ani, IList<GeneInfo> info)
        {
            var type = ani.GetType();
            if (!dictionary.TryAdd(type, info))
            {
                throw new ArgumentException("Animal type already exists in dictionary");
            }
        }

        public GeneInfo GetGene(EcologyAnimal animal, string name)
        {
            var type = animal.GetType();
            foreach (GeneInfo info in dictionary[type])
            {
                if (info.Name == name)
                {
                    return info;
                }
            }

            throw new KeyNotFoundException();
        }

        public GeneInfo GetGene(EcologyAnimal animal, int index)
        {
            return dictionary[animal.GetType()][index];
        }

        public IList<GeneInfo> GetGeneList(EcologyAnimal ani)
        {
            return dictionary[ani.GetType()];
        }

        public bool HasAnimal(EcologyAnimal ani)
        {
            return dictionary.ContainsKey(ani.GetType());
        }

        public int GetIndex(EcologyAnimal ani, string gene)
        {
            var type = ani.GetType();
            var list = dictionary[type];
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Name == gene)
                {
                    return i;
                }
            }

            throw new KeyNotFoundException();
        }

        public Dictionary<Type, IList<GeneInfo>> Dictionary { get { return dictionary; } }
    }
}
