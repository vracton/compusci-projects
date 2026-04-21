using Arena.GraphicTurns;
using VisualizerBaseClasses;

namespace Arena
{
    /// <summary>
    /// Base class for all graphic turns. Graphic turns are the commands that
    /// cause changes to the visualization.
    /// </summary>
    abstract public class GraphicTurn : ICommand<IArenaDisplay>
    {
        public GraphicTurn()
        { }

        /// <summary>
        /// Executes the graphic turn on the display.
        /// </summary>
        abstract public void Do(IArenaDisplay display);

        /// <summary>
        /// All possible types of graphic turns. Needs to updated for each new one :(
        /// Used for reading/writing to file.
        /// </summary>
        protected enum GraphicTurnTypes : byte { SetWindowDimensions, AddObject, RemoveObject, MoveObject, RotateObject, ChangeObjectGraphic };

        /// <summary>
        /// Writes the specific content of the graphic turn to a writer.
        /// </summary>
        abstract protected void WriteContent(BinaryWriter bw);

        /// <summary>
        /// The type of graphic turn. Used for reading/writing to file.
        /// </summary>
        abstract protected GraphicTurnTypes GraphicType { get; }

        /// <summary>
        /// Writes the graphic turn to a file. The first byte is the type of graphic turn.
        /// </summary>
        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write((byte)GraphicType);
            WriteContent(bw);
        }

        /// <summary>
        /// Reads a graphic turn from a file.
        /// </summary>
        /// <param name="registry">The registry associated with this arena</param>
        static public GraphicTurn ReadFromFile(BinaryReader br, Registry registry)
        {
            byte typeCode = br.ReadByte();
            GraphicTurnTypes type = (GraphicTurnTypes)typeCode;

            return type switch
            {
                GraphicTurnTypes.AddObject => new AddObject(br, registry),
                GraphicTurnTypes.MoveObject => new MoveObject(br),
                GraphicTurnTypes.RotateObject => new RotateObject(br),
                GraphicTurnTypes.RemoveObject => new RemoveObject(br),
                GraphicTurnTypes.SetWindowDimensions => new SetWindowDimensions(br),
                GraphicTurnTypes.ChangeObjectGraphic => new ChangeObjectGraphic(br),
                _ => throw new NotImplementedException("Should never reach here"),
            };
        }
    }
}
