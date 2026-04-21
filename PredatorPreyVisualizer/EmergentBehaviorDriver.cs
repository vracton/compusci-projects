using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredatorPreyVisualizer
{
    internal class EmergentBehaviorDriver
    {
        static internal void RunEmergentBehavior()
        {
            double xSize = 100;
            double ySize = 100;
            int nHares = 10;
            double timeStep = .1;
            var engine = new EmergentBehaviorEngine(xSize, ySize, nHares);
            engine.Initialize();

            // Create the main window and show it
            var window = new MainWindow(timeStep, engine);
            window.Show();
        }
    }
}
