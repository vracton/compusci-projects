using DongUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visualizer.RaceToTheBottom
{
    public class SimpleLinearPath : SimplePath
    {
        public SimpleLinearPath(Vector point1, Vector point2) :
            base(point1, point2)
        {
            distance = Vector.Distance(point1, point2);
            displacement = (point2 - point1).UnitVector();
        }

        private double distance;
        private Vector displacement;

        protected override Vector Function(double parameter)
        {
            return Point1 + displacement * parameter * distance;
        }
    }
}
