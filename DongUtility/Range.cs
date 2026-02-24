namespace DongUtility
{
    /// <summary>
    /// A data structure for min and max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public class Range(double min, double max)
    {
        public double Min = min;
        public double Max = max;
        public double Width => Max - Min;

        /// <summary>
        /// Compares inserted value to current max and min and if it is greater than the max or less than the min
        /// it becomes that value.
        /// </summary>
        /// <param name="val">New min or max</param>
        public void SetMinMax(double val)
        {
            if (val < Min)
                Min = val;
            if (val > Max)
                Max = val;
        }

        /// <summary>
        /// Gives a default range where any inserted value for min would be less and any value for max would be more
        /// </summary>
        static public Range Default()
        {
            return new Range(double.MaxValue, double.MinValue);
        }

        /// <summary>
        /// Updates range for the inserted Range
        /// </summary>
        /// <param name="pair">Range with new max and min</param>
        public void AdjustRange(Range pair)
        {
            AdjustRange(pair.Min, pair.Max);
        }

        /// <summary>
        /// Updates range for the inserted min and max and checks if the inserted values are 
        /// less than min and greater than max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void AdjustRange(double min, double max)
        {
            // Swap them if max is less than min
            if (max < min)
            {
                (min, max) = (max, min); // This is a swap - pretty cool notation!
            }

            if (min < Min)
                Min = min;
            if (max > Max)
                Max = max;
        }

        /// <summary>
        /// Check if a point lies in the range
        /// </summary>
        public bool IsInRange(double val)
        {
            return val >= Min && val <= Max;
        }

        /// <summary>
        /// Checks if any of a different Range called other lies in this range
        /// </summary>
        public bool Overlaps(Range other)
        {
            // The last one checks to see if this Range is completely inside the other RangePair
            // The reverse is covered by the first two checks, which also cover partial overlap
            return IsInRange(other.Min) || IsInRange(other.Max) || (other.Min < Min && other.Max > Max);
        }
    }
}

