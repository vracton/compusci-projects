using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// A Connector that connects two projectiles
    /// </summary>
    class TwoProjectileConnector : Connector
    {
        public IProjectile Projectile1 { get; }
        public IProjectile Projectile2 { get; }

        public TwoProjectileConnector(double radius, Color color, IProjectile proj1, IProjectile proj2) :
            base(radius, color)
        {
            Projectile1 = proj1;
            Projectile2 = proj2;
        }

        protected override Vector3D Point1 => Projectile1.Position;

        protected override Vector3D Point2 => Projectile2.Position;
    }
}
