using DongUtility;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Thermodynamics;
using VisualizerBaseClasses;
using VisualizerControl;
using VisualizerControl.Commands;
using VisualizerControl.Shapes;
using static WPFUtility.UtilityFunctions;

namespace Visualizer.Thermodynamics
{
    /// <summary>
    /// Adapted to use visualizer for thermodynamics project
    /// </summary>
    class ThermodynamicsVisualization : IVisualization
    {
        /// <summary>
        /// The underlying particle container
        /// </summary>
        private readonly ParticleContainer container;

        /// <summary>
        /// A map of particles to indices
        /// </summary>
        private Dictionary<Molecule, int> particleMap = new();

        public ThermodynamicsVisualization(ParticleContainer container)
        {
            this.container = container;
        }

        public bool Continue => Time < StopTime;
        public double Time { get; private set; }

        public double ParticleSize { get; set; } = 1;
        private int counter = 0;

        public double BoxScale { get; set; } = 1.1;

        /// <summary>
        /// The time at which the simulation will stop automatically
        /// </summary>
        public double StopTime { get; set; } = double.MaxValue;

        public CommandSet<VisualizerControl.Visualizer> Initialization()
        {
            var set = new VisualizerCommandSet();

            Color boxTransparentColor = Color.FromArgb((byte)80, BoxColor.R, BoxColor.G, BoxColor.B);
            var box = new ObjectPrototype(new Cube3D(), new BasicMaterial(boxTransparentColor, .05, .3),
                ConvertToVector3D(container.Size / 2), ConvertToVector3D(container.Size / 2 * BoxScale));

            set.AddCommand(new AddObject(box, counter));
            ++counter;

            // Add all the particles
            foreach (var particle in container.Particles)
            {
                // Start it off in the right place
                var obj = new ObjectPrototype(new Sphere3D(2), new BasicMaterial(ConvertColor(particle.Info.Color), .05, .3),
                    ConvertToVector3D(particle.Position), new Vector3D(ParticleSize, ParticleSize, ParticleSize));

                set.AddCommand(new AddObject(obj, counter));
                particleMap.Add(particle, counter);
                ++counter;
            }

            return set;
        }

        private void AddParticle(Molecule particle, VisualizerCommandSet set)
        {
            // Start it off in the right place
            var obj = new ObjectPrototype(particle.Info.Shape, new BasicMaterial(ConvertColor(particle.Info.Color), .05, .3),
                ConvertToVector3D(particle.Position), new Vector3D(ParticleSize, ParticleSize, ParticleSize));

            set.AddCommand(new AddObject(obj, counter));
            particleMap.Add(particle, counter);
            ++counter;
        }

        private void RemoveParticle(Molecule particle, VisualizerCommandSet set)
        {
            int index = particleMap[particle];
            set.AddCommand(new RemoveObject(index));
        }

        public Color BoxColor { get; set; } = Colors.SlateBlue;

        public CommandSet<VisualizerControl.Visualizer> Tick(double newTime)
        {
            var set = new VisualizerCommandSet();

            container.Update(newTime - Time);

            container.ParticlesToAdd.ForEach((particle) => AddParticle(particle, set));
            container.ParticlesToRemove.ForEach((particle) => RemoveParticle(particle, set));

            foreach (var particle in container.Particles)
            {
                int index = particleMap[particle];
                set.AddCommand(new MoveObject(index, ConvertToVector3D(particle.Position)));
            }

            // Adjust size of box if volume changes
            set.AddCommand(new TransformObject(0, container.Size / 2, container.Size / 2 * BoxScale, Rotation.Identity));

            Time = newTime;

            return set;
        }

        static private Vector3D ConvertToVector3D(Vector vec)
        {
            return new Vector3D(vec.X, vec.Y, vec.Z);
        }
    }
}
