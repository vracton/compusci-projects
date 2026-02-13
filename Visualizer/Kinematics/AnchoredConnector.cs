using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// A Connector that connects a fixed point to a Projectile
    /// </summary>
    class AnchoredConnector(double radius, Color color, Vector3D anchor, IProjectile projectile) : Connector(radius, color)
    {
        protected override Vector3D Point1 => anchor;

        protected override Vector3D Point2 => projectile.Position;
    }
}
