using GraphData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// Interaction logic for LeaderBar.xaml
    /// Implements a single leader bar for a leader board, updating automatically
    /// </summary>
    public partial class LeaderBar : UserControl, IUpdating
    {
        public Color Color
        {
            get
            {
                return MyBrush.Color;
            }
            set
            {
                MyBrush.Color = value;
                // Put the title in a contrasting color
                Title.Foreground = new SolidColorBrush(WPFUtility.UtilityFunctions.HighContrast(value));
            }
        }

        /// <summary>
        /// The length of the bar, as a fraction of the total possible size of the bar (so it caps out at 1)
        /// </summary>
        public double BarLength
        {
            get
            {
                return Bar.Width / (ActualWidth * WidthPercentage);
            }
            set
            {
                Bar.Width = value > 0 ? value * ActualWidth * WidthPercentage : 0;
            }
        }

        public string NameOfBar { get { return Title.Text; } set { Title.Text = value; } }
        private double number = 0;

        /// <summary>
        /// The number that appears on the side of the bar, usually correlated with the size of the bar
        /// </summary>
        public double NumberOnRight { get { return number; } set { number = value; SetText(); } }

        /// <summary>
        /// The percentage of the height of the area filled up by the bar
        /// </summary>
        public double HeightPercentage { get; set; } = .75;
        /// <summary>
        /// The percentage of the width of the area filled up by the bar at maximum size
        /// </summary>
        public double WidthPercentage { get; set; } = .8;

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            double heightBased = ActualHeight * HeightPercentage;
            Title.FontSize = ActualHeight * HeightPercentage;

            double width = WPFUtility.UtilityFunctions.MeasureString(Title.Text, Title).Width;
            double widthBased = (ActualWidth * WidthPercentage / width) * WidthPercentage * Title.FontSize;

            Title.FontSize = Math.Min(heightBased, widthBased);

            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// The value that is used for calls to sort
        /// </summary>
        public double SortValue => NumberOnRight;

        private void SetText()
        {
            Number.Text = TextFunction(NumberOnRight);
        }

        /// <summary>
        /// Update the bar with the new data - just one number
        /// </summary>
        public void Update(GraphDataPacket data)
        {
            double value = data.GetData();
            NumberOnRight = value;

            SetText();

            // Adjust font size of Number to get it to fit
            double heightBased = ActualHeight * HeightPercentage;
            Number.FontSize = ActualHeight * HeightPercentage;
            double numberWidth = WPFUtility.UtilityFunctions.MeasureString(Number.Text, Number).Width;
            double numberWidthBased = (ActualWidth * (1 - WidthPercentage) / numberWidth) * WidthPercentage * Number.FontSize;
            Number.FontSize = Math.Min(heightBased, numberWidthBased);
        }

        public delegate string TextSetter(double val);
        /// <summary>
        /// The function used to display text. Change this to modify what is displayed in the leader bar.
        /// It must be passed in as a number and then modified by the function to custom text.
        /// </summary>
        public TextSetter TextFunction { get; set; } = DefaultTextFunction;

        /// <summary>
        /// By default, just return the number
        /// </summary>
        private static string DefaultTextFunction(double val)
        {
            return val.ToString();
        }

        public LeaderBar()
        {
            InitializeComponent();
        }

        public LeaderBar(TextSetter textFunction)
        {
            TextFunction = textFunction;
            InitializeComponent();
        }
    }
}
