using DongUtility;
using PhysicsUtility.Kinematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualizer.FiniteElement.AnswerKey
{
    internal class Cube : ParticleStructure
    {
        public Cube()
        {
            const double totalMass = 5;
            const double springConstant = 5e6;
            const double side = 1;
            const double initialHeight = 10;
            const int nParticles = 3;
            const double eachMass = totalMass / (nParticles * nParticles * nParticles);

            Projectile[,,] projectiles = new Projectile[nParticles, nParticles, nParticles];
            for (int ix = 0; ix < nParticles; ++ix)
                for (int iy = 0; iy < nParticles; ++iy)
                    for (int iz = 0; iz < nParticles; ++iz)
                    {

                        var proj = new Projectile(new Vector((double)ix / nParticles * side,
                            (double)iy / nParticles * side, (double)iz / nParticles * side + initialHeight), Vector.NullVector(), eachMass);
                        projectiles[ix, iy, iz] = proj;
                        //AddProjectile(new Vector(ix, iy, iz + initialHeight), eachMass);
                    }

            foreach (var proj in projectiles)
            {
                AddProjectile(proj);
            }

            for (int ix = 0; ix < nParticles; ++ix)
                for (int iy = 0; iy < nParticles; ++iy)
                    for (int iz = 0; iz < nParticles; ++iz)
                    {
                        if (ix < nParticles - 1)
                            AddConnector(projectiles[ix, iy, iz], projectiles[ix + 1, iy, iz], springConstant, 0);
                        if (iy < nParticles - 1)
                            AddConnector(projectiles[ix, iy, iz], projectiles[ix, iy + 1, iz], springConstant, 0);
                        if (iz < nParticles - 1)
                            AddConnector(projectiles[ix, iy, iz], projectiles[ix, iy, iz + 1], springConstant, 0);
                    }
        }
    }
}