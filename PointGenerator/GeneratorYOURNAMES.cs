using DongUtility;
using System.Drawing;

namespace PointGenerator
{
    /// <summary>
    /// Please make sure to change YOURNAMES to your actual names.
    /// Change both the class name and the file name.
    /// </summary>
    public class GeneratorYOURNAMES : Generator
    {
        public override string Names => "YOUR NAMES HERE"; // Please change this to your names

        protected override Point CreatePoint(bool signal)
        {
            // Here is an example of how your points can be generated.
            // It's not a good example because these are very easy to tell apart
            if (signal)
            {
                // Note that all positions must be between 0 and 100 inclusive
                double x = Random.NextDouble(0, 100);
                double y = Random.NextDouble(0, 100);
                double z = Random.NextDouble(0, 100);
                // Note that colors range between 0 and 255 inclusive
                int r = Random.Next(0, 128);
                int g = Random.Next(0, 128);
                int b = Random.Next(0, 128);

                var position = new Vector(x, y, z);
                var color = Color.FromArgb(r, g, b);
                return new Point(position, color);
            }
            // This part is for the background points.  You want this part to be
            // different in some crucial but hard-to-find way from the signal
            // so that you can tell them apart but no one else can, even with 
            // a decision tree.
            // This is a very simple example but there are much more interesting things you can do
            else
            {
                double x = Random.NextDouble(0, 100);
                double y = Random.NextDouble(0, 100);
                double z = Random.NextDouble(0, 100);
                int r = Random.Next(128, 256);
                int g = Random.Next(128, 256);
                int b = Random.Next(128, 256);

                var position = new Vector(x, y, z);
                var color = Color.FromArgb(r, g, b);
                return new Point(position, color);
            }
        }
    }
}
