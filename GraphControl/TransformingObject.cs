using System.Windows.Media;
using RangePair = DongUtility.RangePair;

namespace GraphControl
{
    /// <summary>
    /// Any object that will be displayed on a graph and needs to be transformed to the right location
    /// </summary>
    abstract public class TransformingObject
    {
        /// <summary>
        /// The transformation matrix that is applied to the object
        /// </summary>
        public MatrixTransform Transform { get; set; } = new MatrixTransform();

        /// <summary>
        /// The drawing group that contains the drawing
        /// </summary>
        protected DrawingGroup drawing = new();

        /// <summary>
        /// The drawing that represents this object
        /// </summary>
        public Drawing Drawing { get { return drawing; } }

        private readonly Axis xAxis;
        private readonly Axis yAxis;

        public TransformingObject(string xTitle, string yTitle)
        {
            xAxis = new Axis(xTitle);
            yAxis = new Axis(yTitle, true);
            drawing.Children.Add(xAxis.Drawing);
            drawing.Children.Add(yAxis.Drawing);
        }

        /// <summary>
        /// Updates the transformation matrix based on the size and position of the display area and the range of values of the graph
        /// </summary>
        protected void UpdateMatrix(double width, double height, double widthOffset, double heightOffset, RangePair range)
        {
            var mat = Transform.Matrix;
            double ax = width / (range.Width);
            double ay = height / (-range.Height);
            mat.M11 = ax;
            mat.M22 = ay;
            mat.OffsetX = -ax * range.X.Min + widthOffset;
            mat.OffsetY = -ay * range.Y.Max + heightOffset;
            Transform.Matrix = mat;
        }

        /// <summary>
        /// Updates the axes based on the size and position of the display area and the range of values of the graph
        /// </summary>
        protected void UpdateAxes(double width, double height, double widthOffset, double heightOffset, RangePair range)
        {
            xAxis.Transform = new TranslateTransform(widthOffset, height + heightOffset);
            var group = new TransformGroup();
            group.Children.Add(new RotateTransform(-90, 0, 0));
            group.Children.Add(new TranslateTransform(0, height + heightOffset));
            yAxis.Transform = group;

            xAxis.Update(width, heightOffset, range.X.Min, range.X.Max);
            yAxis.Update(height, widthOffset, range.Y.Min, range.Y.Max);
        }
    }
}
