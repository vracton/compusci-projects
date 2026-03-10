using GraphData;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using RangePair = DongUtility.RangePair;

namespace GraphControl
{
    /// <summary>
    /// An underlying object that defines the logic for a single graph, which then is expanded by Graph
    /// </summary>
    public class GraphUnderlying : TransformingObject, IGraphInterface
    {
        private readonly List<Timeline> timelines = [];
        /// <summary>
        /// This is the point at which we stop trying to zoom and just create a fixed space around the data points.
        /// </summary>
        private const double roundoffRatio = 2e-5;

        /// <summary>
        /// Returns one of the timelines stored, by index.
        /// </summary>
        public Timeline GetTimeline(int index)
        {
            return timelines[index];
        }

        private readonly Legend legend = new();

        public GraphUnderlying(string xTitle, string yTitle) :
            base(xTitle, yTitle)
        {
            drawing.Children.Add(legend.Drawing);
        }

        /// <summary>
        /// Add a new timeline to the graph
        /// </summary>
        /// <param name="timeline"></param>
        public void AddTimeline(Timeline timeline)
        {
            timelines.Add(timeline);
            drawing.Children.Insert(0, timeline.GetDrawing());
            timeline.Transform = Transform;
            legend.AddTimeline(timeline);
        }

        /// <summary>
        /// Update all timelines with the given data
        /// </summary>
        public void Update(GraphDataPacket data)
        {
            for (int i = 0; i < timelines.Count; ++i)
            {
                timelines[i].Update(data);
            }
        }

        /// <summary>
        /// Rescales the graph so the data fits in the window
        /// </summary>
        public void UpdateTransform(double width, double height, double widthOffset, double heightOffset)
        {
            RangePair range = RangePair.Default();

            foreach (var timeline in timelines)
            {
                range.AdjustRange(timeline.RangePair);
            }

            if (range.X.Min == range.X.Max)
            {
                range.X = new DongUtility.Range(-1, 1);
            }
            if (range.Y.Min != 0)
            {
                double ratio = Math.Abs(range.Y.Width / range.Y.Min);
                if (ratio < roundoffRatio)
                {
                    double diff = roundoffRatio - ratio;
                    double shift = diff / 2 * range.Y.Min;
                    range.Y = new DongUtility.Range(range.Y.Min - shift, range.Y.Max + shift);
                }

            }

            if (double.IsInfinity(range.Width))
            {
                range.X = new DongUtility.Range(0, 1);

            }
            if (double.IsInfinity(range.Height))
            {
                range.Y = new DongUtility.Range(0, 1);
            }

            //centers the line in the middle of the graphs
            double alignNum = range.Y.Width * .1;
            range.Y = new DongUtility.Range(range.Y.Min - alignNum, range.Y.Max + alignNum);

            UpdateMatrix(width, height, widthOffset, heightOffset, range);
            UpdateAxes(width, height, widthOffset, heightOffset, range);
            SetLegend(width, height, widthOffset, heightOffset);
        }

        /// <summary>
        /// How far in, as a fraction, the right edge of the legend is from the right edge of the graph
        /// </summary>
        private const double legendMarginFactor = .1;

        /// <summary>
        /// Creates the legend
        /// </summary>
        private void SetLegend(double width, double height, double widthOffset, double heightOffset)
        {
            double rhs = width * (1 - legendMarginFactor) + widthOffset;
            double lhs = rhs - legend.Drawing.Bounds.Width;
            double top = legendMarginFactor * height + heightOffset;
            legend.Transform = new TranslateTransform(lhs, top);
        }

    }
}

