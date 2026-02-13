using System.IO;
using System.Windows.Media;
using WPFUtility;

namespace VisualizerControl
{
    /// <summary>
    /// A basic material, with a color, fresnel coefficient, and roughness
    /// </summary>
    public class BasicMaterial
    {
        public Color Color { get; set; }
        /// <summary>
        /// The Fresnel coefficient of the material - how much light is reflected at normal incidence
        /// </summary>
        public double Fresnel { get; }
        /// <summary>
        /// Gets the roughness value, which represents the surface texture or irregularity
        /// </summary>
        public double Roughness { get; }
        public string Name { get; }

        public BasicMaterial(Color color, double fresnel = .05, double roughness = .3) :
            this(color, fresnel, roughness, $"R{color.R}G{color.G}B{color.B}F{fresnel}R{roughness}")
        {

        }

        public BasicMaterial(Color color, double fresnel, double roughness, string name)
        {
            Color = color;
            Fresnel = fresnel;
            Roughness = roughness;
            Name = name;
        }

        public BasicMaterial(BinaryReader br)
        {
            Color = br.ReadColor();
            Fresnel = br.ReadDouble();
            Roughness = br.ReadDouble();
            Name = br.ReadString();
        }

        public void WriteContent(BinaryWriter bw)
        {
            bw.Write(Color);
            bw.Write(Fresnel);
            bw.Write(Roughness);
            bw.Write(Name);
        }
    }
}
