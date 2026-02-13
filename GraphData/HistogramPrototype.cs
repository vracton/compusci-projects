using DongUtility;
using System.Drawing;

namespace GraphData
{
    /// <summary>
    /// The information about a histogram that needs to be saved, separated from its graphical implementation
    /// Contains the information needed to make a histogram but does not draw it
    /// </summary>
    public class HistogramPrototype : IGraphPrototype
    {
        /// <summary>
        /// The number of bins in the histogram
        /// </summary>
        public int NBins { get; }
        public Color Color { get; }
        public string XAxisTitle { get; }

        public HistogramPrototype(int nbins, Color color, string xAxisTitle)
        {
            NBins = nbins;
            Color = color;
            XAxisTitle = xAxisTitle;
        }

        IGraphPrototype.GraphType IGraphPrototype.GetGraphType()
        {
            return IGraphPrototype.GraphType.Histogram;
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(NBins);
            bw.Write(Color);
            bw.Write(XAxisTitle);
        }

        internal HistogramPrototype(BinaryReader br)
        {
            NBins = br.ReadInt32();
            Color = br.ReadColor();
            XAxisTitle = br.ReadString();
        }
    }
}
