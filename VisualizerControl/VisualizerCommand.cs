using System;
using System.Collections.Generic;
using System.IO;
using VisualizerBaseClasses;
using VisualizerControl.Commands;

namespace VisualizerControl
{
    /// <summary>
    /// An interface for a command issued to the visualizer
    /// </summary>
    abstract public class VisualizerCommand : ICommand<Visualizer>
    {
        public abstract void Do(Visualizer viz);

        /// <summary>
        /// Writes the command to file
        /// </summary>
        public void WriteToFile(BinaryWriter bw)
        {
            CommandType type = enumDictionary[GetType()];
            bw.Write((byte)type);
            WriteContent(bw);
        }

        /// <summary>
        /// Reads the command from file
        /// </summary>
        static public VisualizerCommand ReadFromFile(BinaryReader br)
        {
            byte typeCode = br.ReadByte();
            CommandType type = (CommandType)typeCode;

            // Have to use switch since we can't instantiate from type
            return type switch
            {
                CommandType.AddObject => new AddObject(br),
                CommandType.RemoveObject => new RemoveObject(br),
                CommandType.LookAt => new LookAt(br),
                CommandType.MoveObject => new MoveObject(br),
                CommandType.TransformObject => new TransformObject(br),
                CommandType.ChangeMaterial => new UpdateMaterial(br),
                CommandType.MoveCamera => new MoveCamera(br),
                CommandType.ClearAll => new ClearAll(),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Writes the content of the command to file
        /// </summary>
        abstract protected void WriteContent(BinaryWriter bw);

        private enum CommandType : byte
        {
            AddObject, RemoveObject, MoveObject, TransformObject, ChangeMaterial,
            MoveCamera, ClearAll, LookAt
        }

        private static readonly Dictionary<Type, CommandType> enumDictionary = new()
        {
            { typeof(AddObject), CommandType.AddObject },
            { typeof(RemoveObject), CommandType.RemoveObject },
            { typeof(MoveObject), CommandType.MoveObject },
            { typeof(TransformObject), CommandType.TransformObject },
            { typeof(UpdateMaterial), CommandType.ChangeMaterial },
            { typeof(MoveCamera), CommandType.MoveCamera },
            { typeof(ClearAll), CommandType.ClearAll },
            { typeof(LookAt), CommandType.LookAt },
        };

    }
}
