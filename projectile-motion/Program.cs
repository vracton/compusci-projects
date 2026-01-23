using Logging;
using System.IO;

class ProjectileMotion
{
    const double g = 9.8;
    const double C = 0.5;

    static void Main(string[] args)
    {
        LevelTwo();
    }

    static void LevelOne()
    {
        Logger logger = new Logger("levelone.txt"); //logger both prints nicely to the console and writes to a file for graph making

        const double _m = 3.0;
        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 3.0;

        var v = (x: v0 * Math.Cos(theta), z: v0 * Math.Sin(theta));
        var pos = (x: 0.0, z: 0.0);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        for (int t = 0; t <= 20 * (int)(1 / dt); t++)
        {
            if (t > 0)
            {
                v.z += -g * dt;

                pos.x += v.x * dt;
                pos.z += v.z * dt;
            }
            logger.WriteLine((t * dt), pos.x, pos.z, Math.Sqrt(v.x * v.x + v.z * v.z), -g); //auto-rounds to 3 places
        }
    }

    static void LevelTwo()
    {
        Logger logger = new Logger("leveltwo.txt");

        const double m = 3.0;
        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 3.0;

        var v = (x: v0 * Math.Cos(theta), z: v0 * Math.Sin(theta));
        var pos = (x: 0.0, z: 0.0);

        const double dt = 0.1;
        logger.WriteLine("t", "x", "z", "v", "a");

        for (int t = 0; t <= 20 * (int)(1 / dt); t++)
        {
            double vTotal = Math.Sqrt(v.x * v.x + v.z * v.z);
            double dragAccel = C * vTotal * vTotal / m;
            
            //calculate drag angle and normalize to [0,2pi)
            double dragTheta = Math.Atan2(v.z, v.x) + Math.PI;
            if (dragTheta < 0)
            {
                dragTheta += 2 * Math.PI;
            }
            else if (dragTheta >= 2  * Math.PI)
            {
                dragTheta -= 2 * Math.PI;
            }

            var a = (x: dragAccel * Math.Cos(dragTheta), z: dragAccel * Math.Sin(dragTheta) - g); //calculate acceleration on each direction

            if (t > 0)
            {
                v.x += a.x * dt;
                v.z += a.z * dt;

                pos.x += v.x * dt;
                pos.z += v.z * dt;
            }
            logger.WriteLine(t * dt, pos.x, pos.z, Math.Sqrt(v.x * v.x + v.z * v.z), Math.Sqrt(a.x * a.x + a.z * a.z));
        }
    }
}