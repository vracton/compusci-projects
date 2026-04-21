using Arena;
using System;
using System.Collections.Generic;
using System.Text;
using VisualizerBaseClasses;

namespace ArenaVisualizer
{
    public class ArenaEngineAdapter : IEngine<ArenaCoreInterface, GraphicTurnAdapter>
    {
        private ArenaEngine engine;

        public ArenaEngineAdapter(ArenaEngine engine)
        {
            this.engine = engine;
        }
        public bool Continue => engine.Continue;

        public double Time => engine.Time;

        public CommandSet<ArenaCoreInterface> Initialization()
        {
            var commands = engine.Initialization();
            return Convert(commands);
        }

        public CommandSet<ArenaCoreInterface> Tick(double newTime)
        {
            var commands = engine.Tick(newTime);
            return Convert(commands);
        }

        private CommandSet<ArenaCoreInterface> Convert(CommandSet<IArenaDisplay> original)
        {
            var response = new CommandSet<ArenaCoreInterface>();
            foreach (var command in original.Commands)
            {
                response.AddCommand(new GraphicTurnAdapter((GraphicTurn)command));
            }
            return response;
        }
    }
}
