using DongUtility;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// A class that runs a kinematics engine and writes the results to a file
    /// </summary>
    public class KinematicsEngineFileWriter
    {
        private readonly BinaryWriter writer;
        private readonly KinematicsEngine engine;

        public const string Header = "KinematicsEnginev1";

        public KinematicsEngineFileWriter(string filename, KinematicsEngine engine)
        {
            writer = new BinaryWriter(File.Create(filename));
            this.engine = engine;
            writer.Write(Header);
        }

        private bool wroteInitialConditions = false;

        /// <summary>
        /// Writes the initial conditions of the engine to the file
        /// </summary>
        public void WriteInitialConditions()
        {
            if (wroteInitialConditions)
            {
                return;
            }

            writer.Write(engine.Projectiles.Count);
            foreach (var projectile in engine.Projectiles)
            {
                writer.Write(projectile.Position);
                writer.Write(projectile.Velocity);
                writer.Write(projectile.Mass);
            }

            wroteInitialConditions = true;
        }

        /// <summary>
        /// Writes the current state of the engine to the file
        /// </summary>
        public void WriteCurrentPoint()
        {
            if (!wroteInitialConditions)
            {
                WriteInitialConditions();
            }

            writer.Write(engine.Time);
            foreach (var projectile in engine.Projectiles)
            {
                writer.Write(projectile.Position);
                writer.Write(projectile.Velocity);
            }
        }
    }
}
