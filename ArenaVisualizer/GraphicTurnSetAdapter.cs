using Arena;
using System;
using System.Collections.Generic;
using System.Text;
using VisualizerBaseClasses;

namespace ArenaVisualizer
{
    public class GraphicTurnSetAdapter : CommandSet<ArenaCoreInterface>
    {
        public GraphicTurnSetAdapter(GraphicTurnSet turns)
        {
            foreach (var turn in turns.Commands)
            {
                AddCommand(new GraphicTurnAdapter((GraphicTurn)turn));
            }
        }
    }
}
