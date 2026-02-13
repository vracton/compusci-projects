using DongUtility;
using System.IO;
using WPFUtility;
using static WPFUtility.UtilityFunctions;

namespace VisualizerControl.Commands
{
    /// <summary>
    /// A command to move the camera to a new position.
    /// </summary>
    public class MoveCamera : VisualizerCommand
    {
        private Vector position;
        private Vector lookDirection;
        private Vector upDirection;

        /// <param name="position">The new position of the camera</param>
        /// <param name="lookDirection">A vector that indicates where the camera is looking</param>
        /// <param name="upDirection">A vector that indicates the direction of up on the viewport</param>
        public MoveCamera(Vector position, Vector lookDirection, Vector upDirection)
        {
            this.position = position;
            this.lookDirection = lookDirection;
            this.upDirection = upDirection;
        }
        public override void Do(Visualizer viz)
        {
            viz.MoveCamera(ConvertToPoint3D(position), ConvertToVector3D(lookDirection), ConvertToVector3D(upDirection));
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(position);
            bw.Write(lookDirection);
            bw.Write(upDirection);
        }

        internal MoveCamera(BinaryReader br)
        {
            position = br.ReadVector();
            lookDirection = br.ReadVector();
            upDirection = br.ReadVector();
        }

    }
}
