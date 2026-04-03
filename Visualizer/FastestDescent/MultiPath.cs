using DongUtility;
using System;
using System.Collections.Generic;

namespace Visualizer.FastestDescent
{
    public class MultiPath : Path
    {
        private readonly List<Path> subPaths = [];

        public void AddPath(Path path)
        {
            subPaths.Add(path);
        }

        public override double InitialParameter => 0;

        public override double FinalParameter => subPaths.Count;

        protected override Vector Function(double parameter)
        {
            int currentIndex = (int)(Math.Floor(parameter));

            if (currentIndex >= subPaths.Count)
                currentIndex = subPaths.Count - 1;
            if (currentIndex < 0)
                currentIndex = 0;

            Path currentPath = subPaths[currentIndex];
            double pathParameter = Math.Clamp(parameter - currentIndex, 0, 1);

            return GetSubValue(currentPath, pathParameter);
        }

        /// <summary>
        /// Gets the value from a given path object
        /// </summary>
        /// <param name="parameter">The sub-parameter of the path, normalized from zero to one.  
        /// This gets converted to the true parameter value.</param>
        private static Vector GetSubValue(Path path, double parameter)
        {
            double range = path.FinalParameter - path.InitialParameter;
            double parVal = range * parameter + path.InitialParameter;
            return path.GetPosition(parVal);
        }
    }
}
