using DongUtility;
using Geometry.Geometry3D;

namespace PhysicsUtility.Kinematics.Forces
{
    /// <summary>
    /// A force that acts on a projectile when it is in contact with an extended object and keeps the projectile from getting inside the object
    /// </summary>
    public class ExtendedVersusPointContactForce(ContinuousExtendedProjectile projectile, KinematicsEngine engine) : Force
    {
        private readonly ContinuousExtendedProjectile mainProjectile = projectile;

        public override void AddForce(double deltaTime)
        {
            foreach (var projectile in engine.Projectiles)
            {
                if (projectile == mainProjectile)
                    continue;

                var predictedPosition = PredictPosition(projectile, deltaTime);
                if (mainProjectile.Shape.IsInside(predictedPosition))
                {
                    var force = ExpulsiveForce(projectile, predictedPosition, deltaTime);
                    projectile.AddForce(force);
                    mainProjectile.AddForce(-force);
                }
            }
        }

        /// <summary>
        /// Figures out where the projectile will be in the next time step
        /// </summary>
        private static Point PredictPosition(Projectile projectile, double deltaTime)
        {
            Vector velocity = projectile.Velocity + projectile.Acceleration * deltaTime;
            Vector position = projectile.Position + velocity * deltaTime;
            return position.ToPoint();
        }

        /// <summary>
        /// Calculates the force needed to keep the projectile outside of the extended object
        /// </summary>
        private Vector ExpulsiveForce(Projectile projectile, Point predictedPosition, double deltaTime)
        {
            var pointingLine = new Line(predictedPosition, projectile.Position.ToPoint());
            var intersections = mainProjectile.Shape.Intersection(pointingLine);
            var intersection = ClosestIntersection(projectile.Position.ToPoint(), intersections);
            var targetVector = intersection - projectile.Position.ToPoint();
            return ExplusiveForce(projectile, targetVector, deltaTime);
        }

        /// <summary>
        /// Finds the closest intersection point to the current position
        /// </summary>
        /// <param name="intersections">All possible intersection points to consider</param>
        private static Point ClosestIntersection(Point currentPosition, IEnumerable<Point> intersections)
        {
            double minDistance = double.MaxValue;
            Point closest = new();
            foreach (var point in intersections)
            {
                double distanceSquared = (point - currentPosition).MagnitudeSquared;
                if (distanceSquared < minDistance)
                {
                    minDistance = distanceSquared;
                    closest = point;
                }
            }
            return closest;
        }

        /// <summary>
        /// Calculates the force needed to move the projectile to the target position
        /// </summary>>
        private static Vector ExplusiveForce(Projectile projectile, Vector target, double deltaTime)
        {
            // Adjust for current acceleration
            Vector accelerationAdjustment = -projectile.Acceleration * projectile.Mass;

            // Adjust for current velocity
            Vector velocityAdjustment = -projectile.Velocity * projectile.Mass / deltaTime;

            // Adjust for current position
            Vector positionAdjustment = target * projectile.Mass / UtilityFunctions.Square(deltaTime);

            return accelerationAdjustment + velocityAdjustment + positionAdjustment;
        }
    }
}
