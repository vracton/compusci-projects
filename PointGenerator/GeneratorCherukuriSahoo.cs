using DongUtility;
using System.Drawing;
namespace PointGenerator
{
    /// <summary>
    /// Please make sure to change YOURNAMES to your actual names.
    /// Change both the class name and the file name.
    /// </summary>
    public class GeneratorCherukuriSahoo : Generator
    {

        private double x;
        private double y;

        public GeneratorCherukuriSahoo()
        {
            x = Random.NextDouble(30, 70);
            y = Random.NextDouble(30, 70);
        }

        public override string Names => "Kalyan Cherukuri, Sonit Sahoo"; // Please change this to your names
        protected override Point CreatePoint(bool signal)
        {
            // Here is an example of how your points can be generated.
            // It's not a good example because these are very easy to tell apart


            if (signal)
            {
                // signal all clustered within some \epsilon of (37, 25, 85) -- scratch that we're doing random x, y and approx 85 for z
                // compounded with dr. dong's jitter this yields a tight cluster of points that are all close together
                double SignalX = x;
                double SignalY = y;

                double z = Random.NextDouble(83, 84);

                // Note that all positions must be between 0 and 100 inclusive
                // Note that colors range between 0 and 255 inclusive

                //color is congruent to 0 mod 98
                int r = Random.Next(0, 98);
                int g = Random.Next(0, 98 - r);
                int b = Random.Next(0, 98 - r - g);


                var position = new Vector(SignalX, SignalY, z);
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
                // Background points are sampled from the curved surface of a hemisphere (x^2+ y^2 + z^2 = r^2)
                // they are spread across the dome so they look related to signal

                //spherical coordinates akin to that of MVC
                double theta = Random.NextDouble(10, 90) * Math.PI / 180;
                double phi = Random.NextDouble(0, 360) * Math.PI / 180;

                double BackgroundX = x + 3 * Math.Sin(theta) * Math.Cos(phi);
                double BackgroundY = y + 3 * Math.Sin(theta) * Math.Sin(phi);
                double z = 85 + 3 * Math.Cos(theta);

                //mod 121 for background
                int r = Random.Next(0, 121);
                int g = Random.Next(0, 121 - r);
                int b = Random.Next(0, 121 - r - g);

                var position = new Vector(BackgroundX, BackgroundY, z);
                var color = Color.FromArgb(r, g, b);
                return new Point(position, color);
            }
        }
    }
}