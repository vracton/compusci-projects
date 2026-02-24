using PhysicsUtility.Kinematics;
using System;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// Stops when all projectiles leave a cube of a given size.
    /// Assumes a cube centered at the origin.
    /// </summary>
    /// <param name="max">The maximum absolute value of the position coordinate</param>
    internal class CubeExitStopCondition(double max) : StopCondition
    {
        public override bool ShouldContinue(KinematicsEngine engine)
        {
            foreach (var projectile in engine.Projectiles)
            {
                var position = projectile.Position;
                if (Math.Abs(position.X) < max
                    && Math.Abs(position.Y) < max
                    && Math.Abs(position.Z) < max)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
