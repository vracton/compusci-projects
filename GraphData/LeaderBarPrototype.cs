using System.Drawing;

namespace GraphData
{
    /// <summary>
    /// The basic information about a leader bar that is needed to make a leaderboard prototype.  Contains information but does not draw anything
    /// </summary>
    public class LeaderBarPrototype(string name, Color color)
    {
        public string Name { get; } = name;
        public Color Color { get; } = color;
    }
}
