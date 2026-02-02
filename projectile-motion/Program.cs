using Helpers;
using Engine.Core;
using Engine.Forces;

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

        World world = new();

        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 4.0;

        var projV = new Vector(v0 * Math.Cos(theta), 0, v0 * Math.Sin(theta));
        Projectile p = new(3.0, new(), projV);
        world.AddProjectile(p);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        while (p.Position.Z >= 0.0)
        {
            world.AddForce(new Gravity(g));

            world.Tick(dt, () =>
            {
                logger.WriteLine(world.Time, p.Position.X, p.Position.Z, p.Velocity.Magnitude, p.Acceleration.Magnitude); //auto-rounds to 3 places
            });
        }
    }

    static void LevelTwo()
    {
        Logger logger = new Logger("leveltwo.txt");

        World world = new();

        const double theta = 45.0 * (Math.PI / 180.0);
        const double v0 = 4.0;

        var projV = new Vector(v0 * Math.Cos(theta), 0, v0 * Math.Sin(theta));
        Projectile p = new(3.0, new(), projV);
        world.AddProjectile(p);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "z", "v", "a");

        while (p.Position.Z >= 0)
        {
            world.AddForces(new Gravity(g), new Drag(C));

            world.Tick(dt, () =>
            {
                logger.WriteLine(world.Time, p.Position.X, p.Position.Z, p.Velocity.Magnitude, p.Acceleration.Magnitude);
            });
        }
    }

    static void LevelThree()
    {
        Logger logger = new Logger("levelthree.txt");

        const double m = 3.0;
        const double k = 9.0;
        const double L = 3.0;

        World world = new();

        Projectile p = new Projectile(m, new(-1.0, 1.0, -3.0), new(5.0, -1.0, -3.0));
        world.AddProjectile(p);

        const double dt = 0.01;
        logger.WriteLine("t", "x", "y", "z", "v", "a");

        double lastTimeG1 = 0; //last tick where velocity was greater than one

        while (world.Time <= 20.01) // 20 is abitrary, but worked out
        {
            world.AddForces(new Gravity(g), new Drag(C));
            p.ApplyForce(new Spring(new(), k, L));

            world.Tick(dt, () =>
            {
                logger.WriteLine(world.Time, p.Position, p.Velocity.Magnitude, p.Acceleration.Magnitude);
            });

            if (p.Velocity.Magnitude >= 1)
            {
                lastTimeG1 = world.Time;
            }

        }

        logger.WriteLine($"last time above 1 m/s was {lastTimeG1:F2}");
    }
}