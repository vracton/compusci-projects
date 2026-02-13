using System.Collections;
using System.Drawing;

namespace DongUtility
{
    /// <summary>
    /// Simple utility functions
    /// </summary>
    static public class UtilityFunctions
    {
        static public double DegreesToRadians(double degrees)
        {
            return degrees / 180 * Math.PI;
        }

        static public double RadiansToDegrees(double radians)
        {
            return radians / Math.PI * 180;
        }

        /// <summary>
        /// Square a number
        /// </summary>
        static public double Square(double input)
        {
            return input * input;
        }

        /// <summary>
        /// Raise any number to an integer power (including negative powers)
        /// Thanks to Wikipedia for the algorithm
        /// </summary>
        static public double Pow(double baseNum, int exponent)
        {
            // Calculate numbers in place to avoid unnecessary extra allocation
            if (exponent == 0)
                return 1;
            else if (exponent == 1)
                return baseNum;
            else if (exponent < 0)
                return 1 / Pow(baseNum, -exponent);

            double extraFactor = 1;
            while (exponent > 1)
            {
                if (exponent % 2 == 0)
                {
                    baseNum *= baseNum;
                    exponent /= 2;
                }
                else
                {
                    extraFactor *= baseNum;
                    baseNum *= baseNum;
                    exponent = (exponent - 1) / 2;
                }
            }
            return baseNum * extraFactor;
        }

        /// <summary>
        /// Return if input is between low and high (exclusive)
        /// </summary>
        static public bool Between(double input, double low, double high)
        {
            return input > low && input < high;
        }

        /// <summary>
        /// Return if input is between bound1 and bound2, with no assumption as to which is larger
        /// </summary>
        static public bool UnsortedBetween(double input, double bound1, double bound2)
        {
            double lower = Math.Min(bound1, bound2);
            double upper = Math.Max(bound1, bound2);
            return Between(input, lower, upper);
        }

        /// <summary>
        /// Return if input is between low and high (inclusive)
        /// </summary>
        static public bool BetweenInclusive(double input, double low, double high)
        {
            return input >= low && input <= high;
        }

        /// <summary>
        /// Sorts an IList
        /// Rhanks to https://stackoverflow.com/questions/15486/sorting-an-ilist-in-c-sharp
        /// </summary>
        static public void Sort<T>(IList<T> list)
        {
            ArrayList.Adapter((IList)list).Sort();
        }

        /// <summary>
        /// Takes a list and permutes it in a predictable way such that it will cycle through all possible permutations
        /// Returns false if it is at the last permutation
        /// Thanks to https://www.nayuki.io/page/next-lexicographical-permutation-algorithm
        /// </summary>
        static public bool NextPermutation<T>(IList<T> list) where T : IComparable<T>
        {
            // Find non-increasing suffix
            int i = list.Count - 1;
            while (i > 0 && list[i - 1].CompareTo(list[i]) > 0)
                i--;
            if (i <= 0)
                return false;

            // Find successor to pivot
            int j = list.Count - 1;
            while (list[j].CompareTo(list[i - 1]) < 0)
                j--;
            (list[j], list[i - 1]) = (list[i - 1], list[j]);

            // Reverse suffix
            j = list.Count - 1;
            while (i < j)
            {
                (list[i], list[j]) = (list[j], list[i]);
                i++;
                j--;
            }

            return true;
        }

        /// <summary>
        /// Invert a color simply, in RGB space
        /// </summary>
        public static Color InvertColor(Color color)
        {
            return Color.FromArgb(color.A, Constants.MaxByte - color.R, Constants.MaxByte - color.G, 
                Constants.MaxByte - color.B);
        }

        /// <summary>
        /// Finds the maximum of a list
        /// </summary>
        public static double Max(params double[] list)
        {
            double returnVal = list[0];
            for (int i = 1; i < list.Length; ++i)
            {
                if (list[i] > returnVal)
                    returnVal = list[i];
            }
            return returnVal;
        }

        /// <summary>
        /// Returns the minimum of a list
        /// </summary>
        public static double Min(params double[] list)
        {
            double returnVal = list[0];
            for (int i = 1; i < list.Length; ++i)
            {
                if (list[i] < returnVal)
                    returnVal = list[i];
            }
            return returnVal;
        }

        /// <summary>
        /// Checks if a number is not NaN or infinity
        /// </summary>
        public static bool IsValid(double number)
        {
            return !(double.IsNaN(number) || double.IsInfinity(number));
        }

        /// <summary>
        /// Finds the order of magnitude of a number
        /// </summary>
        public static int OrderOfMagnitude(double num)
        {
            return num <= 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(num)));
        }

        /// <summary>
        /// Clamp a number between two values
        /// </summary>
        public static double Clamp(double num, double min, double max)
        {
            return Math.Min(Math.Max(num, min), max);
        }

        /// <summary>
        /// Remove any of a list of characters from a string
        /// </summary>
        /// <returns>A new string, since strings are immutable</returns>
        public static string RemoveCharacters(string input, params char[] characters)
        {
            string returnString = "";
            foreach (var character in input)
            {
                if (!characters.Contains(character))
                {
                    returnString += character;
                }
            }
            return returnString;
        }

        /// <summary>
        /// Returns the midpoint of the extremes of a list of vectors in x, y, and z directions
        /// </summary>
        public static Vector GetCenter(IEnumerable<Vector> list)
        {
            Vector min = new(double.MaxValue, double.MaxValue, double.MaxValue);
            Vector max = new(double.MinValue, double.MinValue, double.MinValue);

            foreach (var vertex in list)
            {
                if (vertex.X < min.X)
                    min.X = vertex.X;
                if (vertex.X > max.X)
                    max.X = vertex.X;
                if (vertex.Y < min.Y)
                    min.Y = vertex.Y;
                if (vertex.Y > max.Y)
                    max.Y = vertex.Y;
                if (vertex.Z < min.Z)
                    min.Z = vertex.Z;
                if (vertex.Z > max.Z)
                    max.Z = vertex.Z;
            }

            Vector center = (max + min) / 2;
            return center;
        }
    }
}
