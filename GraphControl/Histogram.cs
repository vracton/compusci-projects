using GraphData;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using RangePair = DongUtility.RangePair;

namespace GraphControl
{
    /// <summary>
    /// An automatically binning, automatically scaling histogram
    /// </summary>
    public class Histogram : TransformingObject, IGraphInterface
    {
        private readonly IList<HistoBin> histo = [];
        private readonly IList<RectangleGeometry> geoList = [];

        /// <summary>
        /// Used internally to define a single bin
        /// </summary>
        /// <param name="lowEdge">The low edge of the bin - used to sort and distinguish the bins</param>
        private class HistoBin(double lowEdge)
        {
            /// <summary>
            /// The number of events in this bin
            /// </summary>
            public int Num { get; set; } = 0;
            /// <summary>
            /// The low edge of the bin - the high edge is not stored but it is assumed that the low edge 
            /// of the next highest bin is the high edge.
            /// </summary>
            public double LowEdge { get; set; } = lowEdge;
        }

        public Histogram(int nBins, Color color, string xTitle) :
            base(xTitle, "Frequency")
        {
            NBins = nBins;

            // Create a list of rectangles and get them ready
            var mybrush = new SolidColorBrush(color);
            for (int i = 0; i < nBins; ++i)
            {
                var geo = new RectangleGeometry
                {
                    Transform = Transform
                };
                geoList.Add(geo);

                var geoDraw = new GeometryDrawing(mybrush, null, geo);
                drawing.Children.Add(geoDraw);

            }

        }

        /// <summary>
        /// The number of bins in the histogram; note that the maximum and minimum values are done automatically and cannot be set
        /// </summary>
        public int NBins { get; }

        /// <summary>
        /// Update the histogram
        /// </summary>
        /// <param name="data">This needs to include a list of data</param>
        public void Update(GraphDataPacket data)
        {
            var list = data.GetSet().ToList();

            list.Sort();

            if (list.Count == 0)
                return;

            histo.Clear();

            // Automatically create the bin sizes
            double range = list.Last() - list.First();
            double binSize = range / (NBins - 1); // The minus 1 here gives a total number of NBins bins

            if (range < 1e-12) // Smaller than this and you get roundoff problems
            {
                binSize = .1; // Arbitrary bin size if all the numbers are the same
                double currentLow = list.First() - (binSize * NBins / 2);
                for (int i = 0; i < geoList.Count; ++i)
                {
                    int num = i == geoList.Count / 2 ? list.Count : 0; // Put all the numbers in the middle bin; sorry for the hard-coding
                    histo.Add(new HistoBin(currentLow) { Num = num });
                    currentLow += binSize;
                }
            }
            else
            {
                int currentBin = 0;
                double currentLow = list.First() - binSize / 2; // Offset to avoid roundoff problems
                histo.Add(new HistoBin(currentLow));



                foreach (var number in list)
                {
                    // Loop here to be broken when you actually add the number to a bin
                    // Is guaranteed not to break because we based this off the maximum of the list
                    while (true)
                    {
                        if (number <= currentLow + binSize)
                        {
                            ++histo[currentBin].Num;
                            break;
                        }
                        else
                        {
                            histo.Add(new HistoBin(currentLow += binSize));
                            ++currentBin;
                        }
                    }
                }
            }

            // Update the rectangles for visualization
            for (int i = 0; i < histo.Count; ++i)
            {
                geoList[i].Rect = new System.Windows.Rect(histo[i].LowEdge, 0, binSize, histo[i].Num);
            }
        }

        /// <summary>
        /// Creates the scaling needed for the overall histogram to display, given position and size on the screen
        /// </summary>
        public void UpdateTransform(double width, double height, double widthOffset, double heightOffset)
        {
            RangePair range;

            if (histo.Count == 0)
            {
                range = new RangePair(0, 0, 1, 1);
            }
            else
            {
                double binSize = (histo.Last().LowEdge - histo[0].LowEdge) / (NBins - 1); // The minus 1 accounts for only going to the low edge of the last histo
                range = new RangePair(histo[0].LowEdge, 0, histo.Last().LowEdge + binSize, histo.Max((x) => x.Num));
            }

            UpdateMatrix(width, height, widthOffset, heightOffset, range);

            UpdateAxes(width, height, widthOffset, heightOffset, range);
        }
    }
}
