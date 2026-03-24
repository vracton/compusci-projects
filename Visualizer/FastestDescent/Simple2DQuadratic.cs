using DongUtility;
using static DongUtility.UtilityFunctions;

namespace Visualizer.FastestDescent
{
    public class Simple2DQuadratic(double initial, double final, params double[] parameters) : Path
    {
        public override double InitialParameter { get; } = initial;

        public override double FinalParameter { get; } = final;

        protected override Vector Function(double parameter)
        {
            double z = parameters[0] + parameters[1] * parameter + parameters[2] * Square(parameter);
            return new Vector(parameter, 0, z);
        }
    }
}
