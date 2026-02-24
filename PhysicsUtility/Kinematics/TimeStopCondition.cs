namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A stop condition that stops the engine after a certain amount of time
    /// </summary>
    public class TimeStopCondition(double endTime) : StopCondition
    {
        public override bool ShouldContinue(KinematicsEngine engine)
        {
            return engine.Time < endTime;
        }
    }
}
