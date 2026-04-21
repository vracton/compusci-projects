using Arena;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VisualizerBaseClasses;

namespace ArenaVisualizer
{
    public class GraphicTurnAdapter : ICommand<ArenaCoreInterface>
    {
        private readonly GraphicTurn turn;

        public GraphicTurnAdapter(GraphicTurn turn)
        {
            this.turn = turn;
        }

        public void Do(ArenaCoreInterface viz)
        {
            turn.Do(viz);
        }

        public void WriteToFile(BinaryWriter bw)
        {
            turn.WriteToFile(bw);
        }
    }
}
