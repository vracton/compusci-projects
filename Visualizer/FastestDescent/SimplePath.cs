using DongUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visualizer.RaceToTheBottom
{
    abstract public class SimplePath : Path
    {
        protected Vector Point1 { get; }
        protected Vector Point2 { get; }

        public SimplePath(Vector point1, Vector point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public override double InitialParameter => 0;
        public override double FinalParameter => 1;
    }
}
