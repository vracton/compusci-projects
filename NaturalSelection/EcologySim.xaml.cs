using System.Windows;
using System.Windows.Controls;
using Arena;
using ArenaVisualizer;
using WPFUtility;
using System;

namespace NaturalSelection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EcologySim : Window
    {
        private readonly ArenaEngine engine;
        public MainArenaVisualizer Arena { get; }

        public EcologySim(ArenaEngine engine)
        {
            this.engine = engine;
            Arena = new MainArenaVisualizer(engine, new ArenaVisualizerStandalone(engine));
            InitializeComponent();
            Arena.TimeIncrement = 1;

            ArenaSpot.Content = Arena.Content;

            TimeIncrementSlider.Text = Arena.TimeIncrement.ToString();

            ContentRendered += WireUpDisplay;
            ContentRendered += Arena.LinkManager;

            Arena.UpdatedTime += WhenUpdatedTime;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        private void WhenUpdatedTime(object? sender, EventArgs e)
        {
            TimeText.Text = Math.Round(Arena.DisplayTime, 2).ToString() + " days";
        }

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

        public double TimePerTurn
        {
            get
            {
                return Arena.TimeScale;
            }
            set
            {
                TimeIncrementSlider.Text = value.ToString();
            }
        }

        private void TimeIncrementSlider_TextChanged(object sender, TextChangedEventArgs e)
        {
            Arena.TimeScale = 1 / double.Parse(TimeIncrementSlider.Text);
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isPaused = !Arena.IsRunning;

            Arena.IsRunning = false;
            UtilityFunctions.SaveScreenshot((int)ActualWidth, (int)ActualHeight, this);

            if (!isPaused)
            {
                Arena.IsRunning = true;
                Arena.StartAll();
            }
        }

        private void Clipboard_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isPaused = !Arena.IsRunning;

            Arena.IsRunning = false;

            UtilityFunctions.MakeScreenshot((int)ActualWidth, (int)ActualHeight, this);

            if (!isPaused)
            {
                Arena.IsRunning = true;
                Arena.StartAll();
            }
        }
    }
}
