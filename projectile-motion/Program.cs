using Helpers;
using System.IO;

class ProjectileMotion
{
    const double g = 9.8;
    const double C = 0.5;

    static void Main(string[] args)
    {
        LevelThree();
    }

    static void LevelOne()
    {
        Logger logger = new Logger("levelone.txt"); //logger both prints nicely to the console and writes to a file for graph making

        const double _m = 3.0;
        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 4.0;

        var v = new Vector(v0 * Math.Cos(theta), 0, v0 * Math.Sin(theta));
        var pos = new Vector();

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        int t = 0;
        while (pos.Z >= 0.0)
        {
            if (t > 0)
            {
                v.Z += -g * dt;

                pos += v * dt;
            }
            logger.WriteLine((t * dt), pos.X, pos.Z, v.Magnitude, g); //auto-rounds to 3 places
            t++;
        }
    }

    static void LevelTwo()
    {
        Logger logger = new Logger("leveltwo.txt");

        const double m = 3.0;
        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 4.0;

        var v = new Vector(v0 * Math.Cos(theta), 0.0, v0 * Math.Sin(theta));
        var pos = new Vector();

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        int t = 0; //ticks
        while (pos.Z >= 0)
        {
            double vTotal = v.Magnitude;          
            var a = -C * vTotal * v / m + new Vector(0.0, 0.0, -g); //calculate acceleration on each direction

            if (t > 0)
            {
                v += a * dt;

                pos += v * dt;
            }
            logger.WriteLine(t * dt, pos.X, pos.Z, v.Magnitude, a.Magnitude);
            t++;
        }
    }

    static void LevelThree()
    {
        Logger logger = new Logger("levelthree.txt");

        const double m = 3.0;
        const double k = 9.0;
        const double L = 3.0;

        var pos = new Vector(-1.0, 1.0, -3.0);
        var v = new Vector(5.0, -1.0, -3.0);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "y", "z", "v", "a");

        int lastTickG1 = 0; //last tick where velocity was greater than one

        for (int t = 0; t <= 20 * (int)(1 / dt); t++) // 20 is abitrary, but worked out
        {
            //calculate acceleration from spring (Hooke's law)
            double dMag = pos.Magnitude; //magnitude of position vector which happens to be the displacement vector since spring is anchored at origin
            var dUV = pos.UnitVector; //displacement unit vector
            var aSpring = -k * (dMag - 3.0) * dUV / m;

            var vTotal = v.Magnitude;
            var aDrag = -C * v * vTotal / m;

            var aTotal = aDrag + aSpring + new Vector(0, 0, -g);
            if (t > 0)
            {
                v += aTotal * dt;

                if (v.Magnitude >= 1)
                {
                    lastTickG1 = t;
                }

                pos += v * dt;
            }
            logger.WriteLine(t * dt, pos, v.Magnitude, aTotal.Magnitude);
        }

        logger.WriteLine($"last time above 1 m/s was {lastTickG1 * dt}");
    }
}