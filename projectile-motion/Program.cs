using Logging;
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

        var v = (x: v0 * Math.Cos(theta), z: v0 * Math.Sin(theta));
        var pos = (x: 0.0, z: 0.0);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        int t = 0;
        while (pos.z >= 0.0)
        {
            if (t > 0)
            {
                v.z += -g * dt;

                pos.x += v.x * dt;
                pos.z += v.z * dt;
            }
            logger.WriteLine((t * dt), pos.x, pos.z, Math.Sqrt(v.x * v.x + v.z * v.z), g); //auto-rounds to 3 places
            t++;
        }
    }

    static void LevelTwo()
    {
        Logger logger = new Logger("leveltwo.txt");

        const double m = 3.0;
        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 4.0;

        var v = (x: v0 * Math.Cos(theta), z: v0 * Math.Sin(theta));
        var pos = (x: 0.0, z: 0.0);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        int t = 0; //ticks
        while (pos.z >= 0)
        {
            double vTotal = Math.Sqrt(v.x * v.x + v.z * v.z);          
            var a = (x: -C * vTotal * v.x / m, z: -C * vTotal * v.z / m - g); //calculate acceleration on each direction

            if (t > 0)
            {
                v.x += a.x * dt;
                v.z += a.z * dt;

                pos.x += v.x * dt;
                pos.z += v.z * dt;
            }
            logger.WriteLine(t * dt, pos.x, pos.z, Math.Sqrt(v.x * v.x + v.z * v.z), Math.Sqrt(a.x * a.x + a.z * a.z));
            t++;
        }
    }

    static void LevelThree()
    {
        Logger logger = new Logger("levelthree.txt");

        const double m = 3.0;
        const double k = 9.0;
        const double L = 3.0;

        var pos = (x: -1.0, y: 1.0, z: -3.0);
        var v = (x: 5.0, y: -1.0, z: -3.0);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "y", "z", "v", "a");

        int lastTickG1 = 0; //last tick where velocity was greater than one

        for (int t = 0; t <= 20 * (int)(1 / dt); t++) // 20 is abitrary, but worked out
        {
            //calculate acceleration from spring (Hooke's law)
            double dMag = Math.Sqrt(pos.x * pos.x + pos.y * pos.y + pos.z * pos.z); //magnitude of position vector which happens to be the displacement vector since spring is anchored at origin
            var dUV = (x: pos.x / dMag, y: pos.y / dMag, z: pos.z / dMag); //displacement unit vector
            var aSpring = (x: -k * (dMag - 3.0) * dUV.x / m, y: -k * (dMag - 3.0) * dUV.y / m, z: -k * (dMag - 3.0) * dUV.z / m);

            var vTotal = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            var aDrag = (x: -C * v.x * vTotal / m, y: -C * v.y * vTotal / m, z: -C * v.z * vTotal / m);

            var aTotal = (x: aDrag.x + aSpring.x, y: aDrag.y + aSpring.y, z: aDrag.z + aSpring.z - g);
            if (t > 0)
            {
                v.x += aTotal.x * dt;
                v.y += aTotal.y * dt;
                v.z += aTotal.z * dt;

                if (Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z) >= 1)
                {
                    lastTickG1 = t;
                }

                pos.x += v.x * dt;
                pos.y += v.y * dt;
                pos.z += v.z * dt;
            }
            logger.WriteLine(t * dt, pos.x, pos.y, pos.z, Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z), Math.Sqrt(aTotal.x * aTotal.x + aTotal.y * aTotal.y + aTotal.z * aTotal.z));
        }

        logger.WriteLine($"last time above 1 m/s was {lastTickG1 * dt}");
    }
}