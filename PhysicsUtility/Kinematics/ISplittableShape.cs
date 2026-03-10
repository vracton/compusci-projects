using Geometry.Geometry3D;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A shape that can be split into some number of objects that are themselves ISplittableShapes
    /// </summary>
    public interface ISplittableShape
    {
        public IEnumerable<ISplittableShape> SplitShapes();
        public Point CenterOfMass { get; }
        public double Mass { get; }
    }
}
