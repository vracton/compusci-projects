using DongUtility;
using System.IO;
using static WPFUtility.UtilityFunctions;

namespace VisualizerControl.Commands
{
    /// <summary>
    /// A command to move the camera to a new position, looking at a specific point.
    /// </summary>
    public class LookAt : VisualizerCommand
    {
        private Vector newPosition;
        private Vector target;
        private Vector newUpDirection;

        /// <param name="newPosition">The new position of the camera</param>
        /// <param name="target">The point the camera is looking at</param>
        /// <param name="newUpDirection">The new up direction of the camera</param>
        public LookAt(Vector newPosition, Vector target, Vector newUpDirection)
        {
            this.newPosition = newPosition;
            this.target = target;
            this.newUpDirection = newUpDirection;
        }
        public override void Do(Visualizer viz)
        {
            viz.LookAt(ConvertToPoint3D(newPosition), 
                ConvertToPoint3D(target), 
                ConvertToVector3D(newUpDirection));
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            bw.Write(newPosition);
            bw.Write(target);
            bw.Write(newUpDirection);
        }

        internal LookAt(BinaryReader br)
        {
            newPosition = br.ReadVector();
            target = br.ReadVector();
            newUpDirection = br.ReadVector();
        }

    }
}
