using DongUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredatorPreyVisualizer
{
    internal class IntelligentHare : Hare
    {
        public override string Name => "IntelligentHare";
        protected override Vector2D ChooseVelocityChange()
        {
            // Implement the logic for the intelligent hare's movement

            // How to find other hares:
            var otherHares = Arena.GetObjectsOfType<Hare>();

            // How to head toward another hare
            return otherHares.First().Position - Position;

        }
    }
}
