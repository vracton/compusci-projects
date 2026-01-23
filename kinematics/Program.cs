class Program
{
    const double g = 9.8;
    const double C = 0.5;

    static void Main(string[] args)
    {
        Challenge();
    }

    static void LevelOne()
    {
        const double v = 4.0;
        double z = 0.0;

        Console.WriteLine("time\tpos\tvel");
        for (int i = 0; i <= 200; i+=1)
        {
            z = v * i * 0.1;
            Console.WriteLine($"{i * 0.1:F1}\t{z:F1}\t{v}");
        }
    }

    static void LevelTwo()
    {
        double v = 3.0;
        double z = 0.0;

        Console.WriteLine("time\tpos\tvel\ta");
        for (int i = 0; i <= 200; i += 1)
        {
            if (i > 0)
            {
                v += -g * 0.1;
                z += v * 0.1;
            }
            Console.WriteLine($"{i * 0.1:F1}\t{z:F1}\t{v:F2}\t{-g}");
        }
    }

    static void LevelThree()
    {
        const double m = 3.0;
        double v = 4.0;
        double z = 0.0;

        Console.WriteLine("time\tpos\tvel\ta");
        for (int i = 0; i <= 200; i += 1)
        {
            double aDrag = -C * v * Math.Abs(v) / m;

            if (i > 0)
            {
                v += (-g + aDrag) * 0.1;
                z += v * 0.1;
            }

            Console.WriteLine($"{i * 0.1:F1}\t{z:F1}\t{v:F2}\t{(-g+aDrag):F2}");
        }
    }

    static void Challenge()
    {
        const double m = 3.0;
        const double k = 9.0;
        double v = 120.0;
        double z = -4.0;

        Console.WriteLine("time\tpos\tvel\ta");
        for (int i = 0; i <= 200; i += 1)
        {
            double fGrav = m * -g;
            double fDrag = -C * v * Math.Abs(v);
            double fSpring = -(z + 3) * k;
            double fNet = fGrav + fDrag + fSpring;

            if (i > 0)
            {
                v += (fNet / m) * 0.1;
                z += v * 0.1;
            }

            Console.WriteLine($"{i * 0.1:F1}\t{z:F1}\t{v:F2}\t{(fNet / m):F2}");
        }
    }
}