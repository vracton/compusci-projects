using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// A legend for a graph
    /// </summary>
    class Legend
    {
        private readonly DrawingGroup drawing = new();
        /// <summary>
        /// The actual drawing that is displayed visibly
        /// </summary>
        public Drawing Drawing { get { return drawing; } }

        /// <summary>
        /// The transformation used to display the drawing properly.
        /// </summary>
        public Transform Transform
        {
            get { return drawing.Transform; }
            set { drawing.Transform = value; }
        }

        private readonly IList<Tuple<string, Color>> list = [];

        /// <summary>
        /// Adds the name and the color of the line
        /// </summary>
        public void AddTimeline(Timeline timeline)
        {

            list.Add(new Tuple<string, Color>(timeline.Name, timeline.Color));
            Update();
        }

        /// <summary>
        /// Updates the legend with all of the boxes and descriptions
        /// </summary>
        private void Update()
        {
            drawing.Children.Clear();

            double currentY = 0;

            //if there is one thing being graphed, don't include a legend
            if (list.Count <= 1)
            {
                return;
            }
            else
            {
                //makes a new box and description for each tuple in the list
                foreach (var tuple in list)
                {
                    var myBrush = new SolidColorBrush(tuple.Item2);
                    var thisText = new FormattedText(tuple.Item1, CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight, new Typeface("Arial"), 10, myBrush, 1);
                    // 1 is pixels per dip, which seems not to matter
                    double boxSize = thisText.Height;
                    var geo = new RectangleGeometry(new Rect(0, currentY, boxSize, boxSize));
                    drawing.Children.Add(new GeometryDrawing(myBrush, null, geo));
                    var textGeo = thisText.BuildGeometry(new Point(1.5 * boxSize, currentY));
                    drawing.Children.Add(new GeometryDrawing(myBrush, null, textGeo));
                    currentY += boxSize;
                }

                //Rectangle border around legend
                var legendRect = new RectangleGeometry(new Rect(-.05 * drawing.Bounds.Width, -.05 * drawing.Bounds.Height, drawing.Bounds.Width * 1.1, 
                    drawing.Bounds.Height * 1.1));
                drawing.Children.Insert(0, new GeometryDrawing(Brushes.White, new Pen(Brushes.Black, 1), legendRect));
            }
          
        }
    }
}
