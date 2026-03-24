using DongUtility;
using PhysicsUtility.Kinematics;
using System.Collections.Generic;
using System.Windows.Media;
using VisualizerBaseClasses;
using VisualizerControl;
using VisualizerControl.Commands;
using VisualizerControl.Shapes;

namespace Visualizer.FastestDescent
{
    /// <summary>
    /// A visualization for a kinematics engine
    /// </summary>
    class DescentVisualization(KinematicsEngine engine, Path path) : IVisualization
    {
        private int counter = 0;
        private readonly List<int> projectileIndices = [];

        public bool Continue { get; private set; } = true;
        public double Time => engine.Time;

        public Color ProjectileColor { get; set; } = Colors.IndianRed;
        public double ProjectileSize { get; set; } = 1;
        public Color PathColor { get; set; } = Colors.NavajoWhite;
        public double PathThickness { get; set; } = .5;

        public CommandSet<VisualizerControl.Visualizer> Initialization()
        {
            var set = new VisualizerCommandSet();

            foreach (var projectile in engine.Projectiles)
            {
                var newObj = new ObjectPrototype(new Sphere3D(), new BasicMaterial(ProjectileColor, .3, .1),
                    projectile.Position, new Vector(ProjectileSize, ProjectileSize, ProjectileSize));
                set.AddCommand(new AddObject(newObj, counter));
                projectileIndices.Add(counter++);
            }

            var shape = new FunctionShape3D(path)
            {
                CircleRadius = PathThickness
            };
            var newPath = new ObjectPrototype(shape, new BasicMaterial(PathColor, .3, .1));
            set.AddCommand(new AddObject(newPath, counter));

            return set;
        }

        public CommandSet<VisualizerControl.Visualizer> Tick(double newTime)
        {
            Continue = engine.Increment(newTime - engine.Time);
            
            var set = new VisualizerCommandSet();

            for (int i = 0; i < engine.Projectiles.Count; ++i)
            {
                set.AddCommand(new MoveObject(projectileIndices[i], engine.Projectiles[i].Position));
            }

            return set;
        }
    }
}
