using Arena;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArenaVisualizer
{
    /// <summary>
    /// Interaction logic for ArenaVisualizerStandalone.xaml
    /// </summary>
    public partial class ArenaVisualizerStandalone : UserControl
    {
        public ArenaEngine TheArena { get; set; }
        public ArenaVisualizerStandalone(ArenaEngine arena)
        {
            InitializeComponent();
            TheArena = arena;
            Loaded += WhenLoaded;
            timer.Elapsed += Redraw;
            timer.AutoReset = true;
            timer.Start();
        }

        public void ArenaVisualizerStandalone_Closed(object sender, EventArgs e)
        {
            Display.Destroy();
        }

        private void Redraw(object sender, ElapsedEventArgs e)
        {
            if (Display != null && ShowVisual)
                Display.Redraw();
        }

        private Application app;
        private IntPtr hwndListBox;
        private Window myWindow;
        internal ArenaCoreInterface Display { get; set; } = null;

        private Timer timer = new Timer(33);

        private void OnUIReady(object sender, EventArgs e)
        {
            //var initial = TheArena.Initialization();

            app = Application.Current;
            myWindow = app.MainWindow;
            //myWindow.SizeToContent = SizeToContent.WidthAndHeight;
            Display = new ArenaCoreInterface(ArenaCoreInterfaceHolder.ActualWidth,
                ArenaCoreInterfaceHolder.ActualHeight, TheArena.Width, TheArena.Height);
            ArenaCoreInterfaceHolder.Child = Display;
            hwndListBox = Display.HwndListBox;

            Display.AfterStartup(TheArena);
            //Display.AfterStartup(new GraphicTurnSetAdapter((GraphicTurnSet)initial));
            ShowVisual = true;
        }

        private void WhenLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            InvalidateVisual();
        }

        public bool ShowVisual { get; set; } = false;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Display != null && ShowVisual)
            {
                Display.Redraw();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (Display != null)
                Display.ScaleDisplay((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height);
        }
    }
}
