using GraphData;

namespace VisualizerBaseClasses
{
    /// <summary>
    /// A class that runs a real-time engine and stores the results in a file
    /// </summary>
    /// <typeparam name="TVisualizer">The type of visualizer</typeparam>
    /// <typeparam name="TCommand">The type of commands to send to the visualizer</typeparam>
    /// <typeparam name="TEngine">The type of engine that is running</typeparam>
    public class FileWriter<TVisualizer, TCommand, TEngine>(TEngine engine)
        where TCommand : ICommand<TVisualizer>
        where TEngine : IEngine<TVisualizer, TCommand>
    {
        /// <summary>
        /// The manager for all the graph data
        /// </summary>
        public GraphDataManager Manager { get; } = new GraphDataManager();

        // Just to really try to avoid the weird buffer overrun error we get
        private static double myMaxTime = 0;

        /// <summary>
        /// Runs the engine for the given time, writing the results to a file
        /// </summary>
        /// <param name="messageEvery">Prints a statement to the console at this frequency of events</param>
        public void Run(string filename, double timeStep, double maxTime = double.MaxValue, double messageEvery = double.MaxValue)
        {
            myMaxTime = maxTime; // To avoid the weird buffer overrun error we get

            using var bw = new BinaryWriter(File.OpenWrite(filename));

            var initialSet = engine.Initialization();
            initialSet.WriteToFile(bw);
            Manager.WriteGraphHeader(bw);
            double realTime = maxTime;

            if (messageEvery < double.MaxValue)
            {
                Console.WriteLine("Completed initialization");
            }
            double nextMessageTime = messageEvery;
            while (engine.Continue && engine.Time < maxTime)
            {
                if (engine.Time > nextMessageTime)
                {
                    Console.WriteLine("Reached time " + engine.Time);
                    nextMessageTime += messageEvery;
                }
                // We add some variables here in hopes that the memory error will corrupt these instead of my good local variables
                double oldTime = engine.Time;
                double corruptThis = engine.Time + maxTime;
                double alsoCorruptThis = engine.Time - maxTime;

                double newTime = engine.Time + timeStep;
                var commands = engine.Tick(newTime);
                maxTime = realTime;
                //Console.WriteLine($"Max time: {maxTime} Current time: {engine.Time}");
                var data = Manager.GetData();
                commands.WriteToFile(bw);
                data.WriteData(bw);
                bw.Write(newTime);
                // A bit on whether to continue
                bw.Write(engine.Continue);
                maxTime = myMaxTime; // Reset maxTime to the original value to avoid issues with the loop condition
                if (engine.Time < 0)
                {
                    Console.WriteLine("Time has gone negative!  Stopping.");
                    Console.ReadLine();
                }
            }
            if (messageEvery < double.MaxValue)
            {
                Console.WriteLine("Completed simulation");
            }
            //Console.WriteLine($"Value of engine.Continue: {engine.Continue}   Total time: {engine.Time}   Maxtime:  {maxTime}   Value of nextMessageTime: {nextMessageTime}");
        }
    }
}
