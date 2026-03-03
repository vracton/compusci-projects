using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A tube - a cylinder with an inner and outer radius so there is a hole through the middle, rendered on the inside and outside
    /// </summary>
    public class Tube3D : Shape3D
    {
        public Tube3D(double innerR, double outerR, double length, int nSegments = 16) :
            base(GetName(innerR, outerR, length, nSegments))
        {
            if (!(outerR > innerR))
            {
                throw new ArgumentException("Outer radius must be larger than inner radius!");
            }

            this.innerR = innerR;
            this.outerR = outerR;
            this.length = length;
            this.nSegments = nSegments;
        }

        private readonly double innerR;
        private readonly double outerR;
        private readonly double length;
        private readonly double nSegments;

        static private string GetName(double innerR, double outerR, double length, int nSegments)
        {
            return "Tube" + innerR + " " + outerR + " " + length + " " + nSegments;
        }


        protected override List<Vertex> MakeVertices()
        {
            var points = new List<Vertex>();

            double phiSeg = 2 * Math.PI / nSegments;

            // Texture calculations
            // x is around the cylinder, y is along the z axis
            double lengthProportion = length / (length + outerR - innerR);
            double thicknessProportion = 1 - lengthProportion;

            // Points along the edge
            for (double iphi = 0; iphi < 2 * Math.PI; iphi += phiSeg)
            {
                double xin = innerR * Math.Cos(iphi);
                double yin = innerR * Math.Sin(iphi);
                double xout = outerR * Math.Cos(iphi);
                double yout = outerR * Math.Sin(iphi);

                double u = iphi / (2 * Math.PI);

                // Top
                points.Add(new Vertex(new Point3D(xin, yin, length / 2), new Vector3D(0, 0, 1), new Point(u, 0)));
                points.Add(new Vertex(new Point3D(xout, yout, length / 2), new Vector3D(0, 0, 1), new Point(u, (2 * lengthProportion + thicknessProportion) / 2)));

                // Bottom
                points.Add(new Vertex(new Point3D(xin, yin, -length / 2), new Vector3D(0, 0, -1), new Point(u, .5)));
                points.Add(new Vertex(new Point3D(xout, yout, -length / 2), new Vector3D(0, 0, -1), new Point(u, lengthProportion / 2)));

                // Outside
                points.Add(new Vertex(new Point3D(xout, yout, length / 2), new Vector3D(xin, yin, 0), new Point(u, 0)));
                points.Add(new Vertex(new Point3D(xout, yout, -length / 2), new Vector3D(xin, yin, 0), new Point(u, lengthProportion / 2)));

                // Inside
                points.Add(new Vertex(new Point3D(xin, yin, length / 2), new Vector3D(-xin, -yin, 0), new Point(u, 0)));
                points.Add(new Vertex(new Point3D(xin, yin, -length / 2), new Vector3D(-xin, -yin, 0), new Point(u, lengthProportion / 2)));
            }

            return points;
        }

        protected override Int32Collection MakeTriangles()
        {
            var triangles = new Int32Collection();

            for (int i = 0; i < nSegments; ++i)
            {
                int nexti = i == nSegments - 1 ? 0 : i + 1;

                int index = i * 8;
                int nextIndex = nexti * 8;

                // Top
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(nextIndex + 1);

                triangles.Add(index);
                triangles.Add(nextIndex + 1);
                triangles.Add(nextIndex);

                // Bottom
                // Reverse orientation to face out
                int bIndex = index + 2;
                int nextBIndex = nextIndex + 2;

                triangles.Add(bIndex);
                triangles.Add(nextBIndex + 1);
                triangles.Add(bIndex + 1);

                triangles.Add(bIndex);
                triangles.Add(nextBIndex);
                triangles.Add(nextBIndex + 1);

                // Outside
                int oIndex = index + 4;
                int nextOIndex = nextIndex + 4;

                triangles.Add(oIndex);
                triangles.Add(oIndex + 1);
                triangles.Add(nextOIndex + 1);

                triangles.Add(oIndex);
                triangles.Add(nextOIndex + 1);
                triangles.Add(nextOIndex);

                // Inside
                int iIndex = index + 6;
                int nextIIndex = nextIndex + 6;

                triangles.Add(iIndex);
                triangles.Add(nextIIndex + 1);
                triangles.Add(iIndex + 1);

                triangles.Add(iIndex);
                triangles.Add(nextIIndex);
                triangles.Add(nextIIndex + 1);
            }

            return triangles;
        }

    }
}
