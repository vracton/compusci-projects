using DongUtility;
using System.Drawing;

namespace GraphData
{
    /// <summary>
    /// The prototype for updating text objects
    /// </summary>
    public class TextPrototype : IGraphPrototype
    {
        public string Title { get; }
        public Color Color { get; }

        public TextPrototype(string title, Color color)
        {
            Title = title;
            Color = color;
        }

        IGraphPrototype.GraphType IGraphPrototype.GetGraphType()
        {
            return IGraphPrototype.GraphType.Text;
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(Title);
            bw.Write(Color);
        }

        internal TextPrototype(BinaryReader br)
        {
            Title = br.ReadString();
            Color = br.ReadColor();
        }
    }
}
