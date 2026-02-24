using DongUtility;
using GraphData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// A single scatterplot (treated as a timeline, but the x axis need not be time) for graphing on a graph
    /// </summary>
    public class Timeline : IUpdating
    {
        public string Name { get; set; }

        public Color Color { get; }

        private readonly PolyLineSegment line = new();
        private readonly PathGeometry geom = new();
        private readonly GeometryDrawing drawing = new();

        /// <summary>
        /// The geometric transform for the graph
        /// </summary>
        public Transform Transform
        {
            get
            {
                return geom.Transform;
            }
            set
            {
                geom.Transform = value;
            }
        }

        /// <summary>
        /// The thickness of the line on the graph
        /// </summary>
        public double Thickness { get; set; } = 3;

        public Timeline(string name, Color color)
        {
            Name = name;
            Color = color;

            drawing.Geometry = geom;
            var path = new PathFigure();
            geom.Figures.Add(path);
            path.Segments.Add(line);
            geom.Transform = Transform;

            drawing.Pen = new Pen(new SolidColorBrush(color), Thickness);
        }

        /// <summary>
        /// Returns the current graph.
        /// </summary>
        public GeometryDrawing GetDrawing()
        {
            return drawing;
        }

        private bool justStarted = true;

        /// <summary>
        /// Adds a point to the graph to be drawn.
        /// </summary>
        public void AddPoint(double xVal, double yVal)
        {
            if (justStarted)
            {
                geom.Figures[0].StartPoint = new Point(xVal, yVal);
                justStarted = false;
            }
            line.Points.Add(new Point(xVal, yVal));

            RangePair.SetMinMax(xVal, yVal);

            if (line.Points.Count > MaximumPoints)
            {
                DeleteFirstPoint();
            }
        }

        /// <summary>
        /// Converts all stored points into tuples.
        /// </summary>
        public List<Tuple<double, double>> ExtractPoints()
        {
            var list = new List<Tuple<double, double>>();

            foreach (var point in line.Points)
            {
                list.Add(new Tuple<double, double>(point.X, point.Y));
            }

            return list;
        }

        /// <summary>
        /// The maximum number of points that can be stored on the plot before it starts removing them
        /// </summary>
        static public double MaximumPoints { get; set; } = 1000;

        /// <summary>
        /// Deletes the first stored point.
        /// </summary>
        private void DeleteFirstPoint()
        {
            Point toRemove = line.Points[0];

            line.Points.RemoveAt(0);
            if (toRemove.X <= RangePair.X.Min)
            {
                RangePair.X = new DongUtility.Range(line.Points.Min(x => x.X), RangePair.X.Max);
            }
            if (toRemove.X >= RangePair.X.Max)
            {
                RangePair.X = new DongUtility.Range(RangePair.X.Min, line.Points.Max(x => x.X));
            }

            if (toRemove.Y <= RangePair.Y.Min)
            {
                RangePair.Y = new DongUtility.Range(line.Points.Min(x => x.Y), RangePair.Y.Max);
            }
            if (toRemove.Y >= RangePair.Y.Max)
            {
                RangePair.Y = new DongUtility.Range(RangePair.Y.Min, line.Points.Max(x => x.Y));
            }
            geom.Figures[0].StartPoint = toRemove;

        }

        /// <summary>
        /// Adds a new point based on the given funcions.
        /// </summary>
        public void Update(GraphDataPacket data)
        {
            double x = data.GetData();
            double y = data.GetData();
            AddPoint(x, y);
        }

        /// <summary>
        /// Gets the minimum and maximum values for both axes of the graph
        /// </summary>
        public RangePair RangePair { get; private set; } = RangePair.Default();
    }
}
