using System.Drawing;
using VisualizerControl.Shapes;

namespace Thermodynamics
{
    /// <summary>
    /// A class holding general info about a particular type of particle
    /// </summary>
    /// <param name="name">The name of the particle type</param>
    /// <param name="mass">The mass of the particle, in kg</param>
    /// <param name="color">The color of the particle for display</param>
    public class ParticleInfo(string name, double mass, Color color, Shape3D shape)
    {
        public string Name { get; set; } = name;
        public double Mass { get; set; } = mass;
        public Color Color { get; set; } = color;
        public Shape3D Shape { get; set; } = shape;

        public ParticleInfo(string name, double mass, Color color) : this(name, mass, color, new Sphere3D())
        { }
    }


}
