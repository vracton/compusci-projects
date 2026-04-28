using DongUtility;
using System;
using System.Collections.Generic;
using System.Text;
using static DongUtility.UtilityFunctions;

namespace Visualizer.FastestDescent
{
    public class Simple2DQuadratic : Path
    {
        private double[] parameters;

        public Simple2DQuadratic(double initial, double final, params double[] parameters)
        {
            this.parameters = parameters;
            InitialParameter = initial;
            FinalParameter = final;
        }
        public override double InitialParameter { get; }

        public override double FinalParameter { get; }

        protected override Vector Function(double parameter)
        {
            double z = parameters[0] + parameters[1] * parameter + parameters[2] * Square(parameter);
            return new Vector(parameter, 0, z);
        }
    }
}
