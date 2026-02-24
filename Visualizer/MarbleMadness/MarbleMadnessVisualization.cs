using DongUtility;
using System.Collections.Generic;
using Visualizer.Kinematics;
using VisualizerBaseClasses;
using VisualizerControl;
using VisualizerControl.Commands;
using VisualizerControl.Shapes;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// A visualization with some extras for the marble machine
    /// </summary>
    internal class MarbleMadnessVisualization(IEngine engine) : KinematicsVisualization(engine)
    {
        private readonly List<Triangle> triangles = [];

        /// <summary>
        /// All surfaces are fundamentally triangles, so they must be added as such
        /// </summary>
        public void AddTriangle(Triangle triangle)
        {
            triangles.Add(triangle);
        }

        public override CommandSet<VisualizerControl.Visualizer> Initialization()
        {
            var commands = base.Initialization();

            foreach (var triangle in triangles)
            {
                var tri = new Triangle3D(
                    WPFUtility.UtilityFunctions.ConvertToVector3D(triangle.Points[0]),
                    WPFUtility.UtilityFunctions.ConvertToVector3D(triangle.Points[1]),
                    WPFUtility.UtilityFunctions.ConvertToVector3D(triangle.Points[2]), true);
                var color = triangle.Color;
                if (triangle.IsTransparent)
                {
                    color.A = Constants.MaxByte / 2;
                }
                var obj = new ObjectPrototype(tri, new BasicMaterial(color));
                commands.AddCommand(new AddObject(obj, Counter++));
            }
            return commands;
        }
    }
}
