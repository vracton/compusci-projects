using DongUtility;

namespace Visualizer.FastestDescent
{
    public class CubicPath : Path
    {
        private readonly double a;
        private readonly double b;
        private readonly double c;
        private readonly double d;
        private readonly double xStart;

        public override double InitialParameter { get; }

        public override double FinalParameter { get; }

        public CubicPath(double x0, double z0, double x1, double z1, double m0, double m1)
        {
            double h = x1 - x0;
            xStart = x0;
            InitialParameter = x0;
            FinalParameter = x1;

            a = z0;
            b = m0;
            c = 3 * (z1 - z0) / (h * h) - (2 * m0 + m1) / h;
            d = 2 * (z0 - z1) / (h * h * h) + (m0 + m1) / (h * h);
        }

        protected override Vector Function(double parameter)
        {
            double t = parameter - xStart;
            double z = a + b * t + c * t * t + d * t * t * t;
            return new Vector(parameter, 0, z);
        }
    }
}
