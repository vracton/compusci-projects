using System;
using System.Windows.Media;

namespace NaturalSelection
{
    public readonly struct GeneInfo(string name, double mean, double sd, Color color, Type type)
    {
        public string Name { get; } = name;
        public double Mean { get; } = mean;
        public double SD { get; } = sd;
        public Color Color { get; } = color;
        public Type Type { get; } = type;
    }
}
