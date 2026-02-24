using DongUtility;
using System.Collections.Generic;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// A marble machine base class
    /// </summary>
    public abstract class MarbleMachine
    {
        /// <summary>
        /// All the surfaces in the machine
        /// </summary>
        public List<Surface> Surfaces { get; } = [];

        public void AddSurface(Surface surface)
        {
            Surfaces.Add(surface);
        }

        internal Vector GetBeginning => Beginning;
        internal Vector GetEnding => Ending;

        /// <summary>
        /// The point at which the marble starts in the machine.  The z component must be 0.5, and the x and y components must be between -.5 and .5.
        /// </summary>
        abstract protected Vector Beginning { get; }

        /// <summary>
        /// The point at which you predict the marble will exit the machine.  There are no restrictions on this point except that it lie on the boundary
        /// </summary>
        abstract protected Vector Ending { get; }
    }
}
