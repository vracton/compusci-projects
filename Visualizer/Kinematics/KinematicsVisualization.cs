using DongUtility;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualizerBaseClasses;
using VisualizerControl;
using VisualizerControl.Commands;

namespace Visualizer.Kinematics
{
    /// <summary>
    /// A visualization for a kinematics engine
    /// </summary>
    class KinematicsVisualization(IEngine engine) : IVisualization
    {
        protected IEngine Engine { get; } = engine;
        private readonly List<IProjectile> projectiles = engine.Projectiles;

        /// <summary>
        /// Keeps track of projectile indices
        /// </summary>
        protected Dictionary<int, IProjectile> ProjectileMap { get; } = [];
        protected int Counter { get; set; } = 0;

        /// <summary>
        /// Keeps track of connector indices
        /// </summary>
        protected Dictionary<int, Connector> ConnectorMap { get; } = [];

        public bool Continue { get; private set; } = true;
        public double Time => Engine.Time;

        /// <summary>
        /// Determines the radius of the connector relative to the projectiles it connects.
        /// It is a scale factor for visualizing connectors.
        /// </summary>
        public double ConnectorRadiusScale { get; set; } = .2;

        /// <summary>
        /// Whether or not to draw in the ground
        /// </summary>
        public bool Ground { get; set; } = false;

        /// <summary>
        /// Add a connector that is anchored to a fixed point and another projectile
        /// </summary>
        public int AddAnchoredConnector(int projectileIndex, Vector3D anchorPoint, Color color)
        {
            var projectile = projectiles[projectileIndex];
            double scale = ConnectorRadiusScale * projectile.Size;
            var connector = new AnchoredConnector(scale, color, anchorPoint, projectile);
            ConnectorMap.Add(Counter, connector);
            return Counter++;
        }

        /// <summary>
        /// Add a connector that is anchored to two projectiles
        /// </summary>
        public int AddTwoParticleConnector(int projectileIndex1, int projectileIndex2, Color color)
        {
            var proj1 = projectiles[projectileIndex1];
            var proj2 = projectiles[projectileIndex2];
            double scale = (proj1.Size + proj2.Size) / 2 * ConnectorRadiusScale;
            var connector = new TwoProjectileConnector(scale, color, proj1, proj2);
            ConnectorMap.Add(Counter, connector);
            return Counter++;
        }

        public double GroundSize { get; set; } = 10;
        public Color GroundColor { get; set; } = Colors.SaddleBrown;

        public bool Box { get; set;  } = false;

        virtual public CommandSet<VisualizerControl.Visualizer> Initialization()
        {
            var set = new VisualizerCommandSet();

            // Add all the projectiles
            foreach (var projectile in projectiles)
            {
                // Start it off in the right place
                var obj = new ObjectPrototype(projectile.Shape, new BasicMaterial(projectile.Color, .05, .3),
                    projectile.Position, new Vector3D(projectile.Size, projectile.Size, projectile.Size));

                set.AddCommand(new AddObject(obj, Counter));
                ProjectileMap.Add(Counter, projectile);
                ++Counter;
            }

            const double nTurns = 10;

            // Add all the connectors
            foreach (var connector in ConnectorMap)
            {
                // Update them first so they are in the right position
                connector.Value.Update();
                var obj = new ObjectPrototype(new VisualizerControl.Shapes.Helix3D(1, new Geometry.Geometry2D.Point(0, 0), 1, new Geometry.Geometry3D.Point(0, 0, 0), nTurns), 
                    new BasicMaterial(connector.Value.Color, .05, .3));
                set.AddCommand(new AddObject(obj, connector.Key));
                set.AddCommand(connector.Value.GetTransformCommand(connector.Key));
            }

            // Add the ground
            if (Ground)
            {
                var ground = new ObjectPrototype(new VisualizerControl.Shapes.Quadrilateral3D(new Vector3D(-GroundSize, -GroundSize, 0),
                    new Vector3D(-GroundSize, GroundSize, 0), new Vector3D(GroundSize, GroundSize, 0), new Vector3D(GroundSize, -GroundSize, 0)),
                                       new BasicMaterial(GroundColor, .05, .3));
                set.AddCommand(new AddObject(ground, Counter));
                ++Counter;
            }

            if (Box)
            {
                var color = Colors.SlateGray;
                color.A = Constants.MaxByte / 2;
                var box = new ObjectPrototype(new VisualizerControl.Shapes.Cube3D(),
                                       new BasicMaterial(color, .05, .3), new Vector(0, 0, 0),
                                       new Vector(GroundSize / 2, GroundSize / 2, GroundSize / 2));
                set.AddCommand(new AddObject(box, Counter));
                ++Counter;
            }

            return set;
        }

        virtual public CommandSet<VisualizerControl.Visualizer> Tick(double newTime)
        {
            Continue = Engine.Tick(newTime);

            var set = new VisualizerCommandSet();

            // Change all the projectiles
            foreach (var entry in ProjectileMap)
            {
                set.AddCommand(new MoveObject(entry.Key, entry.Value.Position));
            }

            // Change all the connectors
            foreach (var connector in ConnectorMap)
            {
                connector.Value.Update();
                set.AddCommand(connector.Value.GetTransformCommand(connector.Key));
            }

            return set;
        }
    }
}
