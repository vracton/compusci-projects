using GraphData;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphControl
{
    /// <summary>
    /// Interaction logic for UpdatingText.xaml
    /// Refers to displayed text that updates based on incoming data
    /// </summary>
    public partial class UpdatingText : UserControl, IUpdating
    {
        public UpdatingText()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The title (static) of the text
        /// </summary>
        public string Title
        {
            get
            {
                return TitleBlock.Text;
            }
            set
            {
                TitleBlock.Text = value;
            }
        }
        public Color Color
        {
            get
            {
                return TitleBlock.Foreground is SolidColorBrush br ? br.Color : Colors.Black;
            }
            set
            {
                Brush newBrush = new SolidColorBrush(value);
                TitleBlock.Foreground = newBrush;
                TextBlock.Foreground = newBrush;
            }
        }

        /// <summary>
        /// Updates the value of the text based on the incoming data packet
        /// </summary>
        public void Update(GraphDataPacket text)
        {
            TextBlock.Text = text.GetTextData();
        }
    }
}
