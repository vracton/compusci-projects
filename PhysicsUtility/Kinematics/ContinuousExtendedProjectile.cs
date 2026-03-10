using DongUtility;
using Geometry.Geometry3D;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A shape that is treated as a continuous object with uniform density (override to change that)
    /// </summary>
    public class ContinuousExtendedProjectile(Vector position, Vector velocity, double mass, Shape3D shape) 
        : ExtendedProjectile(position, velocity, mass)
    {
        public Shape3D Shape { get; } = shape;

        public override Matrix InertialTensorInverse => Shape.InertialTensor.Inverse();

        public override Vector CenterOfMass => Shape.CenterOfMass.PositionVector();
    }
}
