using DongUtility;
using System.Windows.Media;
using Geometry.Geometry3D;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// A basic triangle class - uses the Geometry.Gemetry3D.Triangle class for the underlying geometry, but adds color and transparency information, as well as some helper functions.
    /// </summary>
    /// <param name="isTransparent">Makes the triangle translucent (50% opacity). Here for convenience but you can adjust the alpha channel (color.A) in the color directly.</param>
    public class Triangle(Vector point1, Vector point2, Vector point3, Color color, bool isTransparent = false)
    {
        /// <summary>
        /// The underlying triangle, where all the calculation happens
        /// </summary>
        public Geometry.Geometry3D.Triangle UnderlyingGeometry { get; } = new Geometry.Geometry3D.Triangle(VectorExtensions.ToPoint(point1),
                VectorExtensions.ToPoint(point2),
                VectorExtensions.ToPoint(point3));

        /// <summary>
        /// The vertices of the triangle
        /// </summary>
        public Point[] Points => UnderlyingGeometry.Vertices;
        public Color Color { get; } = color;

        /// <summary>
        /// True if the triangle should be drawn as translucent (50% opacity), overriding the alpha channel of the color. Here for convenience but you can adjust the alpha channel (color.A) in the color directly.
        /// </summary>
        public bool IsTransparent { get; } = isTransparent;

        /// <summary>
        /// The perpendicular vector to the triangle.
        /// </summary>
        public Vector Normal => UnderlyingGeometry.ContainingPlane.Normal;

        /// <summary>
        /// Whether the segment connecting these two points (the initial and final position of a particle) intersects the triangle.
        /// </summary>
        public bool PassedThrough(Vector p1, Vector p2)
        {
            var segment = new Geometry.Geometry3D.LineSegment(VectorExtensions.ToPoint(p1),
                VectorExtensions.ToPoint(p2));
            return UnderlyingGeometry.Intersects(segment);
        }

        /// <summary>
        /// The distance of the point on the line segment that is closest to the triangle.
        /// </summary>
        public double ClosestApproach(Geometry.Geometry3D.LineSegment segment)
        {
            return UnderlyingGeometry.DistanceFrom(segment);
        }

        /// <summary>
        /// The Point at which the segment connecting these two vectors intersects the triangle, if an intersection exists
        /// . Returns null if no intersection exists.
        /// </summary>
        public Point? Intersection(Vector p1, Vector p2)
        {
            var segment = new Geometry.Geometry3D.LineSegment(VectorExtensions.ToPoint(p1),
                VectorExtensions.ToPoint(p2));
            return UnderlyingGeometry.ContainingPlane.Intersection(segment.UnderlyingLine);
        }
    }
}
