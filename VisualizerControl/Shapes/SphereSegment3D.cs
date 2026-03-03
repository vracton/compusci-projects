using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VisualizerControl.Shapes
{
    /// <summary>
    /// A shell-shaped segment of a sphere
    /// </summary>
    public class SphereSegment3D : Shape3D
    {
        private readonly int NFacePoints;
        private readonly double StartPhi;
        private readonly double EndPhi;
        private readonly double StartTheta;
        private readonly double EndTheta;
        private readonly double StartR;
        private readonly double EndR;

        /// <param name="nFacePoints">Number of points on the face in each direction in addition to the endpoints.  nFacePoints = 3 will give 25 total points on the face (3 + 2 in each direction)</param>
        /// <param name="startPhi">Initial value of phi, the azimuthal angle (physics convention)</param>
        /// <param name="startTheta">Initial value of theta, the polar angle (physics convention)</param>
        public SphereSegment3D(int nFacePoints, double startPhi, double endPhi, double startTheta, double endTheta, double startR, double endR) :
            base($"SphereSegmentFP{nFacePoints}SP{startPhi}EP{endPhi}ST{startTheta}ET{endTheta}SR{startR}ER{endR}", true)
        {
            // First check the arguments
            if (!(nFacePoints >= 0)) // Also catches NaN entries
            {
                throw new ArgumentException("NFacePoints cannot be negative!");
            }
            if (!(startR >= 0))
            {
                throw new ArgumentException("r must be positive!");
            }
            if (!(startTheta >= 0 && endTheta <= Math.PI))
            {
                throw new ArgumentException("theta must be between 0 and pi!");
            }
            if (!(startPhi < endPhi & startTheta < endTheta && startR < endR)) // Use the ! form to catch NaN entries
            {
                throw new ArgumentException("start values of r, phi, and theta must be less than the end values");
            }

            NFacePoints = nFacePoints;
            StartPhi = startPhi;
            EndPhi = endPhi;
            StartTheta = startTheta;
            EndTheta = endTheta;
            StartR = startR;
            EndR = endR;
        }

        protected override List<Vertex> MakeVertices()
        {
            var vertices = new List<Vertex>();
            // Outer face first
            MakeCurvedFace(EndR, false, ref vertices);

            // Then the inner face
            if (StartR > 0)
            {
                MakeCurvedFace(StartR, true, ref vertices);
            }
            else // If inner r = 0, only need to worry about side faces
            {
                double averagePhi = (StartPhi + EndPhi) / 2;
                var origin = new Point3D(0, 0, 0);
                // Varying-phi sides first
                vertices.Add(new Vertex(origin, -ThetaHat(averagePhi, StartTheta), new Point(.5, 0)));
                vertices.Add(new Vertex(origin, ThetaHat(averagePhi, EndTheta), new Point(.5, 0)));
                // Now varying-theta sides
                vertices.Add(new Vertex(origin, -PhiHat(StartPhi), new Point(0, .5)));
                vertices.Add(new Vertex(origin, PhiHat(EndPhi), new Point(0, .5)));
            }

            return vertices;
        }

        /// <summary>
        /// Creates one curved face and adds the values to the list called vertices
        /// Also addes in duplicated vertices used to make the hard edge
        /// </summary>
        /// <param name="inward">True if the normals should point toward the center of the circle</param>
        private void MakeCurvedFace(double radius, bool inward, ref List<Vertex> vertices)
        {
            int nDivs = 1 + NFacePoints; // There is always at least 1 division

            double totalPhi = EndPhi - StartPhi;
            double totalTheta = EndTheta - StartTheta;
            double phiStep = totalPhi / nDivs;
            double thetaStep = totalTheta / nDivs;

            int nPoints = nDivs + 1; // One more to catch the endpoints

            // TODO There remains an inefficiency if theta is at a pole

            for (int i = 0; i < nPoints; ++i)
                for (int j = 0; j < nPoints; ++j)
                {
                    double iphi = StartPhi + i * phiStep;
                    double itheta = StartTheta + j * thetaStep;

                    // First the face points
                    var point = SphericalPoint(radius, iphi, itheta);
                    var normal = new Vector3D(point.X, point.Y, point.Z);
                    if (inward)
                    {
                        normal = -normal;
                    }
                    var uv = new Point((double)i / nDivs, (double)j / nDivs);
                    var vertex = new Vertex(point, normal, uv);
                    vertices.Add(vertex);
                }

            // Now the theta edge points
            for (int i = 0; i < nPoints; ++i)
            {
                double iphi = StartPhi + i * phiStep;
                // One side
                var point = SphericalPoint(radius, iphi, StartTheta);
                var normal = -ThetaHat(iphi, StartTheta);
                var uv = new Point((double)i / nDivs, radius == EndR ? 1 : 0);
                var vertex = new Vertex(point, normal, uv);
                vertices.Add(vertex);
                // The other side
                point = SphericalPoint(radius, iphi, EndTheta);
                normal = ThetaHat(iphi, EndTheta);
                var vertex2 = new Vertex(point, normal, uv);
                vertices.Add(vertex2);
            }

            // Now the phi edge points
            // These are independent of theta
            var lowNormal = -PhiHat(StartPhi);
            var highNormal = PhiHat(EndPhi);
            for (int i = 0; i < nPoints; ++i)
            {
                double itheta = StartTheta + i * thetaStep;
                // One side
                var point = SphericalPoint(radius, StartPhi, itheta);
                var uv = new Point(radius == EndR ? 1 : 0, (double)i / nDivs);
                var vertex = new Vertex(point, lowNormal, uv);
                vertices.Add(vertex);
                // The other side
                point = SphericalPoint(radius, EndPhi, itheta);
                var vertex2 = new Vertex(point, highNormal, uv);
                vertices.Add(vertex2);
            }
        }

        private static Point3D SphericalPoint(double radius, double phi, double theta)
        {
            double ix = radius * Math.Cos(phi) * Math.Sin(theta);
            double iy = radius * Math.Sin(phi) * Math.Sin(theta);
            double iz = radius * Math.Cos(theta);
            return new(ix, iy, iz);
        }

        private static Vector3D ThetaHat(double phi, double theta)
        {
            double x = Math.Cos(theta) * Math.Cos(phi);
            double y = Math.Cos(theta) * Math.Sin(phi);
            double z = -Math.Sin(theta);
            return new(x, y, z);
        }

        private static Vector3D PhiHat(double phi)
        {
            double x = -Math.Sin(phi);
            double y = Math.Cos(phi);
            return new(x, y, 0);
        }

        protected override Int32Collection MakeTriangles()
        {

            var triangles = new Int32Collection();

            int nPoints = NFacePoints + 2;

            int nFacePoints = nPoints * nPoints;
            int nSidePoints = 4 * nPoints;

            // Remember the order: first outer face, then outer side points, then inner face, then inner face points
            // For face points, it is phi-varying low, then high, then theta-varying low, then high

            // Outer face first
            for (int iphi = 0; iphi < nPoints - 1; ++iphi)
                for (int itheta = 0; itheta < nPoints - 1; ++itheta)
                {
                    int mainPoint = iphi + itheta * nPoints;
                    // Remember counterclockwise winding
                    triangles.Add(mainPoint);
                    triangles.Add(mainPoint + 1);
                    triangles.Add(mainPoint + nPoints + 1);

                    triangles.Add(mainPoint);
                    triangles.Add(mainPoint + nPoints + 1);
                    triangles.Add(mainPoint + nPoints);
                }

            // Special case if it goes all the way to the center
            // Then there are just four points at the center for the four side pieces
            if (StartR == 0)
            {
                int firstCenterPoint = nFacePoints + nSidePoints;
                for (int i = 0; i < nPoints - 1; ++i)
                {
                    // Low theta
                    triangles.Add(nFacePoints + i);
                    triangles.Add(firstCenterPoint);
                    triangles.Add(nFacePoints + i + 1);

                    // High theta
                    triangles.Add(nFacePoints + nPoints + i);
                    triangles.Add(nFacePoints + nPoints + i + 1);
                    triangles.Add(firstCenterPoint + 1);

                    // Low phi
                    triangles.Add(nFacePoints + 2 * nPoints + i);
                    triangles.Add(nFacePoints + 2 * nPoints + i + 2);
                    triangles.Add(firstCenterPoint + 2);

                    // High phi
                    triangles.Add(nFacePoints + 3 * nPoints + i);
                    triangles.Add(firstCenterPoint + 3);
                    triangles.Add(nFacePoints + 3 * nPoints + i + 1);
                }
            }
            else
            {
                // Next inner face
                for (int iphi = 0; iphi < nPoints - 1; ++iphi)
                    for (int itheta = 0; itheta < nPoints - 1; ++itheta)
                    {
                        int mainPoint = nFacePoints + nSidePoints + iphi + itheta * nPoints;
                        // Remember counterclockwise winding
                        triangles.Add(mainPoint);
                        triangles.Add(mainPoint + nPoints + 1);
                        triangles.Add(mainPoint + nPoints);

                        triangles.Add(mainPoint);
                        triangles.Add(mainPoint + 1);
                        triangles.Add(mainPoint + nPoints + 1);
                    }

                // Now the sides
                for (int i = 0; i < nPoints - 1; ++i)
                {
                    // Low theta
                    int outsideIndex = nFacePoints + i;
                    int insideIndex = 2 * nFacePoints + nSidePoints + i;
                    triangles.Add(outsideIndex);
                    triangles.Add(insideIndex);
                    triangles.Add(outsideIndex + 1);

                    triangles.Add(insideIndex);
                    triangles.Add(insideIndex + 1);
                    triangles.Add(outsideIndex + 1);

                    // High theta
                    outsideIndex = nFacePoints + nPoints + i;
                    insideIndex = 2 * nFacePoints + nSidePoints + nPoints + i;
                    triangles.Add(outsideIndex);
                    triangles.Add(outsideIndex + 1);
                    triangles.Add(insideIndex);

                    triangles.Add(insideIndex);
                    triangles.Add(outsideIndex + 1);
                    triangles.Add(insideIndex + 1);

                    // Low phi
                    outsideIndex = nFacePoints + 2 * nPoints + i;
                    insideIndex = 2 * nFacePoints + nSidePoints + 2 * nPoints + i;
                    triangles.Add(outsideIndex);
                    triangles.Add(insideIndex);
                    triangles.Add(outsideIndex + 1);

                    triangles.Add(insideIndex);
                    triangles.Add(insideIndex + 1);
                    triangles.Add(outsideIndex + 1);

                    // High phi
                    outsideIndex = nFacePoints + 3 * nPoints + i;
                    insideIndex = 2 * nFacePoints + nSidePoints + 3 * nPoints + i;
                    triangles.Add(outsideIndex);
                    triangles.Add(outsideIndex + 1);
                    triangles.Add(insideIndex);

                    triangles.Add(insideIndex);
                    triangles.Add(outsideIndex + 1);
                    triangles.Add(insideIndex + 1);

                }
            }
            return triangles;
        }


    }
}
