using DongUtility;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// A surface that marbles bounce off of
    /// </summary>
    /// <param name="elasticity">The bounciness of the surface.  Must be between 0 and 1, with zero being no bounce and 1 being fully elastic</param>
    /// <param name="frictionCoefficient">The friction coefficient, which must be between 0 and 2</param>
    public class Surface(double elasticity, double frictionCoefficient)
    {
        /// <summary>
        /// All the triangles in the surface - there can be as many as you want and they don't have to be contiguous or even non-overlapping
        /// </summary>
        public List<Triangle> Triangles { get; } = [];
        public double Elasticity { get; } = Math.Clamp(elasticity, 0, 1);
        public double FrictionCoefficient { get; } = Math.Clamp(frictionCoefficient, 0, 2);

        /// <summary>
        /// Add another triangle to the surface
        /// </summary>
        public void AddTriangle(Triangle triangle)
        {
            Triangles.Add(triangle);
        }

        /// <summary>
        /// Add a quadrilateral to the surface
        /// </summary>
        /// <param name="isTransparent">Whether the quad is transparent.  You can control this more directly by using the color.A parameter (the alpha channel)</param>
        public void AddQuad(Vector p1, Vector p2, Vector p3, Vector p4, Color color, bool isTransparent = false)
        {
            AddTriangle(new Triangle(p1, p2, p4, color, isTransparent));
            AddTriangle(new Triangle(p4, p2, p3, color, isTransparent));
        }

    }
}
