using Arena;
using System.IO;
using VisualizerBaseClasses;

namespace ArenaVisualizer
{
    /// <summary>
    /// A class that reads arena visualization commands from a file
    /// </summary>
    public class ArenaCommandFileReader(Registry registry) : ICommandFileReader<ArenaCoreInterface>
    {
        /// <summary>
        /// Reads a single command from a file.
        /// </summary>
        public ICommand<ArenaCoreInterface> ReadCommand(BinaryReader br)
        {
            var turn = GraphicTurn.ReadFromFile(br, registry);
            return new GraphicTurnAdapter(turn);
        }
    }
}
