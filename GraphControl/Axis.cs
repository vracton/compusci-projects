using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using DongUtility;

namespace GraphControl
{
    /// <summary>
    /// A class to create an axis for a graph
    /// </summary>
    /// <param name="bottom">
    /// True if the graph is on the bottom; otherwise, it is on the left
    /// </param>
    internal class Axis(string title, bool bottom = false)
    {
        /// <summary>
        /// All the objects in the axis, as a DrawingGroup so things can be added and removed
        /// </summary>
        private readonly DrawingGroup drawing = new();

        /// <summary>
        /// The drawable output of the axis
        /// </summary>
        public Drawing Drawing { get { return drawing; } }

        /// <summary>
        /// The Transformation that is applied to the underlying drawing
        /// </summary>
        public Transform Transform
        {
            get { return drawing.Transform; }
            set { drawing.Transform = value; }
        }

        /// <summary>
        /// The title on the axis
        /// </summary>
        public string Title { get; set; } = title;

        /// <summary>
        /// The line that makes up the main axis
        /// </summary>
        private readonly LineGeometry line = new();

        /// <summary>
        /// The geometry object for the axis itself
        /// </summary>
        private System.Windows.Media.Geometry geoAxis = new GeometryGroup();

        /// <summary>
        /// A geometry object for the scientific notation multiplier that goes on the end of the axis
        /// </summary>
        private System.Windows.Media.Geometry scientificNotation = new GeometryGroup();

        /// <summary>
        /// The group of all Geometry objects
        /// </summary>
        private GeometryGroup group = new();

        /// <summary>
        /// All axis labels
        /// </summary>
        private readonly DrawingGroup axisLabels = new();

        /// <summary>
        /// A dictionary that stores generated labels so they don't need to be generated again
        /// </summary>
        private readonly Dictionary<double, System.Windows.Media.Geometry> labelDict = [];

        /// <summary>
        /// Creates a Geometry object from input text
        /// </summary>
        private static System.Windows.Media.Geometry MakeText(string text, Point position, int fontSize = 10)
        {
            FormattedText thisText = new(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Arial"), fontSize, Brushes.Black, 1)
            // 1 is the pixels per dip, which seems not to matter for us
            {
                TextAlignment = TextAlignment.Center
            };
            return thisText.BuildGeometry(position);
        }

        /// <summary>
        /// Updates the graph given its new content
        /// </summary>
        /// <param name="width">The width of the graph</param>
        /// <param name="height">The height of the graph</param>
        /// <param name="min">The minimum value of the values in the graph</param>
        /// <param name="max">The maximum value of the values in the graph</param>
        public void Update(double width, double height, double min, double max)
        {
            if (bottom)
            {
                line.StartPoint = new Point(0, height);
                line.EndPoint = new Point(width, height);
            }
            else
            {
                line.StartPoint = new Point(0, 0);
                line.EndPoint = new Point(width, 0);
            }

            MakeLabels(width, height, min, max);
            group.Transform = new TranslateTransform(width / 2, height / 3);

        }

        /// <summary>
        /// Automatically creates axis labels and stores them
        /// </summary>
        private void MakeLabels(double width, double height, double min, double max)
        {
            axisLabels.Children.Clear();

            // A check to give some room if there is only one value on the axis
            if (min == max)
            {
                if (min > 0)
                {
                    max *= 2;
                    min = 0;
                }
                else if (min == 0)
                {
                    min = -1;
                    max = 1;
                }
                else
                {
                    min *= 2;
                    max = 0;
                }
            }

            int order = UtilityFunctions.OrderOfMagnitude(max - min);
            double scale = Math.Pow(10, order);

            // Adjust the scale if the difference is too small to show up well
            if ((max - min) / scale <= 2)
            {
                scale /= 2;
            }

            // Assign left-hand and right-hand sides
            double lh = scale * Math.Floor(min / scale);
            double rh = scale * Math.Ceiling(max / scale);

            // The location of the scale values
            double location = bottom ? 3 * height / 4 : height / 4;

            // Check for roundoff error
            while (lh + scale == lh)
                scale *= 10;

            // If max is larger than 1000, use scientific notation to shorten
            int scaleOrder = UtilityFunctions.OrderOfMagnitude(max);

            for (double place = lh; place <= rh; place += scale)
            {
                double pos = Position(min, max, width, place);
                if (pos <= width && pos >= 0)
                {
                    if (Math.Abs(scaleOrder) >= 3)
                        AddPoint(new Point(pos, location), place / UtilityFunctions.Pow(10, scaleOrder));
                    else
                        AddPoint(new Point(pos, location), place);
                }
            }

            AddLabel(new Point(0, bottom ? 5 : 10), Title, scaleOrder);
        }

        /// <summary>
        /// Calculate the position of a particular point on the axis in absolute terms
        /// </summary>
        private static double Position(double min, double max, double width, double x)
        {
            return (x - min) / (max - min) * width;
        }

        /// <summary>
        /// The number of digits to round to
        /// </summary>
        const int roundDigits = 3;

        /// <summary>
        /// Add a numerical label to the geometry group at a given position
        /// </summary>
        private void AddPoint(Point position, double value)
        {
            Transform finalTransform;
            var trans = new TranslateTransform(position.X, position.Y);
            // Rotate depending on whether the axis is horizontal
            if (bottom)
            {
                var tr = new TransformGroup();
                tr.Children.Add(trans);
                tr.Children.Add(new RotateTransform(90, position.X, position.Y));
                finalTransform = tr;
            }
            else
            {
                finalTransform = trans;
            }

            // Create text only if it is not already in the dictionary
            System.Windows.Media.Geometry newGeom;
            if (labelDict.TryGetValue(value, out System.Windows.Media.Geometry? value2))
            {
                newGeom = value2;
            }
            else
            {
                string printVersion = (Math.Round(value, roundDigits)).ToString();
                newGeom = MakeText(printVersion, new Point(0, 0));
                labelDict.Add(value, newGeom);
            }

            newGeom.Transform = finalTransform;
            Drawing newDrawing = new GeometryDrawing(Brushes.Black, null, newGeom);
            axisLabels.Children.Add(newDrawing);
        }

        /// <summary>
        /// Add a label given that the point is raised to a particular power
        /// </summary>
        private void AddLabel(Point position, string title, int power)
        {
            drawing.Children.Clear();
            var trans = new TranslateTransform(position.X, position.Y);
            if (bottom)
            {
                var tr = new TransformGroup();
                tr.Children.Add(trans);
                tr.Children.Add(new RotateTransform(90, position.X, position.Y));
            }

            // Add scientific notation if order of magnitude is greater than 10^3
            if (Math.Abs(power) >= 3)
            {
                const int powerFont = 8;
                geoAxis = MakeText(title + " ×10", new Point(0, bottom ? 5 : 10));
                scientificNotation = MakeText(power+"", new Point(geoAxis.Bounds.Right+(powerFont), geoAxis.Bounds.Top-(powerFont/3.0)), powerFont);
                group = new GeometryGroup();
                group.Children.Add(geoAxis);
                group.Children.Add(scientificNotation);

            }
            else
            {
                geoAxis = MakeText(title, new Point(0, bottom ? 5 : 10));
                group = new GeometryGroup();
                group.Children.Add(geoAxis);
            }
            drawing.Children.Add(new GeometryDrawing(null, new Pen(Brushes.Black, 2), line));
            drawing.Children.Add(new GeometryDrawing(Brushes.Black, null, group));
            drawing.Children.Add(axisLabels);
        }
    }
}
