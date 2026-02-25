using PhysicsUtility.Kinematics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualizerControl.Shapes;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// An adapter class to get Projectile to be an IProjectile
    /// </summary>
    class ProjectileAdapter(Projectile projectile, double size = .5) : IProjectile
    {
        public Vector3D Position => new(projectile.Position.X, projectile.Position.Y, projectile.Position.Z);

        public Color Color { get; set; } = WPFUtility.UtilityFunctions.ConvertColor(projectile.Color);

        public Shape3D Shape => new Sphere3D(2);

        public double Size { get; } = size;
    }
}
