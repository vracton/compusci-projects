using System.IO;

namespace VisualizerControl.Commands
{
    /// <summary>
    /// A command to clear all objects from the visualizer.
    /// </summary>
    public class ClearAll : VisualizerCommand
    {
        public override void Do(Visualizer viz)
        {
            viz.Clear();
        }

        protected override void WriteContent(BinaryWriter bw)
        {
            // Has no extra data to add
        }
    }
}
