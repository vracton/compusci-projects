using DongUtility;
using Visualizer.FiniteElement;
using PhysicsUtility.Kinematics;
using System;
using System.Collections.Generic;

namespace Visualizer.MarbleMadness
{
    /// <summary>
    /// Your particle structure - used for you to simulate the marble
    /// Feel free to modify as much as you want - this part will not be used by my simulation
    /// </summary>
    class YourParticleStructure : ParticleStructure
    {
        public YourParticleStructure()
        {
            const double totalMass = 5;
            Vector start = new(0, 0.45, 0.5);
            const double s = 200.0;

            //center
            Projectile center = new(start, new(), totalMass / 2);
            AddProjectile(center);
            //layer 2
            //var (icoVerts1, projs1) = CreateNewSubIco(start, 1, 0.01, totalMass / 4, s);
            //layer 3
            //var (icoVerts2, projs2) = CreateNewSubIco(start, 1, 0.01, totalMass / 4, s);

            //connect opposite pairs in layer 2
            //for (int i = 0; i < icoVerts1.Count; i++)
            //{
            //    int oppIndex = icoVerts1.IndexOf(-icoVerts1[i]);
            //    if (i > oppIndex)
            //    {
            //        AddConnector(projs1[i], projs1[oppIndex], s, Vector.Distance(projs1[i].Position, projs1[oppIndex].Position));
            //    }
            //}

            //layer connections
            //foreach (var p in projs1)
            //{
            //    AddConnector(center, p, s * 2, Vector.Distance(center.Position, p.Position));
            //}
            //for (int i = 0; i < icoVerts1.Count; i++)
            //{
            //    AddConnector(projs1[i], projs2[i], s, Vector.Distance(projs1[i].Position, projs2[i].Position));
            //}
        }

        (List<Vector>, Projectile[]) CreateNewSubIco(Vector center, int divs, double radius, double mass, double springConstant)
        {
            double phi = (1 + Math.Sqrt(5)) / 2;
            List<Vector> verts = new();
            List<(int, int, int)> faces = new();

            //12 vertices of regular icosahedron
            verts.AddRange(new Vector[]
            {
                new(-1, phi, 0), new(1, phi, 0), new(-1, -phi, 0), new(1, -phi, 0),
                new(0, -1, phi), new(0, 1, phi), new(0, -1, -phi), new(0, 1, -phi),
                new(phi, 0, -1), new(phi, 0, 1), new(-phi, 0, -1), new(-phi, 0, 1)
            });

            faces.AddRange(new[]
            {
                (0,11,5),(0,5,1),(0,1,7),(0,7,10),(0,10,11),
                (1,5,9),(5,11,4),(11,10,2),(10,7,6),(7,1,8),
                (3,9,4),(3,4,2),(3,2,6),(3,6,8),(3,8,9),
                (4,9,5),(2,4,11),(6,2,10),(8,6,7),(9,8,1)
            });

            //subdivs
            Dictionary<int, int> midpointCache = new();
            int getMidpointInd(int x, int y)
            {
                int hash = HashCode.Combine(Math.Min(x, y), Math.Max(x, y));
                if (midpointCache.ContainsKey(hash))
                    return midpointCache[hash];

                Vector midpoint = (verts[x] + verts[y]) * 0.5;
                midpointCache[hash] = verts.Count;
                verts.Add(midpoint);
                return midpointCache[hash];
            }

            for (int n = 0; n < divs; n++)
            {
                List<(int, int, int)> newFaces = new();
                foreach (var (a, b, c) in faces)
                {
                    int ab = getMidpointInd(a, b);
                    int bc = getMidpointInd(b, c);
                    int ca = getMidpointInd(c, a);
                    newFaces.Add((a, ab, ca));
                    newFaces.Add((b, bc, ab));
                    newFaces.Add((c, ca, bc));
                    newFaces.Add((ab, bc, ca));
                }
                faces = newFaces;
            }

            //add projectiles + normalize on sphere
            Projectile[] projectiles = new Projectile[verts.Count];
            foreach (var vert in verts)
            {
                Vector pos = center + vert.UnitVector() * radius;
                Projectile p = new(pos, new(), mass / verts.Count);
                projectiles[verts.IndexOf(vert)] = p;
                AddProjectile(p);
            }

            //face connections
            foreach (var (a, b, c) in faces)
            {
                AddConnector(projectiles[a], projectiles[b], springConstant, Vector.Distance(projectiles[a].Position, projectiles[b].Position));
                AddConnector(projectiles[b], projectiles[c], springConstant, Vector.Distance(projectiles[b].Position, projectiles[c].Position));
                AddConnector(projectiles[c], projectiles[a], springConstant, Vector.Distance(projectiles[c].Position, projectiles[a].Position));
            }

            return (verts, projectiles);
        }
    }
}
