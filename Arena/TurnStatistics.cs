using GraphData;
using System.Drawing;

namespace Arena
{
    /// <summary>
    /// Stores information about each turn for statistical plots
    /// Wait, do we even use this?
    /// </summary>
    public class TurnStatistics
    {
        private Dictionary<string, HashSet<string>> longTermDictionary = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, Dictionary<string, double>> dictionary = new Dictionary<string, Dictionary<string, double>>();
        private Dictionary<string, Color> colorDictionary = new Dictionary<string, Color>();
        public bool HasChanged { get; private set; } = false;

        public void AddColorMapping(string objectName, Color color)
        {
            colorDictionary.Add(objectName, color);
        }

        public void AddScore(string statName, string objectName, double value)
        {
            if (!dictionary.ContainsKey(statName))
            {
                dictionary.Add(statName, new Dictionary<string, double>());
            }

            var objectDictionary = dictionary[statName];
            if (!objectDictionary.ContainsKey(objectName))
            {
                objectDictionary.Add(objectName, 0);
            }

            objectDictionary[objectName] += value;

            if (objectDictionary[objectName] != 0)
            {
                if (!longTermDictionary.ContainsKey(statName))
                {
                    longTermDictionary.Add(statName, new HashSet<string>());
                    HasChanged = true;
                }

                if (!longTermDictionary[statName].Contains(objectName))
                {
                    longTermDictionary[statName].Add(objectName);
                    HasChanged = true;
                }
            }
        }

        public void ClearStatistics()
        {
            dictionary.Clear();
        }

        public IEnumerable<GraphPrototype> GetGraphPrototype()
        {
            HasChanged = false;

            foreach (var entry in longTermDictionary)
            {
                var graphPrototype = new GraphPrototype("Time (s)", entry.Key);
                foreach (var objectEntry in entry.Value)
                {
                    var color = colorDictionary[objectEntry];
                    var timeline = new TimelinePrototype(objectEntry, color);
                    graphPrototype.AddTimeline(timeline);
                }
                yield return graphPrototype;
            }
        }

    }
}
