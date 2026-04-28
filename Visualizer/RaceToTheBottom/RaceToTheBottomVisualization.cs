using DongUtility;
using PhysicsUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualizerBaseClasses;
using VisualizerControl;
using VisualizerControl.Commands;
using VisualizerControl.Shapes;
using WPFUtility;
using static Visualizer.RaceToTheBottom.RaceToTheBottomEngine;
using static WPFUtility.UtilityFunctions;

namespace Visualizer.RaceToTheBottom
{
    /// <summary>
    /// A visualization for a kinematics engine
    /// </summary>
    class RaceToTheBottomVisualization : IVisualization
    {
        private readonly RaceToTheBottomEngine engine;

        private int counter = 0;
        private int projectileIndexOffset;

        private Dictionary<Point, int> pointIndices = new Dictionary<Point, int>();

        public RaceToTheBottomVisualization(RaceToTheBottomEngine engine)
        {
            this.engine = engine;
        }

        public bool Continue { get; private set; } = true;
        public double Time => engine.Time;

        public double ProjectileSize { get; set; } = 1;
        public double PathThickness { get; set; } = .5;

        public Color BoxColor { get; set; } = Colors.SlateGray;

        public double BoxSize { get; set; } = 50;

        public double PointSize { get; set; } = 1;

        public CommandSet<VisualizerControl.Visualizer> Initialization()
        {
            var set = new VisualizerCommandSet();

            var boxColor = Color.FromArgb(50, BoxColor.R, BoxColor.G, BoxColor.B);
            var box = new ObjectPrototype(new Cube3D(), new BasicMaterial(boxColor),
                new Vector3D(BoxSize,BoxSize, BoxSize), new Vector3D(BoxSize, BoxSize, BoxSize));
            set.AddCommand(new AddObject(box, counter++));

            // Add projectiles
            projectileIndexOffset = counter;
            foreach (var projectile in engine.ProjectilesAndPaths)
            {
                var projectileObject = new ObjectPrototype(new Sphere3D(),
                    new BasicMaterial(projectile.ProjectileColor),
                    ConvertToVector3D(projectile.Projectile.Position),
                    new Vector3D(ProjectileSize, ProjectileSize, ProjectileSize));
                set.AddCommand(new AddObject(projectileObject, counter++));
            }

            // Add paths
            foreach (var projectile in engine.ProjectilesAndPaths)
            {
                var shape = new FunctionShape3D(projectile.Path)
                {
                    CircleRadius = PathThickness
                };
                var path = new ObjectPrototype(shape, new BasicMaterial(projectile.PathColor));
                set.AddCommand(new AddObject(path, counter++));
            }

            // Add points
            foreach (var point in engine.Points)
            {
                var pointObject = new ObjectPrototype(new Sphere3D(), new BasicMaterial(point.Color),
                    point.Position, new Vector(PointSize, PointSize, PointSize));
                pointIndices.Add(point, counter);
                set.AddCommand(new AddObject(pointObject, counter++));
            }

            return set;
        }

        public CommandSet<VisualizerControl.Visualizer> Tick(double newTime)
        {
            Continue = engine.Increment(newTime - Time);
            
            var set = new VisualizerCommandSet();

            for (int i = 0; i < engine.Projectiles.Count; ++i)
            {
                set.AddCommand(new MoveObject(projectileIndexOffset + i, engine.Projectiles[i].Position));
            }

            foreach (var point in engine.RemovedPoints)
            {
                set.AddCommand(new RemoveObject(pointIndices[point]));
            }

            return set;
        }
    }
}
