using DongUtility;
using System;
using System.Collections.Generic;
using static DongUtility.UtilityFunctions;
using PhysicsUtility.Kinematics.Forces;
using PhysicsUtility.Kinematics;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// A force simulating a set of surfaces that does not let particles go through.
    /// This must be the last of the set of forces to work properly.
    /// This is hardly ideal, but is the best I have right now.
    /// </summary>
    /// <param name="engine">
    /// The kinematics engine, mainly for the DeltaTime feature
    /// </param>
    class SurfaceForce(KinematicsEngine engine) : GlobalForce(engine)
    {
        /// <summary>
        /// All the surfaces in use
        /// </summary>
        private readonly List<Surface> surfaces = [];

        /// <summary>
        /// Finds the normal force on a particle that is striking a triangle,
        /// given an elasticity.
        /// The perpendicular component of velocity should already have been calculated.
        /// </summary>
        private Vector CalcNormalForce(Projectile projectile, Vector perpendicularVelocity,
            Triangle triangle, double elasticity)
        {
            Vector newPerpendicularVelocity = -perpendicularVelocity * elasticity;
            Vector deltaVPerp = newPerpendicularVelocity - perpendicularVelocity;

            Vector accelerationPerp = deltaVPerp / DeltaTime;

            return projectile.Mass * accelerationPerp + NormalForceToCancel(projectile, triangle);
        }

        /// <summary>
        /// Finds the force of friction on an object, given a friction coefficient and an already calculated normal force.
        /// The parallel component of velocity is also assumed to have been calculated already
        /// </summary>
        private Vector CalcFrictionForce(Projectile projectile, Vector parallelVelocity,
            double frictionCoefficient, Vector normalForce)
        {
            Vector frictionForce = -normalForce.Magnitude * frictionCoefficient * parallelVelocity.UnitVector();
            Vector predictedVelocityParallel = PredictVelocity(projectile, frictionForce).ProjectOnto(frictionForce);

            // Don't use the full friction force if the force will stop the object
            if (!Vector.SameDirection(predictedVelocityParallel, parallelVelocity))
            {
                // Instead, just apply enough force to stop it
                return ForceToStop(predictedVelocityParallel, projectile.Mass);
            }
            else
            {
                return frictionForce;
            }
        }

        /// <summary>
        /// The amount of force needed to stop a projectile moving at a given velocity
        /// </summary>
        private Vector ForceToStop(Vector velocity, double mass)
        {
            Vector deltaV = -velocity;
            Vector acceleration = deltaV / DeltaTime;
            return mass * acceleration;
        }


        /// <summary>
        /// Calculates the normal force needed to cancel the existing perpendicular forces on an object
        /// </summary>
        private static Vector NormalForceToCancel(Projectile projectile, Triangle triangle)
        {
            Vector currentForcePerp = ProjectInSameDirection(projectile.NetForce, triangle.Normal);
            return -currentForcePerp;
        }

        /// <summary>
        /// Returns the projection of vec1 onto vec2 if they point in the same direction, zero otherwise;
        /// </summary>

        static private Vector ProjectInSameDirection(Vector vec1, Vector vec2)
        {
            if (Vector.SameDirection(vec1, vec2))
            {
                return vec1.ProjectOnto(vec2);
            }
            else
            {
                return Vector.NullVector();
            }
        }

        /// <summary>
        /// Predicts the velocity of the projectile in the next time step
        /// </summary>
        /// <param name="extraForce">The proposed external force acting on the particle (from the surface, in addition to the other forces already acting on it)</param>
        private Vector PredictVelocity(Projectile projectile, Vector extraForce)
        {
            Vector newAcceleration = (projectile.NetForce + extraForce) / projectile.Mass;
            return projectile.Velocity + newAcceleration * DeltaTime;
        }

        /// <summary>
        /// Predicts the position of the particle in the next time step
        /// </summary>
        /// <param name="extraForce">The proposed external force acting on the particle (from the surface, in addition to the other forces already acting on it)</param>
        private Vector PredictPosition(Projectile projectile, Vector extraForce)
        {
            return projectile.Position + PredictVelocity(projectile, extraForce) * DeltaTime;
        }

        ///// <summary>
        ///// Whether 
        ///// </summary>
        //private bool CheckForce(Projectile projectile, Vector proposedForce, Triangle triangle)
        //{
        //    Vector predictedVelocity = PredictVelocity(projectile, proposedForce);
        //    Vector intersectionPoint = triangle.Intersection(projectile.Position, predictedVelocity);
        //    double distanceFromTriangle = Vector.Distance(intersectionPoint, projectile.Position);
        //    Vector predictedPosition = PredictPosition(projectile, proposedForce);
        //    double predictedDistance = Vector.Distance(predictedPosition, projectile.Position);

        //    Vector currentToPredictedPosition = predictedPosition - projectile.Position;
        //    Vector forceDirection = (intersectionPoint - projectile.Position).ProjectOnto(triangle.Normal);

        //    return !Vector.SameDirection(currentToPredictedPosition, forceDirection)
        //        || predictedDistance <= distanceFromTriangle;
        //}

        /// <summary>
        /// Returns whether a given trajectory (from one position to another) intersects any triangles in the system
        /// </summary>
        private bool IntersectsATriangle(Vector initial, Vector final)
        {
            foreach (var surface in surfaces)
                foreach (var triangle in surface.Triangles)
                {
                    if (triangle.PassedThrough(initial, final))
                    {
                        return true;
                    }

                }
            return false;
        }

        /// <summary>
        /// Finds the vector between two positions (initial to final) and, if it penetrates the triangle,
        /// scales it down until it does not, returning the resulting position that does not penetrate the triangle.
        /// </summary>
        private static Vector GetInteraction(Vector initial, Vector final, Triangle triangle)
        {
            const double scale = .99;

            var interactionPoint = triangle.Intersection(initial, final)?.PositionVector() ?? throw new ArgumentException("Trajectory does not intersect triangle");
            while (triangle.PassedThrough(initial, interactionPoint))
            {
                Vector direction = interactionPoint - initial;
                direction *= scale;
                interactionPoint = direction + initial;
            }
            return interactionPoint;
        }

        /// <summary>
        /// Takes the projected final position of the projectile, and if it requires penetrating any triangle, replaces it 
        /// with a new position that will not intersect a triangle
        /// </summary>
        private Vector ChooseNewFinalPosition(Projectile projectile, Vector finalPosition)
        {
            Vector finalPoint = Vector.NullVector();
            bool worked = false;

            foreach (var surface in surfaces)
                foreach (var triangle in surface.Triangles)
                {
                    if (triangle.PassedThrough(projectile.Position, finalPosition))
                    {
                        Vector interactionPoint = GetInteraction(projectile.Position, finalPosition, triangle);

                        if (!IntersectsATriangle(projectile.Position, interactionPoint))
                        {
                            if (!worked)
                            {
                                finalPoint = interactionPoint;
                                worked = true;
                            }
                            else
                            {
                                double currentDistance = Vector.Distance(finalPoint, projectile.Position);
                                double newDistance = Vector.Distance(interactionPoint, projectile.Position);
                                if (currentDistance > newDistance)
                                    finalPoint = interactionPoint;
                            }
                        }
                    }

                }
            if (!worked)
                throw new InvalidOperationException("Somehow no point works?");

            return finalPoint;
        }

        /// <summary>
        /// Adjusts the proposed force to make sure the projectile's final position is at the intersectionPoint given.
        /// </summary>
        /// <param name="projectile">The projectile in question</param>
        /// <param name="target">The place the projectile needs to end up</param>
        /// <returns>A new force that puts the particle at the right place in the next time step</returns>
        private Vector AdjustForceForPosition(Projectile projectile, Vector target)
        {
            // Kinematic equation: deltaX = v0 * t + 1/2 * a * t^2
            // So a = (deltaX - v0 * t) * 2 / t^2

            Vector deltaX = target - projectile.Position;
            Vector acceleration = 2 / Square(DeltaTime) * (deltaX - projectile.Velocity * DeltaTime);
            Vector newForce = acceleration * projectile.Mass;
            Vector finalForce = newForce - projectile.NetForce;

            return finalForce;
        }

        override protected Vector GetForce(Projectile projectile)
        {
            Vector force = CheckAndGetForce(projectile);
            Vector finalPosition = PredictPosition(projectile, force);
            if (IntersectsATriangle(projectile.Position, finalPosition))
            {
                Vector newPosition = ChooseNewFinalPosition(projectile, finalPosition);
                force = AdjustForceForPosition(projectile, newPosition);
                Vector tryAgain = PredictPosition(projectile, force);
                if (IntersectsATriangle(projectile.Position, tryAgain))
                    force = LastDitchAttemptToFixForce(projectile, force);
            }
            return force;
        }

        /// <summary>
        /// An increasingly desperate series of flailing forces attempting to get the projectile to not go through any surfaces, in case all other methods are failing.  
        /// A last resort only.
        /// </summary>
        private Vector LastDitchAttemptToFixForce(Projectile projectile, Vector currentForce)
        {
            int counter = 0;
            Vector originalForce = currentForce;
            double magnitude = currentForce.Magnitude;
            Random random = new();

            // Lots of magic numbers here because the whole undertaking is pretty jank
            while (IntersectsATriangle(projectile.Position, PredictPosition(projectile, currentForce)))
            {
                if (counter == 1000 || counter == 2000)
                {
                    currentForce = originalForce;
                }
                if (counter < 1000)
                {
                    currentForce *= 1.01;
                }
                else if (counter < 2000)
                {
                    currentForce /= 1.01;
                }
                else if (counter == 3000)
                {
                    currentForce = -originalForce;
                }
                else if (counter < 10000)
                {
                    currentForce = Vector.RandomDirection(magnitude, random);
                }
                else if (counter < 30000)
                {
                    double newMagnitude = random.NextGaussian(magnitude, magnitude * .1);
                    currentForce = Vector.RandomDirection(newMagnitude, random);
                }
                else if (counter < 100000)
                {
                    double newMagnitude = random.NextDouble(0, 1e6);
                    currentForce = Vector.RandomDirection(newMagnitude, random);
                }
                else
                {
                    throw new ApplicationException("I give up!  I can't find any way to make this thing get out.");
                }

                ++counter;
            }

            return currentForce;
        }

        /// <summary>
        /// Gets the force from all triangles that the projectile hits, in total
        /// </summary>
        private Vector CheckAndGetForce(Projectile projectile)
        {
            Vector totalForce = Vector.NullVector();
            Vector nextLocation = PredictPosition(projectile, totalForce);
            foreach (var surface in surfaces)
                foreach (var triangle in surface.Triangles)
                {
                    if (triangle.PassedThrough(projectile.Position, nextLocation))
                    {
                        totalForce += ForceFromTriangle(projectile, surface, triangle, totalForce);
                    }
                }
            return totalForce;
        }

        /// <summary>
        /// Calcualtes the force of a triangle on a given projectile, given a current proposed force on the particle. 
        /// Includes both normal and frictional forces
        /// </summary>
        private Vector ForceFromTriangle(Projectile projectile, Surface surface, Triangle triangle, Vector currentForce)
        {
            Vector velocity = PredictVelocity(projectile, currentForce);
            Vector perpendicularComponent = velocity.ProjectOnto(triangle.Normal);
            Vector parallelComponent = velocity - perpendicularComponent;

            Vector normalForce = CalcNormalForce(projectile, perpendicularComponent, triangle, surface.Elasticity);
            Vector frictionForce = CalcFrictionForce(projectile, parallelComponent, surface.FrictionCoefficient, normalForce);
            Vector force = normalForce + frictionForce;

            return force;
        }

        /// <summary>
        /// Adds a surface to the list of surface that are relevant
        /// </summary>
        /// <param name="surface"></param>
        public void AddSurface(Surface surface)
        {
            surfaces.Add(surface);
        }
    }
}
