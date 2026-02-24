namespace Geometry.Geometry3D
{
    abstract public class ConvexPolyhedron : Polyhedron
    {
        protected ConvexPolyhedron(IEnumerable<Point> points) : base(points)
        { 
            if (!IsConvex())
            {
                throw new ArgumentException("The given points do not form a convex polyhedron!");
            }
        }

        private bool IsConvex()
        {
            foreach (var face in Faces)
            {
                var testPoint = GetPointNotOnFace(face);
                foreach (var vertex in Vertices)
                {
                    if (face.Vertices.Contains(vertex))
                    {
                        continue;
                    }
                    if (!face.ContainingPlane.SameHalfSpace(vertex, testPoint))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool IsInside(Point point)
        {
            var center = CenterOfMass; // In a convex polyhedron, the center of mass is always inside
            foreach (var face in Faces)
            {
                if (!face.ContainingPlane.SameHalfSpace(point, center))
                {
                    return false;
                }
            }

            return true;
        }

        private Point GetPointNotOnFace(Polygon face)
        {
            foreach (var vertex in Vertices)
            {
                if (!face.Vertices.Contains(vertex))
                {
                    return vertex;
                }
            }
            throw new Exception("All vertices are on the face!");
        }

        public override LineSegment? Overlap(LineSegment lineSegment)
        {
            bool p1Inside = IsInside(lineSegment.Point1);
            bool p2Inside = IsInside(lineSegment.Point2);

            // If the entire segment lies inside, returnt the whole thing
            if (p1Inside && p2Inside)
            {
                return new LineSegment(lineSegment.Point1, lineSegment.Point2);
            }
            // If neither lies inside, return nothing
            if (!p1Inside && !p2Inside)
            {
                return null;
            }
            // Otherwise, it is the point that lies inside and the intersection of the segment with the face
            var intersection = Intersection(lineSegment).First(); // There must be exactly one in this collection
            if (p1Inside)
            {
                return new LineSegment(lineSegment.Point1, intersection);
            }
            else
            {
                return new LineSegment(lineSegment.Point2, intersection);
            }

        }
    }
}
