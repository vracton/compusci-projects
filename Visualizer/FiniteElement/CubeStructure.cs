using DongUtility;
using PhysicsUtility.Kinematics;
using System;
using System.Threading.Tasks.Sources;

namespace Visualizer.FiniteElement
{
    class CubeStructure : ParticleStructure
    {
        public int EdgeCount { get; }
        public double EdgeLength { get; }
        public double Mass { get; }
        public double SpringConstant { get; }
        public CubeStructure(int edgeCount, double edgeLength, Vector center, Vector rot, double mass, double springConstant)
        {
            EdgeCount = edgeCount;
            EdgeLength = edgeLength;
            Mass = mass;
            SpringConstant = springConstant;

            Vector startPoint = center - new Vector(edgeLength / 2, edgeLength / 2, edgeLength / 2);
            Projectile[,,] projs = new Projectile[EdgeCount, EdgeCount, EdgeCount];

            //create point lattice
            for (int x = 0; x < EdgeCount; x++)
            {
                for (int y = 0; y < EdgeCount; y++)
                {
                    for (int z = 0; z < EdgeCount; z++)
                    {
                        var proj = new Projectile(EdgeLength / EdgeCount * new Vector(x, y, z) + startPoint, new(), mass / (EdgeCount * EdgeCount * EdgeCount));
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
                            AddConnector(projs[x, y, z], projs[x + 1, y, z], springConstant, EdgeLength / (EdgeCount - 1));
                        if (y < EdgeCount - 1)
                            AddConnector(projs[x, y, z], projs[x, y + 1, z], springConstant, EdgeLength / (EdgeCount - 1));
                        if (z < EdgeCount - 1)
                            AddConnector(projs[x, y, z], projs[x, y, z + 1], springConstant, EdgeLength / (EdgeCount - 1));
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
                            AddConnector(projs[x, y, z], projs[EdgeCount - 1, y, z], springConstant * 4, EdgeLength);
                        if (y < 1)
                            AddConnector(projs[x, y, z], projs[x, EdgeCount - 1, z], springConstant * 4, EdgeLength);
                        if (z < 1)
                            AddConnector(projs[x, y, z], projs[x, y, EdgeCount - 1], springConstant * 4, EdgeLength);
                    }
                }
            }

            //diagonal braces across 2x3 faces
            for (int y = 0; y < EdgeCount; y++)
            {
                for (int z = 1; z < EdgeCount; z++)
                {
                    AddConnector(projs[0, y, z], projs[EdgeCount - 1, y, z - 1], springConstant * 0.67, EdgeLength * Math.Sqrt(5));
                    AddConnector(projs[EdgeCount - 1, y, z], projs[0, y, z - 1], springConstant * 0.67, EdgeLength * Math.Sqrt(5));
                }
            }
        }
    }
}
