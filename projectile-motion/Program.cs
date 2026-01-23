using Logging;
using System.IO;

class ProjectileMotion
{
    const double g = 9.8;
    const double C = 0.5;

    static void Main(string[] args)
    {
        LevelOne();
    }

    static void LevelOne()
    {
        Logger logger = new Logger("levelone.txt"); //logger both prints nicely to the console and writes to a file for graph making

        const double _m = 3.0;
        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 3.0;

        var v = (x: v0 * Math.Cos(theta), z: v0 * Math.Sin(theta));
        var pos = (x: 0.0, z: 0.0);

        const double dt = 0.1;
        logger.WriteLine("t", "x", "z", "v", "a");

        for (int t = 0; t <= 20 * (int)(1/dt); t++)
        {
            if (t > 0)
            {
                v.z += -g * dt;

                pos.x += v.x * 0.1;
                pos.z += v.z * 0.1;
            }
            logger.WriteLine($"{t * dt:F2}", $"{pos.x:F2}", $"{pos.z:F2}", $"{Math.Sqrt(v.x * v.x + v.z * v.z):F2}", -g);
        }
    }
}