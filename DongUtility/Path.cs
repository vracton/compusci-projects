using static DongUtility.UtilityFunctions;

namespace DongUtility
{
    /// <summary>
    /// A base class representing a generic path in space as a parametric function
    /// </summary>
    abstract public class Path
    {
        /// <summary>
        /// The function that determines the path itself.
        /// </summary>
        abstract protected Vector Function(double parameter);
        
        /// <summary>
        /// The starting and ending times.  It is the programmer's responsibility to make sure 
        /// these are at the right positions!
        /// </summary>
        abstract public double InitialParameter { get; }
        abstract public double FinalParameter { get; }
        
        /// <summary>
        /// The tolerance, in meters, considered acceptable for a linear approximation of the curve
        /// </summary>
        public double Tolerance { get; set; } = .001;

        /// <summary>
        /// The minimum change in the parameter to be considered.
        /// There should be no substantial change in the function anywhere close to this scale.
        /// </summary>
        public double MinimumStep { get; set; } = .0001;

        /// <summary>
        /// Finds the position at a given time.
        /// </summary>
        public Vector GetPosition(double parameter) => Function(parameter);

        /// <summary>
        /// Finds the next parameter that will approximate the function linearly to within the given precision
        /// </summary>
        /// <param name="finalParameter">The last point allowed, to avoid weird overflows</param>
        public double GetNextParameter(double currentParameter, double finalParameter)
        {
            double parameterStep = MinimumStep;
            const double changeFactor = 2;

            while (GoodGuess(currentParameter, currentParameter + parameterStep))
            {
                parameterStep *= changeFactor;
                if (currentParameter + parameterStep > finalParameter)
                    return finalParameter;
            }
            parameterStep /= changeFactor;

            return currentParameter + parameterStep;
        }

        /// <summary>
        /// Find the closest point on the path to a given point
        /// </summary>
        /// <returns>The parameter value of the point of closest approach</returns>
        public double GetClosestParameter(Vector position)
        {
            double closestDistanceSquared = double.MaxValue;
            double closestParameter = InitialParameter;
            for (double param = InitialParameter; param < FinalParameter; param = GetNextParameter(param, FinalParameter))
            {
                double distanceSquared = Vector.Distance2(position, GetPosition(param));
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestParameter = param;
                }
            }
            return closestParameter;
        }

        /// <summary>
        /// Returns the change in position (as a vector) for a small change in the parameter
        /// equal to MinimumStep, evaluated at parameter
        /// </summary>
        public Vector Dx(double parameter)
        {
            Vector currentPosition = GetPosition(parameter);
            Vector nextPosition = GetPosition(parameter + MinimumStep);
            return nextPosition - currentPosition;
        }

        /// <summary>
        /// Projects an input vector along the tangent to the curve at the value of parameter
        /// </summary>
        /// <returns>The projection of the input vector</returns>
        public Vector Project(Vector input, double parameter)
        {
            var tangent = Dx(parameter);
            var magSquared = tangent.MagnitudeSquared;
            if (magSquared == 0)
                return Vector.NullVector();
            return tangent * Vector.Dot(input, tangent) / tangent.MagnitudeSquared;
        }

        /// <summary>
        /// Returns the derivative (as a vector) of position with respect to parameter
        /// Evaluated at parameter.
        /// </summary>
        public Vector Derivative(double parameter)
        {
            return Dx(parameter) / MinimumStep;
        }

        /// <summary>
        /// Returns whether predictionTime can be used as a good linear appoximation. 
        /// It checks whether a change of twice as much time still matches the function to within the stated tolerance
        /// </summary>
        private bool GoodGuess(double currentParameter, double predictionParameter)
        {
            // Linearly extrapolate the position at currentTime + 2*dT
            double deltaT = predictionParameter - currentParameter;
            double extrapolatedParameter = predictionParameter + deltaT;
            Vector currentPosition = GetPosition(currentParameter);
            Vector predictedPosition = GetPosition(predictionParameter);
            Vector deltaX = predictedPosition - currentPosition;
            Vector dTdX = deltaX / deltaT;
            Vector extrapolatedPosition = predictedPosition + dTdX * deltaT;

            // Calculate the true value
            Vector trueFinalPosition = GetPosition(extrapolatedParameter);

            // Compare them
            double distance2 = Vector.Distance2(extrapolatedPosition, trueFinalPosition);
            return distance2 < Square(Tolerance);
        }
    }
}
