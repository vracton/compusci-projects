using NeuralNetStudentVersion;

namespace PredatorPreyVisualizer
{
    internal class Driver
    {
        private const double xSize = 100;
        private const double ySize = 100;
        private const double timeStep = .01;

        static internal void Run()
        {
            WPFUtility.ConsoleManager.ShowConsoleWindow();
            Process();
            Display();
        }

        static private Perceptron bestPerceptron = new SingleLayerPerceptron(4, 2);
        //static private Perceptron bestPerceptron = Perceptron.ReadFromFile(@"C:\Users\pdong\OneDrive\IMSA\Classes\CompuSci\Spring 2023\testPerceptron.pcp");

        static private void Process()
        {
            // Here you can make a new perceptron
            var perceptron = new SingleLayerPerceptron(4, 2);

            // Here is where you do stuff to the Perceptron
            perceptron.RandomWeights(5);

            // Here is how you copy a Perceptron
            var clone = perceptron.Clone();

            // Here is how you make a copy with random variation in weights
            var randomClone = perceptron.RandomClone(4);

            // This is how you run the simulation
            double timeToDie = RunArena(perceptron);

            // And if you like how things went, this is how to set the perceptron
            bestPerceptron = perceptron.Clone();

            // You can save the best perceptron like this
            //bestPerceptron.WriteToFile(@"C:\Users\pdong\OneDrive - imsa.edu\IMSA\Classes\CompuSci\Spring 2025\testPerceptron.pcp");
        }
        static private double RunArena(Perceptron perceptron)
        {
            var arena = new PredatorPreyEngine(xSize, ySize, perceptron.Clone());
            bool keepRunning = true;
            while (keepRunning)
            {
                arena.Tick(arena.Time + timeStep);
                keepRunning = !arena.Hares[0].IsDead;
            }
            return arena.Time;
        }

        static private void Display()
        {
            var window = new MainWindow(xSize, ySize, timeStep, bestPerceptron);
            window.Show();
        }
    }
}
