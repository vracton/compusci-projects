using Arena;
using ArenaVisualizer;
using NeuralNetStudentVersion;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainArenaVisualizer Arena;

        public MainWindow(double xSize, double ySize, double timeStep, Perceptron perceptron, int nHares = 1, int nLynxes = 1)
        {
            var engine = new PredatorPreyEngine(xSize, ySize, perceptron, nHares, nLynxes);
            Title = "Predator / Prey";
            InitializeArena(timeStep, engine);
        }

        private void InitializeArena(double timeStep, ArenaEngine engine)
        {
            var display = new ArenaVisualizerStandalone(engine);
            Arena = new MainArenaVisualizer(engine, display);
            InitializeComponent();
            Arena.TimeIncrement = timeStep;

            Arena.SlowDraw = true;

            ArenaSpot.Content = Arena.Content;

            ContentRendered += WireUpDisplay;
            ContentRendered += Arena.LinkManager;

            Arena.UpdatedTime += WhenUpdatedTime;
        }

        public MainWindow(double timeStep, ArenaEngine engine)
        {
            Title = "Emergent Behavior";
            InitializeArena(timeStep, engine);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        /// <summary>
        /// An event for updating text when the time is updated
        /// </summary>
        private void WhenUpdatedTime(object? sender, EventArgs e)
        {
            TimeText.Text = Math.Round(Arena.DisplayTime, 2).ToString() + " s";
        }

        /// <summary>
        /// An event for connecting the display once the back end is ready
        /// </summary>
        private void WireUpDisplay(object? sender, EventArgs e)
        {
            Arena.Visualizer = Arena.Display;
            InvalidateVisual();
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Arena.IsRunning)
            {
                Start_Button.Content = "Resume";
                Arena.IsRunning = false;
            }
            else
            {
                Start_Button.Content = "Pause";
                Arena.IsRunning = true;

                Arena.StartAll();
            }
        }

        private void TimeIncrementSlider_TextChanged(object sender, TextChangedEventArgs e)
        {
            Arena.TimeIncrement = double.Parse(TimeIncrementSlider.Text);
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            bool needToRestart = false;
            if (Arena.IsRunning)
            {
                Arena.IsRunning = false;
                needToRestart = true;
            }

            WPFUtility.UtilityFunctions.SaveScreenshot((int)ActualWidth, (int)ActualHeight, this);

            if (needToRestart)
            {
                Arena.IsRunning = true;
            }
        }

        private void Clipboard_Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(WPFUtility.UtilityFunctions.MakeScreenshot((int)ActualWidth,
                (int)ActualHeight, this));
        }
    }
}
