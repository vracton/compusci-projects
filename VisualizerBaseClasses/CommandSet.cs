namespace VisualizerBaseClasses
{
    /// <summary>
    /// A collection of VisualizerCommands representing one "turn" for the visualizer
    /// </summary>
    public class CommandSet<TVisualizer>
    {
        public List<ICommand<TVisualizer>> Commands { get; } = [];

        public CommandSet()
        { }

        private static readonly Lock locker = new();

        /// <summary>
        /// Adds a new command to the CommandSet
        /// </summary>
        public void AddCommand(ICommand<TVisualizer> command)
        {
            lock (locker)
            {
                Commands.Add(command);
            }
        }

        /// <summary>
        /// Executes all the commands in the set, in order
        /// </summary>
        /// <param name="visualizer">The Visualizer that is receiving the commands</param>
        public void ProcessAll(TVisualizer visualizer)
        {
            foreach (var command in Commands)
            {
                command.Do(visualizer);
            }
        }

        /// <summary>
        /// Writes the commands to file
        /// </summary>
        /// <param name="bw"></param>
        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(Commands.Count);
            foreach (var command in Commands)
            {
                command.WriteToFile(bw);
            }
        }

        /// <summary>
        /// Reads the commands from a file using a given filereader
        /// </summary>
        public CommandSet(BinaryReader br, ICommandFileReader<TVisualizer> factory)
        {

            int nCommands = br.ReadInt32();
            for (int i = 0; i < nCommands; ++i)
            {
                var newCommand = factory.ReadCommand(br);
                Commands.Add(newCommand);
            }
        }

        static public CommandSet<TVisualizer> operator +(CommandSet<TVisualizer> one,
            CommandSet<TVisualizer> two)
        {
            var response = new CommandSet<TVisualizer>();
            response.Commands.AddRange(one.Commands);
            response.Commands.AddRange(two.Commands);
            return response;
        }
    }
}
