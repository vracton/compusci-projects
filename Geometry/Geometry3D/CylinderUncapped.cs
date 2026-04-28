using DongUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// A cylinder with no caps on the end
    /// </summary>
    /// <param name="center">The center of the cylinder, directly in the middle</param>
    internal class CylinderUncapped(Point center, double radius, double height) : Shape3D
    {
        public override double Volume => Math.PI * UtilityFunctions.Square(radius) * height;

        public override double SurfaceArea => 2 * Math.PI * radius * height;

        public override Matrix InertialTensor => throw new NotImplementedException();

        public override double MaxRadius => throw new NotImplementedException();

        public override Point CenterOfMass => center;

        public override Shape3D Clone()
        {
            return new CylinderUncapped(center, radius, height);
        }

        public override Point ClosestPoint(Point point)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Point> Intersection(Line line)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Point> Intersection(LineSegment segment)
        {
            throw new NotImplementedException();
        }

        public override bool IsInside(Point point)
        {
            throw new NotImplementedException();
        }

        public override LineSegment? Overlap(LineSegment lineSegment)
        {
            throw new NotImplementedException();
        }

        public override Shape3D Translate(Vector vector)
        {
            return new CylinderUncapped(center + vector, radius, height);
        }

        protected override Point GetCenter()
        {
            return center;
        }
    }
}
