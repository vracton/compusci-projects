using DongUtility;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A kinematics engine that uses adaptive time stepping to ensure accuracy
    /// </summary>
    public class AdaptiveEngine : KinematicsEngine
    {
        public override bool Increment(double timeIncrement)
        {
            // Clear the divisions for counting
            NDivisions = 0;

            try
            {
                SubdivideTrajectory(timeIncrement);
            }
            catch (IllegalPositionException)
            {
                FindEndPoint(timeIncrement);
                return false;
            }

            return CheckStopConditions();
        }

        /// <summary>
        /// The number of subdivisions to make in each time interval for testing
        /// </summary>
        public int NDivisions { get; private set; } = 0;

        /// <summary>
        /// How close an answer must be to the subdivided answer to be considered accurate
        /// </summary>
        public double Tolerance { get; set; } = .001;

        /// <summary>
        /// How precise the time should be to find the end point if a stop condition is triggered
        /// </summary>
        public double TimeTolerance { get; set; } = .001;

        /// <summary>
        /// This is the recursive function that subdivides the trajectory
        /// </summary>
        /// <returns>True if the engine should continue running</returns>
        private bool SubdivideTrajectory(double timeIncrement)
        {
            double diffSquared = TestDifferenceSquared(timeIncrement);
            if (diffSquared < UtilityFunctions.Square(Tolerance))
            {
                return FinalAdaptiveIncrement(timeIncrement);
            }
            else
            {
                ++NDivisions;
                SubdivideTrajectory(timeIncrement / 2);
                return SubdivideTrajectory(timeIncrement / 2);
            }
        }

        /// <summary>
        /// This is the increment function that is called only the last time.
        /// In most cases, should be identical to AdaptiveIncrement()
        /// </summary>
        virtual protected bool FinalAdaptiveIncrement(double timeIncrement)
        {
            return AdaptiveIncrement(timeIncrement);
        }

        /// <summary>
        /// This is the increment funciton that is called every step of the adaptive testing
        /// This is usually the same as FinalAdaptiveIncrement(), but can be different for performance reasons,
        /// so that an expensive operation that doesn't affect particle position can be avoided until the end
        /// </summary>
        /// <param name="timeIncrement"></param>
        /// <returns></returns>
        virtual protected bool AdaptiveIncrement(double timeIncrement)
        {
            return base.Increment(timeIncrement);
        }

        /// <summary>
        /// Finds the maximum difference, squared, between running the engine for one time increment and splitting the interval in half
        /// </summary>
        private double TestDifferenceSquared(double timeIncrement)
        {
            double maxDifference = 0;

            var position1step = TestEngine(timeIncrement, 1);
            var position2step = TestEngine(timeIncrement, 2);

            for (int i = 0; i < Projectiles.Count; ++i)
            {
                double difference = (position1step[i] - position2step[i]).MagnitudeSquared;
                if (difference > maxDifference)
                {
                    maxDifference = difference;
                }
            }

            return maxDifference;
        }

        /// <summary>
        /// Steps the engine forward, to test, in a certain number of steps
        /// </summary>
        /// <returns>A vector of the new positions of the projectiles</returns>
        private List<Vector> TestEngine(double timeIncrement, int nSteps)
        {
            double oldTime = Time;
            var oldPositions = new List<Vector>();
            var oldVelocities = new List<Vector>();
            List<Vector> responses = [];

            foreach (var projectile in Projectiles)
            {
                oldPositions.Add(projectile.Position);
                oldVelocities.Add(projectile.Velocity);
            }

            bool result = RunEngine(timeIncrement, nSteps);
            if (!result)
            {
                for (int i = 0; i < Projectiles.Count; ++i)
                {
                    Projectiles[i].Position = oldPositions[i];
                    Projectiles[i].Velocity = oldVelocities[i];
                }
                Time = oldTime;
                throw new IllegalPositionException();
            }
            else
            {
                foreach (var projectile in Projectiles)
                {
                    responses.Add(projectile.Position);
                }
            }
            // Reset everything
            for (int i = 0; i < Projectiles.Count; ++i)
            {
                Projectiles[i].Position = oldPositions[i];
                Projectiles[i].Velocity = oldVelocities[i];
            }
            Time = oldTime;

            return responses;
        }

        /// <summary>
        /// Finds the final point where the particles end up without triggering a stop condition
        /// </summary>
        private List<Vector> FindEndPoint(double timeIncrement)
        {
            double lowerLimit = 0;
            double upperLimit = double.MaxValue;
            while (true)
            {
                try
                {
                    TestEngine(timeIncrement, 1);
                }
                catch (IllegalPositionException)
                {
                    if (upperLimit > timeIncrement)
                    {
                        upperLimit = timeIncrement;
                    }
                    timeIncrement -= (upperLimit - lowerLimit) / 2;
                    if (upperLimit - lowerLimit < TimeTolerance)
                    {
                        break;
                    }
                    continue;
                }
                if (lowerLimit < timeIncrement)
                {
                    lowerLimit = timeIncrement;
                }
                timeIncrement += (upperLimit - lowerLimit) / 2;
            }
            RunEngine(timeIncrement, 1);
            var responses = new List<Vector>();
            foreach (var projectile in Projectiles)
            {
                responses.Add(projectile.Position);
            }
            return responses;
        }

        /// <summary>
        /// Advances the engine for a given amount of time, split into a given number of steps
        /// <returns>Whether the engine says to continue or not</returns>
        private bool RunEngine(double timeIncrement, int nSteps)
        {
            for (int i = 0; i < nSteps; ++i)
            {
                bool result = AdaptiveIncrement(timeIncrement / nSteps);
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
