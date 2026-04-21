using Arena;
using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Geometry2D;
using NeuralNetStudentVersion;

namespace PredatorPreyVisualizer
{
    /// <summary>
    /// PredatorPreyEngine is the main engine for the predator-prey simulation
    /// </summary>
    internal class PredatorPreyEngine : ArenaEngine
    {
        public List<Hare> Hares { get; private set; } = [];
        public List<Lynx> Lynxes { get; private set; } = [];

        public PredatorPreyEngine(double xSize, double ySize, Perceptron perceptron, int nHares = 1, int nLynxes = 1) :
            base(xSize, ySize, "dirt.jpg")
        {
            if (nHares <= 0 || nLynxes <= 0)
            {
                throw new ArgumentException("Invalid number of hares or lynxes");
            }

            for (int i = 0; i < nHares; ++i)
            {
                var hare = new Hare
                {
                    Perceptron = perceptron
                };
                Hares.Add(hare);
            }
            for (int i = 0; i < nLynxes; ++i)
            {
                var lynx = new Lynx();
                Lynxes.Add(lynx);
            }

            double initialHareX = Width / (nHares + 1);
            for (int i = 0; i < nHares; ++i)
            {
                AddObject(Hares[i], new Point(initialHareX + i * initialHareX, 2 * Height / 3));
            }

            AddObject(Lynxes[0], new Point(Width / 2, Height / 3));

            // This is how you add an obstacle
            //AddObjectRandom(new Obstacle(this, ObstacleSize, ObstacleSize));
        }

        /// <summary>
        /// The factor by which the display size of the animals is larger than their logical size
        /// </summary>
        public double SizeScale { get; set; } = 6;

        /// <summary>
        /// The size of one side of an obstacle
        /// </summary>
        public double ObstacleSize { get; set; } = 20;

        public override void Initialize()
        {
            Registry.Initialize(@"PredatorPreyVisualizer\", @"Images\");

            Registry.AddEntry(new GraphicInfo("hare.jpg", Hare.MyWidth * SizeScale, Hare.MyLength * SizeScale));
            Registry.AddEntry(new GraphicInfo("lynx.jpg", Lynx.MyWidth * SizeScale, Lynx.MyWidth * SizeScale));
            Registry.AddEntry(new GraphicInfo("rock.jpg", ObstacleSize, ObstacleSize));
        }

        protected override void UserDefinedEndOfTurn()
        {
            base.UserDefinedEndOfTurn();
            foreach (var hare in GetObjectsOfType<Hare>())
            {
                if (hare.IsDead)
                {
                    RemoveObjectDelay(hare);
                }
            }
        }

        protected override bool Done()
        {
            return Hares.All((x) => x.IsDead);
        }
    }
}
