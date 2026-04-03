using DongUtility;
using System;

namespace Visualizer.FastestDescent
{
    public class Cubic3DVectorPath : Path
    {
        private readonly Vector point0;
        private readonly Vector point1;
        private readonly Vector tangent0;
        private readonly Vector tangent1;

        public override double InitialParameter => 0;

        public override double FinalParameter => 1;

        public Cubic3DVectorPath(Vector point0, Vector point1, Vector tangent0, Vector tangent1)
        {
            this.point0 = point0;
            this.point1 = point1;
            this.tangent0 = tangent0;
            this.tangent1 = tangent1;
        }

        protected override Vector Function(double parameter)
        {
            //code based off of https://en.wikipedia.org/wiki/Cubic_Hermite_spline#Unit_interval_[0,_1]
            double t = Math.Clamp(parameter, 0, 1);
            double h00 = 2 * t * t * t - 3 * t * t + 1;
            double h10 = t * t * t - 2 * t * t + t;
            double h01 = -2 * t * t * t + 3 * t * t;
            double h11 = t * t * t - t * t;

            return h00 * point0 + h10 * tangent0 + h01 * point1 + h11 * tangent1;
        }
    }
}
