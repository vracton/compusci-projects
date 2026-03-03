using GraphData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    abstract public class FileModifier<TVisualizer, TCommand> where TCommand : ICommand<TVisualizer>
    {
        private FromFileEngineCore<TVisualizer, TCommand> engine;
        private GraphDataManager manager = new();

        public FileModifier(string filename )
        {
            

            
        }

        public void Run(string inputFile, string outputFile, ICommandFileReader<TVisualizer> factory)
        {
            using var br = new BinaryReader(File.OpenRead(inputFile));
            var initialSet = new CommandSet<TVisualizer>(br, factory);
            var graphInterface = new FileGraphDataInterface(br);

            using var bw = new BinaryWriter(File.OpenWrite(outputFile));
            initialSet.WriteToFile(bw);
            GraphDataManager manager = new();
            manager.WriteGraphHeader(bw);

        }

        virtual protected void ModifyInitialization(CommandSet<TVisualizer> commands, GraphDataPacket data)
        {}

        abstract protected void Modify(CommandSet<TVisualizer> commands, GraphDataPacket data, double time);
    }
}
