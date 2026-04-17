namespace DongUtility
{
    /// <summary>
    /// A pair of ranges that are used, for example, for x and y axes
    /// </summary>
    public class RangePair
    { 

        /// <summary>
        /// Gives a default range pair where any inserted value for min would be less and any value for max would be more
        /// </summary>
        static public RangePair Default()
        {
            return new RangePair(Range.Default(), Range.Default());
        }

        /// <summary>
        /// Initializes a RangePair, one for the x and y axis
        /// </summary>
        public RangePair(double minx, double miny, double maxx, double maxy)
        {
            X = new Range(minx, maxx);
            Y = new Range(miny, maxy);
        }

        /// <summary>
        /// Initializes a RangePair from two existing Ranges
        /// </summary>
        public RangePair(Range x, Range y)
        {
            X = x;
            Y = y;
        }

        public Range X { get; set; }
        public Range Y { get; set; }

        public double Width => X.Width;
        public double Height => Y.Width;

        /// <summary>
        /// Sets min and max for both RangePairs in Range  
        /// </summary>
        /// <param name="xVal">New min or max for x axis</param>
        /// <param name="yVal">New min or max for y axis</param>
        public void SetMinMax(double xVal, double yVal)
        {
            X.SetMinMax(xVal);
            Y.SetMinMax(yVal);
        }


        /// <summary>
        /// Updates range for inserted range on both Ranges of this RangePair
        /// </summary>
        /// <param name="other">A RangePair to be used to check new maxes and mins</param>
        public void AdjustRange(RangePair other)
        {
            X.AdjustRange(other.X);
            Y.AdjustRange(other.Y);
        }

    }
}
