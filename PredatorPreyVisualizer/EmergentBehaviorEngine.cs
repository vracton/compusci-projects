using Arena;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PredatorPreyVisualizer
{
    internal class EmergentBehaviorEngine : ArenaEngine
    {
        public List<Hare> Hares { get; private set; } = [];
        public List<Lynx> Lynxes { get; private set; } = [];

        public EmergentBehaviorEngine(double xSize, double ySize, int nHares) :
            base(xSize, ySize, "dirt.jpg")
        {
            if (nHares <= 0)
            {
                throw new ArgumentException("Invalid number of hares");
            }

            for (int i = 0; i < nHares; ++i)
            {
                var hare = new IntelligentHare();
                Hares.Add(hare);
            }

            AddObjectRandom<IntelligentHare>(nHares);
        }

        /// <summary>
        /// The factor by which the display size of the animals is larger than their logical size
        /// </summary>
        public double SizeScale { get; set; } = 6;

        public override void Initialize()
        {
            Registry.Initialize(@"PredatorPreyVisualizer\", @"Images\");

            Registry.AddEntry(new GraphicInfo("hare.jpg", Hare.MyWidth * SizeScale, Hare.MyLength * SizeScale));
        }
    }
}
