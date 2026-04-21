using DongUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geometry.Geometry3D
{
    internal class Sphere(Point center, double radius) : Shape3D
    {
        public override double Volume => 4.0 / 3.0 * Math.PI * UtilityFunctions.Pow(radius, 3);

        public override double SurfaceArea => 4 * Math.PI * UtilityFunctions.Square(radius);

        public override Matrix InertialTensor => 0.4 * UtilityFunctions.Square(radius) * Matrix.Identity(3);

        public override double MaxRadius => radius;

        public override Point CenterOfMass => center;

        public override Shape3D Clone()
        {
            return new Sphere(center, radius);
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
            var distance2 = Point.DistanceSquared(point, center);
            return distance2 < UtilityFunctions.Square(radius);
        }

        public override LineSegment? Overlap(LineSegment lineSegment)
        {
            throw new NotImplementedException();
        }

        public override Shape3D Translate(Vector vector)
        {
            return new Sphere(center + vector, radius);
        }

        protected override Point GetCenter()
        {
            return center;
        }
    }
}
