using DongUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Arena
{
    /// <summary>
    /// Interaction logic for ArenaViewer.xaml
    /// </summary>
    public partial class ArenaViewer : UserControl
    {
        private ArenaEngine arena;
        private DrawingGroup group = new DrawingGroup();

        public ArenaViewer(ArenaEngine arena)
        {
            this.arena = arena;
            InitializeComponent();
        }

        public Vector2D UpperLeft { get; set; }
        public Vector2D LowerRight { get; set; }

        public void Update()
        {
           // Drawing drawing = arena.Drawing.Clone();
            group.Children.Clear();
            double coordWidth = LowerRight.X - UpperLeft.X;
            double coordHeight = LowerRight.Y - UpperLeft.Y;

            //            group.ClipGeometry = new RectangleGeometry(new Rect(UpperLeft.X, UpperLeft.Y,
            //                coordWidth, coordHeight));
            //            group.Transform = new MatrixTransform(ArenaBase.Transform(coordWidth, coordHeight, ActualWidth, ActualHeight));
            //group.Children.Add(drawing);


            //InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawDrawing(group);
        }
    }
}
