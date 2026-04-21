using Arena;
using GraphControl;
using GraphData;
using MotionVisualizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
using System.Windows.Threading;

namespace ArenaVisualizer
{
    /// <summary>
    /// Interaction logic for ArenaVisualizer.xaml
    /// </summary>
    public partial class MainArenaVisualizer : MotionVisualizerBase<ArenaCoreInterface, GraphicTurnAdapter>
    {
        private ArenaVisualizerStandalone arena;
        public ArenaCoreInterface Display => arena.Display;

        public MainArenaVisualizer(ArenaEngine engine, ArenaVisualizerStandalone display) :
            base(new ArenaEngineAdapter(engine), display.Display)
        {
            arena = display;
            FinishInitialization();
        }

        public MainArenaVisualizer(string filename, ArenaCommandFileReader reader, ArenaVisualizerStandalone display) :
            base(filename, reader, new ArenaCoreInterface())
        {
            arena = display;
            FinishInitialization();
        }

        public Color BackgroundColor
        {
            set
            {
                GraphSpot.Background = new SolidColorBrush(value);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Display.Redraw();
        }

        public MainArenaVisualizer(string filename, ArenaCommandFileReader reader) :
            base(filename, reader, new ArenaCoreInterface())
        {
            FinishInitialization();
        }

        private void FinishInitialization()
        {
            InitializeComponent();

            //ContentRendered += LinkManager;

            //arena.Display = Visualizer;
            ArenaViewport.Content = arena;

            GraphSpot.Content = Graphs;
        }

        public override void UpdateTime(double time)
        {
            OnUpdatedTime(EventArgs.Empty);
        }

        public event EventHandler UpdatedTime;

        protected virtual void OnUpdatedTime(EventArgs e)
        {
            UpdatedTime?.Invoke(this, e);
        }
    }
}
