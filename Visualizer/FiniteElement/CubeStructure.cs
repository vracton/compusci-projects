using DongUtility;
using PhysicsUtility.Kinematics;
using System;

namespace Visualizer.FiniteElement
{
    class CubeStructure : ParticleStructure
    {
        public int EdgeCount { get; }
        public double EdgeLength { get; }
        public double Mass { get; }
        public double SpringConstant { get; }
        public CubeStructure(int edgeCount, double edgeLength, Vector center, Vector rot, double mass, double springConstant, bool simplified = true, bool damped = false)
        {
            EdgeCount = edgeCount;
            EdgeLength = edgeLength;
            Mass = mass;
            SpringConstant = springConstant;
            double nodeSpacing = EdgeLength / (EdgeCount - 1);
            double faceDiagonalLength = nodeSpacing * Math.Sqrt(2);
            double cellBodyDiagonalLength = nodeSpacing * Math.Sqrt(3);
            double faceDiagonalSpringConstant = springConstant * 0.8;
            double interLayerCrossSpringConstant = springConstant * 0.6;
            const double minScale = 0.25;
            const double maxScale = 1.0;

            // Soften longer links to reduce post-impact jumbling while preserving anti-collapse structure.
            double ScaledSpringConstant(double baseSpringConstant, double connectorLength)
            {
                double scale = UtilityFunctions.Clamp(nodeSpacing / connectorLength, minScale, maxScale);
                return baseSpringConstant * scale;
            }

            var centerOffset = new Vector(edgeLength / 2, edgeLength / 2, edgeLength / 2);
            var rotation = new Rotation();
            rotation.RotateXAxis(UtilityFunctions.DegreesToRadians(rot.X));
            rotation.RotateYAxis(UtilityFunctions.DegreesToRadians(rot.Y));
            rotation.RotateZAxis(UtilityFunctions.DegreesToRadians(rot.Z));

            Projectile[,,] projs = new Projectile[EdgeCount, EdgeCount, EdgeCount];

            //create point lattice
            for (int x = 0; x < EdgeCount; x++)
            {
                for (int y = 0; y < EdgeCount; y++)
                {
                    for (int z = 0; z < EdgeCount; z++)
                    {
                        var localPosition = EdgeLength / EdgeCount * new Vector(x, y, z) - centerOffset;
                        var rotatedPos = center + rotation.ApplyRotation(localPosition);
                        var proj = new Projectile(rotatedPos, new(), mass / (EdgeCount * EdgeCount * EdgeCount));
                        projs[x, y, z] = proj;

                        switch (z)
                        {
                            case 0:
                                proj.Color = System.Drawing.Color.Red;
                                break;
                            case 1:
                                proj.Color = System.Drawing.Color.Green;
                                break;
                            case 2:
                                proj.Color = System.Drawing.Color.Blue;
                                break;
                        }

                        AddProjectile(proj);
                    }
                }
            }

            //create small connectors between points (no diags)
            for (int x = 0; x < EdgeCount; x++)
            {
                for (int y = 0; y < EdgeCount; y++)
                {
                    for (int z = 0; z < EdgeCount; z++)
                    {
                        if (x < EdgeCount - 1)
                            AddConnector(projs[x, y, z], projs[x + 1, y, z], simplified ? springConstant : ScaledSpringConstant(springConstant, nodeSpacing), nodeSpacing);
                        if (y < EdgeCount - 1)
                            AddConnector(projs[x, y, z], projs[x, y + 1, z], simplified ? springConstant : ScaledSpringConstant(springConstant, nodeSpacing), nodeSpacing);
                        if (z < EdgeCount - 1)
                            AddConnector(projs[x, y, z], projs[x, y, z + 1], simplified ? springConstant : ScaledSpringConstant(springConstant, nodeSpacing), nodeSpacing);
                    }
                }
            }

            //create large connectors between maximally seperated points (no diags)
            for (int x = 0; x < EdgeCount; x++)
            {
                for (int y = 0; y < EdgeCount; y++)
                {
                    for (int z = 0; z < EdgeCount; z++)
                    {
                        if (x < 1)
                            AddConnector(projs[x, y, z], projs[EdgeCount - 1, y, z], simplified ? springConstant * 4 : ScaledSpringConstant(springConstant * 4, EdgeLength), EdgeLength);
                        if (y < 1)
                            AddConnector(projs[x, y, z], projs[x, EdgeCount - 1, z], simplified ? springConstant * 4 : ScaledSpringConstant(springConstant * 4, EdgeLength), EdgeLength);
                        if (z < 1)
                            AddConnector(projs[x, y, z], projs[x, y, EdgeCount - 1], simplified ? springConstant * 4 : ScaledSpringConstant(springConstant * 4, EdgeLength), EdgeLength);
                    }
                }
            }

            //diagonal braces across 2x3 faces
            for (int y = 0; y < EdgeCount; y++)
            {
                for (int z = 1; z < EdgeCount; z++)
                {
                    AddConnector(projs[0, y, z], projs[EdgeCount - 1, y, z - 1], simplified ? springConstant * 0.67 : ScaledSpringConstant(springConstant * 0.67, EdgeLength * Math.Sqrt(5)), EdgeLength * Math.Sqrt(5));
                    AddConnector(projs[EdgeCount - 1, y, z], projs[0, y, z - 1], simplified ? springConstant * 0.67 : ScaledSpringConstant(springConstant * 0.67, EdgeLength * Math.Sqrt(5)), EdgeLength * Math.Sqrt(5));
                }
            }

            if (!simplified)
            {
                //connect opposite corners
                AddConnector(projs[0, 0, 0], projs[EdgeCount - 1, EdgeCount - 1, EdgeCount - 1], ScaledSpringConstant(springConstant * 0.5, EdgeLength * Math.Sqrt(3)), EdgeLength * Math.Sqrt(3));
                AddConnector(projs[0, 0, EdgeCount - 1], projs[EdgeCount - 1, EdgeCount - 1, 0], ScaledSpringConstant(springConstant * 0.5, EdgeLength * Math.Sqrt(3)), EdgeLength * Math.Sqrt(3));
                AddConnector(projs[0, EdgeCount - 1, 0], projs[EdgeCount - 1, 0, EdgeCount - 1], ScaledSpringConstant(springConstant * 0.5, EdgeLength * Math.Sqrt(3)), EdgeLength * Math.Sqrt(3));
                AddConnector(projs[0, EdgeCount - 1, EdgeCount - 1], projs[EdgeCount - 1, 0, 0], ScaledSpringConstant(springConstant * 0.5, EdgeLength * Math.Sqrt(3)), EdgeLength * Math.Sqrt(3));

                //diagonal braces on all face types (XY, XZ, YZ)
                for (int z = 0; z < EdgeCount; z++)
                {
                    for (int x = 0; x < EdgeCount - 1; x++)
                    {
                        for (int y = 0; y < EdgeCount - 1; y++)
                        {
                            AddConnector(projs[x, y, z], projs[x + 1, y + 1, z], ScaledSpringConstant(faceDiagonalSpringConstant, faceDiagonalLength), faceDiagonalLength);
                            AddConnector(projs[x + 1, y, z], projs[x, y + 1, z], ScaledSpringConstant(faceDiagonalSpringConstant, faceDiagonalLength), faceDiagonalLength);
                        }
                    }
                }

                for (int y = 0; y < EdgeCount; y++)
                {
                    for (int x = 0; x < EdgeCount - 1; x++)
                    {
                        for (int z = 0; z < EdgeCount - 1; z++)
                        {
                            AddConnector(projs[x, y, z], projs[x + 1, y, z + 1], ScaledSpringConstant(faceDiagonalSpringConstant, faceDiagonalLength), faceDiagonalLength);
                            AddConnector(projs[x + 1, y, z], projs[x, y, z + 1], ScaledSpringConstant(faceDiagonalSpringConstant, faceDiagonalLength), faceDiagonalLength);
                        }
                    }
                }

                for (int x = 0; x < EdgeCount; x++)
                {
                    for (int y = 0; y < EdgeCount - 1; y++)
                    {
                        for (int z = 0; z < EdgeCount - 1; z++)
                        {
                            AddConnector(projs[x, y, z], projs[x, y + 1, z + 1], ScaledSpringConstant(faceDiagonalSpringConstant, faceDiagonalLength), faceDiagonalLength);
                            AddConnector(projs[x, y + 1, z], projs[x, y, z + 1], ScaledSpringConstant(faceDiagonalSpringConstant, faceDiagonalLength), faceDiagonalLength);
                        }
                    }
                }

                // cross-diagonals between adjacent z layers, 2x2x2
                for (int x = 0; x < EdgeCount - 1; x++)
                {
                    for (int y = 0; y < EdgeCount - 1; y++)
                    {
                        for (int z = 0; z < EdgeCount - 1; z++)
                        {
                            AddConnector(projs[x, y, z], projs[x + 1, y + 1, z + 1], ScaledSpringConstant(interLayerCrossSpringConstant, cellBodyDiagonalLength), cellBodyDiagonalLength);
                            AddConnector(projs[x + 1, y, z], projs[x, y + 1, z + 1], ScaledSpringConstant(interLayerCrossSpringConstant, cellBodyDiagonalLength), cellBodyDiagonalLength);
                            AddConnector(projs[x, y + 1, z], projs[x + 1, y, z + 1], ScaledSpringConstant(interLayerCrossSpringConstant, cellBodyDiagonalLength), cellBodyDiagonalLength);
                            AddConnector(projs[x + 1, y + 1, z], projs[x, y, z + 1], ScaledSpringConstant(interLayerCrossSpringConstant, cellBodyDiagonalLength), cellBodyDiagonalLength);
                        }
                    }
                }
            }
        }
    }
}
