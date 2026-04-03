using DongUtility;

namespace Visualizer.FastestDescent
{
    public class Cubic3DPath : Path
    {
        private readonly double xStart;
        private readonly double yA;
        private readonly double yB;
        private readonly double yC;
        private readonly double yD;
        private readonly double zA;
        private readonly double zB;
        private readonly double zC;
        private readonly double zD;

        public override double InitialParameter { get; }

        public override double FinalParameter { get; }

        public Cubic3DPath(Vector point0, Vector point1, double ySlope0, double ySlope1, double zSlope0, double zSlope1)
        {
            double x0 = point0.X;
            double x1 = point1.X;
            double h = x1 - x0;

            xStart = x0;
            InitialParameter = x0;
            FinalParameter = x1;

            yA = point0.Y;
            yB = ySlope0;
            yC = 3 * (point1.Y - point0.Y) / (h * h) - (2 * ySlope0 + ySlope1) / h;
            yD = 2 * (point0.Y - point1.Y) / (h * h * h) + (ySlope0 + ySlope1) / (h * h);

            zA = point0.Z;
            zB = zSlope0;
            zC = 3 * (point1.Z - point0.Z) / (h * h) - (2 * zSlope0 + zSlope1) / h;
            zD = 2 * (point0.Z - point1.Z) / (h * h * h) + (zSlope0 + zSlope1) / (h * h);
        }

        protected override Vector Function(double parameter)
        {
            double t = parameter - xStart;
            double y = yA + yB * t + yC * t * t + yD * t * t * t;
            double z = zA + zB * t + zC * t * t + zD * t * t * t;
            return new Vector(parameter, y, z);
        }
    }
}
