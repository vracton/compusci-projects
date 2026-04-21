using DongUtility;
using System.Linq;
using Geometry.Geometry2D;
using NeuralNetStudentVersion;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// A simple hare class that uses a perceptron to determine its movement
    /// </summary>
    class Hare : PredatorPreyOrganism
    {
        internal const double MyWidth = .2;
        internal const double MyLength = .2;
        private const double myMaxSpeed = 6;
        private const double myStepTime = .15;
        private const double myMaxAccel = 2.5;
        private const double initialx = 25;
        private const double initialy = 10;

        private const int graphicsCode = 1;

        public Hare() :
            base(graphicsCode, MyWidth, MyLength, myMaxSpeed, myMaxAccel, myStepTime, new Point(initialx, initialy))
        {
        }

        public Perceptron Perceptron { get; set; } = new SingleLayerPerceptron(4, 2);

        public override string Name => "Hare";

        protected override Vector2D ChooseVelocityChange()
        {
            Perceptron.Reset();

            // Locate the relevant animals
            var lynx = Arena.GetObjectsOfType<Lynx>().First();

            // Perceptron inputs here
            Perceptron.AddInputs(
                Position.X,
                Position.Y,
                lynx.Position.X,
                lynx.Position.Y
            );

            // Run the perceptron and get output
            Perceptron.Run();

            double x = Perceptron.GetOutput(0);
            double y = Perceptron.GetOutput(1);

            if (!UtilityFunctions.IsValid(x) || !UtilityFunctions.IsValid(y))
                return new Vector2D(0, 0);

            return new Vector2D(x, y);
        }
    }
}
