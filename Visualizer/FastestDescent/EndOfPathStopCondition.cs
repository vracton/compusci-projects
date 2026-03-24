using PhysicsUtility.Kinematics;

namespace Visualizer.FastestDescent
{
    internal class EndOfPathStopCondition : StopCondition
    {
        public override bool ShouldContinue(KinematicsEngine engine)
        {
            foreach (var projectile in engine.Projectiles)
            {
                if (projectile is ConstrainedProjectile constrainedProj)
                {
                    if (constrainedProj.Parameter >= constrainedProj.Path.FinalParameter)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
