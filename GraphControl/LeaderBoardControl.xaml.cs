using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using GraphData;
using System.IO;

namespace GraphControl
{
    /// <summary>
    /// Interaction logic for LeaderBoardControl.xaml
    /// This contains a list of LeaderBars, updates them, and sorts them.
    /// </summary>
    public partial class LeaderBoardControl : UserControl, IUpdating
    {
        public LeaderBoardControl()
        {
            InitializeComponent();
        }

        public void WriteFile(string filename)
        {
            using var file = File.CreateText(filename);
            foreach (var bar in entries)
            {
                file.WriteLine($"{bar.NameOfBar}\t{bar.Number.Text}");
            }
        }

        private readonly List<LeaderBar> entries = [];
        private readonly List<LeaderBar> forSorting = [];

        /// <summary>
        /// Returns whether one of the leader bars has a given name.
        /// </summary>
        public bool HasEntry(string name) => entries.Any(x => x.NameOfBar == name);

        public IEnumerable<LeaderBar> Bars => entries;

        /// <summary>
        /// Adds a new leader bar to the leaderboard
        /// </summary>
        public void AddEntry(string name, Color color)
        {
            var newBar = new LeaderBar
            {
                NameOfBar = name,
                Color = color
            };

            entries.Add(newBar);
            forSorting.Add(newBar);

            TheGrid.Rows = entries.Count;
            TheGrid.Children.Add(newBar);

            Sort();
        }

        /// <summary>
        /// Updates all the leader bars using data from a GraphDataPacket
        /// </summary>
        public void Update(GraphDataPacket data)
        {
            // This peels off an extraneous size variable we don't want
            data.GetData();

            foreach (var entry in entries)
            {
                entry.Update(data);
            }

            Sort();            
        }

        /// <summary>
        /// Whether the leader bars should be sorted in descending order.  True by default.
        /// </summary>
        public bool DescendingOrder { get; set; } = true;

        /// <summary>
        /// Sorts the leader bars based on their values
        /// </summary>
        private void Sort()
        {
            if (entries.Count == 0)
            { 
                return; 
            }

            // Do it manually and not with Max() or Min() or else we get problems if NaNs are present
            double max = double.MinValue;
            double min = double.MaxValue;
            foreach (var bar in entries)
            {
                if (bar.NumberOnRight > max)
                {
                    max = bar.NumberOnRight;
                }
                if (bar.NumberOnRight < min)
                {
                    min = bar.NumberOnRight;
                }
            }

            double bottom = Math.Min(min, 0); // This allows for negative minimum scores

            foreach (var bar in entries)
            {
                bar.BarLength = (bar.NumberOnRight - bottom) / (max - bottom);
            }

            int multiplier = DescendingOrder ? -1 : 1;

            forSorting.Sort((x, y) => multiplier * (x.SortValue.CompareTo(y.SortValue)));
            TheGrid.Children.Clear();
            foreach (var entry in forSorting)
            {
                TheGrid.Children.Add(entry);
            }
        }

    }
}
