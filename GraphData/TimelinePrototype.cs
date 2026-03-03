using DongUtility;
using System.Drawing;

namespace GraphData
{
    /// <summary>
    /// A prototype for a single timeline (with no data or axes).  Used to create the prototype for graphs
    /// </summary>
    public class TimelinePrototype
    {
        public string Name { get; }
        public Color Color { get; }

        public TimelinePrototype(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Color);
        }

        internal TimelinePrototype(BinaryReader br)
        {
            Name = br.ReadString();
            Color = br.ReadColor();
        }
    }
}
