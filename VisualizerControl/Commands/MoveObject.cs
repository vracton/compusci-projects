using DongUtility;
using System;
using System.IO;
using System.Windows.Media.Media3D;
using WPFUtility;
using static WPFUtility.UtilityFunctions;

namespace VisualizerControl.Commands
{
    /// <summary>
    /// A command to move an existing object to a new position
    /// The object is accessed by index.
    /// </summary>
    public class MoveObject : VisualizerCommand
    {
        public Vector3D NewPosition { get; }
        public int Index { get; }

        public MoveObject(int index, Vector3D newPosition)
        {
            if (!newPosition.IsValid())
                throw new ArgumentException("Infinity or not-a-number passed into MoveObject!");

            this.NewPosition = newPosition;
            this.Index = index;
        }

        public MoveObject(int index, Vector newPosition) :
            this(index, ConvertToVector3D(newPosition))
        { }

        public MoveObject(int index, double x, double y, double z) :
            this(index, new Vector3D(x, y, z))
        {}

        public override void Do(Visualizer viz)
        {
            viz.MoveParticle(Index, NewPosition);
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(Index);
            bw.Write(NewPosition);
        }

        internal MoveObject(BinaryReader br)
        {
            Index = br.ReadInt32();
            NewPosition = br.ReadVector3D();
        }
    }
}
