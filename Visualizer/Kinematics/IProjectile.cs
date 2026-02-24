using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// Interface for a projectile to be represented in a KinematicsVisualization
    /// </summary>
    public interface IProjectile
    {
        /// <summary>
        /// The current position of the particle, as a Vector3D.
        /// </summary>
        Vector3D Position { get; }
        
        /// <summary>
        /// The output color of the projectile on the screen.  Your choice of color!
        /// </summary>
        Color Color { get; }

        /// <summary>
        /// The shape of the object, using prototypes derived from Shape3D.
        /// If you don't care, type new Sphere3D();
        /// </summary>
        VisualizerControl.Shapes.Shape3D Shape { get; }

        /// <summary>
        /// Returns the radius of the projectile visualization.
        /// Has no effect on behavior - purely visual.
        /// It is a radius, so a sphere with Size = 1 will have radius 1, or diameter 2.
        /// </summary>
        double Size { get; }
    }
}
