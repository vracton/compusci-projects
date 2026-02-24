using DongUtility;
using Visualizer.FiniteElement;
using PhysicsUtility.Kinematics;

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
            const double eachMass = .1; //.005 / 27;
            const double springConstant = 500;
            const double side = .01;
            const int nParticles = 1;
            const double initialHeight = .5;

            Projectile[,,] projectiles = new Projectile[nParticles, nParticles, nParticles];
            for (int ix = 0; ix < nParticles; ++ix)
                for (int iy = 0; iy < nParticles; ++iy)
                    for (int iz = 0; iz < nParticles; ++iz)
                    {
                        var proj = new Projectile(new Vector((double)ix / nParticles * side,
                            (double)iy / nParticles * side, (double)iz / nParticles * side + initialHeight),
                            Vector.NullVector(), eachMass);
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
                            AddConnector(projectiles[ix, iy, iz], projectiles[ix + 1, iy, iz], springConstant, side / (nParticles - 1));
                        if (iy < nParticles - 1)
                            AddConnector(projectiles[ix, iy, iz], projectiles[ix, iy + 1, iz], springConstant, side / (nParticles - 1));
                        if (iz < nParticles - 1)
                            AddConnector(projectiles[ix, iy, iz], projectiles[ix, iy, iz + 1], springConstant, side / (nParticles - 1));
                    }
        }
    }
}
