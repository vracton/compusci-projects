//#define PARALLEL
#define NOEXCEPTIONS

using DongUtility;

namespace PhysicsUtility.Kinematics
{
    /// <summary>
    /// The class responsible for running the motion of all projectiles
    /// </summary>
    public class KinematicsEngine
    {
        /// <summary>
        /// The current time of the simulation
        /// </summary>
        public double Time { get; set; } = 0;

        private double oldTime = 0;
        public double DeltaTime => Time - oldTime;
        /// <summary>
        /// All the projectiles that are in motion
        /// </summary>
        public List<Projectile> Projectiles { get; } = [];
        /// <summary>
        /// All the forces that act on the particles
        /// </summary>
        protected List<Force> Forces { get; } = [];

        /// <summary>
        /// Gets all projectiles, including ones that are preprocessed and not actively managed by the engine
        /// </summary>
        public IEnumerable<Projectile> AllProjectiles
        {
            get
            {
                foreach (var projectile in Projectiles)
                {
                    yield return projectile;
                }
                foreach (var projectile in PreprocessedProjectiles)
                {
                    yield return projectile;
                }
            }
        }

        /// <summary>
        /// Add a projectile to the simulation
        /// </summary>
        public virtual void AddProjectile(Projectile projectile)
        {
            // I used to have this in to avoid duplicates but it takes too long for large numbers of projectiles
            //if (Projectiles.Contains(projectile))
            //{
            //    throw new InvalidOperationException("Attempted to add Projectile that already exists!");
            //}
            //else
            //{
            Projectiles.Add(projectile);
            //}
        }

        public void AddProjectiles(IEnumerable<Projectile> projectiles)
        {
            foreach (var projectile in projectiles)
            {
                AddProjectile(projectile);
            }
        }

        /// <summary>
        /// Add a Force object to apply to the simulation
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Force force)
        {
            Forces.Add(force);
        }

        private readonly List<StopCondition> stopConditions = [];

        /// <summary>
        /// Add a StopCondition to the engine
        /// </summary>
        public void AddStopCondition(StopCondition condition)
        {
            stopConditions.Add(condition);
        }

        /// <summary>
        /// Checks whether the simulation should stop
        /// </summary>
        protected bool CheckStopConditions()
        {
            // Check whether it's time to stop
            foreach (var condition in stopConditions)
            {
                if (!condition.ShouldContinue(this))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Runs one tick of the clock
        /// </summary>
        virtual public bool Increment(double timeIncrement)
        {
            // Increment time
            oldTime = Time;
            Time += timeIncrement;

            // Add all forces
#if PARALLEL
            Parallel.ForEach(Forces, (force) =>
#else
            foreach (var force in Forces)
#endif
            {
                force.AddForce(timeIncrement);
            }
#if PARALLEL
            );
#endif

            // Update all projectiles
#if PARALLEL
            Parallel.ForEach(Projectiles, (projectile) =>
#else
            foreach (var projectile in Projectiles)
#endif
            {
#if NOEXCEPTIONS
                try
                {
#endif
                    projectile.Update(timeIncrement);
#if NOEXCEPTIONS
                }
                catch (Exception)
                {
                    // Do nothing
                }
#endif
            }
#if PARALLEL
            );
#endif
            // Update preprocessed projectiles
            if (preprocessorReader != null)
            {
                UpdatePreprocessedProjectiles(Time);
            }

            return CheckStopConditions();
        }

        /// <summary>
        /// Preprocesses the simulation and writes it to a file
        /// </summary>
        public void Preprocess(string filename, double duration, double timeStep)
        {
            var file = new KinematicsEngineFileWriter(filename, this);
            file.WriteInitialConditions();

            while (Time < duration)
            {
                Console.WriteLine($"Processing time {Time}");
                bool keepGoing = Increment(timeStep);

                if (!keepGoing)
                {
                    break;
                }
                file.WriteCurrentPoint();
                Time += timeStep;
            }
        }

        private readonly List<Projectile> PreprocessedProjectiles = [];
        private BinaryReader? preprocessorReader;
        private double currentPreprocessedTime = 0;

        /// <summary>
        /// Reads in a file with information on preprocessed projectiles
        /// </summary>
        public void LoadPreprocessedProjectiles(string filename)
        {
            preprocessorReader = new BinaryReader(File.OpenRead(filename));

            // Check the header
            string header = preprocessorReader.ReadString();

            if (header != KinematicsEngineFileWriter.Header)
            {
                throw new FileLoadException("Improper file format!");
            }

            int nProjectiles = preprocessorReader.ReadInt32();
            for (int i = 0; i < nProjectiles; ++i)
            {
                Vector position = preprocessorReader.ReadVector();
                Vector velocity = preprocessorReader.ReadVector();
                double mass = preprocessorReader.ReadDouble();

                var newProjectile = new Projectile(position, velocity, mass);
                PreprocessedProjectiles.Add(newProjectile);
            }
        }

        /// <summary>
        /// Updates the preprocessed projectiles to the current time
        /// </summary>
        private void UpdatePreprocessedProjectiles(double newTime)
        {
            if (preprocessorReader == null)
            {
                return;
            }

            while (currentPreprocessedTime < newTime)
            {
                if (FileUtilities.IsEndOfFile(preprocessorReader))
                {
                    return;
                }

                try
                {
                    currentPreprocessedTime = preprocessorReader.ReadDouble();

                    foreach (var projectile in PreprocessedProjectiles)
                    {
                        Vector position = preprocessorReader.ReadVector();
                        Vector velocity = preprocessorReader.ReadVector();

                        projectile.Position = position;
                        projectile.Velocity = velocity;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// The center of mass position of all projectiles
        /// </summary>
        public Vector CMPosition
        {
            get
            {
                var response = new Vector(0, 0, 0);
                double totalMass = 0;
                foreach (var proj in AllProjectiles)
                {
                    response += proj.Mass * proj.Position;
                    totalMass += proj.Mass;
                }

                return response / totalMass;
            }
        }

        /// <summary>
        /// The velocity of the center of mass of all projectiles
        /// </summary>
        public Vector CMVelocity
        {
            get
            {
                var response = new Vector(0, 0, 0);
                double totalMass = 0;
                foreach (var proj in AllProjectiles)
                {
                    response += proj.Mass * proj.Velocity;
                    totalMass += proj.Mass;
                }

                return response / totalMass;

            }
        }

        /// <summary>
        /// The acceleration of the center of mass of all projectiles
        /// </summary>
        public Vector CMAcceleration
        {
            get
            {
                var response = new Vector(0, 0, 0);
                double totalMass = 0;
                foreach (var proj in AllProjectiles)
                {
                    response += proj.Mass * proj.Acceleration;
                    totalMass += proj.Mass;
                }

                return response / totalMass;
            }
        }
    }
}
