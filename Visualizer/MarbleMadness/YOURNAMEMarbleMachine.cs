using DongUtility;
using System.Windows.Media;

namespace Visualizer.MarbleMadness
{
    // This is your marble machine.  MAKE SURE YOU CHANGE YOUR NAME RIGHT AWAY!
    public class YOURNAMEMarbleMachine : MarbleMachine
    {
       /// <summary>
       /// All the building of the machine should go in the constructor, which must have no arguments
       /// </summary>
        public YOURNAMEMarbleMachine()
        {
            // Create surfaces with different physical characteristics (elasticity and friction)
            var surface1 = new Surface(1, 1);
            // You can add quadrilaterals or triangles to the surface.
            surface1.AddQuad(new Vector(-.5, -.5, 0),
            new Vector(-.5, .5, 0),
            new Vector(.5, .5, -.5),
            new Vector(.5, -.5, -.4),
            Colors.Azure);
            surface1.AddQuad(new Vector(-.5, .5, .5),
            new Vector(-.5, 0, 0),
            new Vector(.5, 0, -.5),
            new Vector(.5, .5, -.5),
            Colors.Firebrick);
            surface1.AddQuad(new Vector(-.5, -.5, -.5), new Vector(-.5, .5, -.5),
                new Vector(.5, .5, -.5), new Vector(.5, -.5, -.5),
                Colors.Fuchsia);
            
            // When you are done with a surface, make sure to add it
            AddSurface(surface1);
        }

        /// <summary>
        ///  Here is where your marble will start
        /// </summary>
        protected override Vector Beginning => new(0, 0, .5);

        /// <summary>
        /// Here is your prediction of where your marble will exit the machine.
        /// </summary>
        protected override Vector Ending => new(.5, 0, -.5);
    }

}
