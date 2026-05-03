using DongUtility;
using System;
using System.Collections.Generic;
using DecisionTree;
using System.Windows.Media;
using System.IO;
using Path = DongUtility.Path;
using PhysicsUtility.Kinematics;

namespace Visualizer.RaceToTheBottom
{
    class RaceToTheBottomEngine : KinematicsEngine
    {
        public class ProjectileAndPath
        {
            public string Name { get; }
            public ConstrainedProjectile Projectile { get; }
            public Path Path { get; }
            public double CurrentParameter { get; set; }
            public double PreviousParameter { get; set; }
            public Vector PreviousPosition { get; set; }
            public Color ProjectileColor { get; }
            public Color PathColor { get; }
            public bool Done { get; set; } = false;
            public int Score { get; set; } = 0;
            public ProjectileAndPath(string name, ConstrainedProjectile projectile, Color projectileColor, Color pathColor)
            {
                Name = name;
                Projectile = projectile;
                Path = projectile.Path;
                ProjectileColor = projectileColor;
                PathColor = pathColor;
                CurrentParameter = Path.InitialParameter;
                PreviousParameter = CurrentParameter;
                PreviousPosition = Projectile.Position;
            }
        }

        public class Point(Vector position, Color color)
        {
            public Vector Position { get; } = position;
            public Color Color { get; } = color;
            public bool IsSignal { get; set; } = false;
        }

        public List<ProjectileAndPath> ProjectilesAndPaths { get; } = [];
        public List<Point> Points { get; } = [];
        public HashSet<Point> RemovedPoints { get; } = [];

        public Vector GravitationalFieldStrength { get; set; } = new Vector(0, 0, -9.8);
        private const double MinimumDistance = .01;
        private const double MinimumDistanceSquared = MinimumDistance * MinimumDistance;
        public RaceToTheBottomEngine(string filename)
        {
            AddDataSet(filename, false);
        }

        public RaceToTheBottomEngine(string backgroundFilename, string signalFilename)
        {
            AddDataSet(backgroundFilename, false);
            AddDataSet(signalFilename, true);
        }

        private void AddDataSet(string filename, bool isSignal)
        {
            var dataset = DataSet.ReadDataSet(filename);
            foreach (var dataPoint in dataset.Points)
            {
                Vector position = new(dataPoint.Variables[0], dataPoint.Variables[1], dataPoint.Variables[2]);
                Color color = Color.FromRgb((byte)dataPoint.Variables[3], (byte)dataPoint.Variables[4], (byte)dataPoint.Variables[5]);
                var point = new Point(position, color)
                {
                    IsSignal = isSignal
                };
                Points.Add(point);
            }
        }

        public void AddSignalListDTFormat(string filename)
        {
            using var file = File.OpenText(filename);

            while (!file.EndOfStream)
            {
                var line = file.ReadLine();
                if (line == null)
                {
                    continue;
                }
                var segments = line.Split('\t');

                if (int.TryParse(segments[0], out int index))
                {
                    if (index >= Points.Count)
                        throw new FileFormatException("Index number too large!");

                    if (segments[3].Equals("TRUE", StringComparison.CurrentCultureIgnoreCase))
                        Points[index].IsSignal = true;
                }
                else if (line != null && line.Length > 0)
                {
                    throw new FileFormatException("Error in signal file!");
                }

            }
        }

        public void AddSignalList(string filename)
        {
            using var file = File.OpenText(filename);

            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                if (int.TryParse(line, out int index))
                {
                    if (index >= Points.Count)
                        throw new FileFormatException("Index number too large!");

                     Points[index].IsSignal = true;
                }
                else if (line != null && line.Length > 0)
                {
                    throw new FileFormatException("Error in signal file!");
                }

            }
        }

        public bool RemovePointsAfterHit { get; set; } = true;

        public void AddProjectile(string name, ConstrainedProjectile projectile, Color projectileColor, Color pathColor)
        {
            projectile.Position = projectile.Path.GetPosition(projectile.Path.InitialParameter);
            var pAndP = new ProjectileAndPath(name, projectile, projectileColor, pathColor);
            ProjectilesAndPaths.Add(pAndP);
        }

        public void AddProjectileAndPath(ProjectileAndPath pnp)
        {
            ProjectilesAndPaths.Add(pnp);
            AddProjectile(pnp.Projectile);
        }

        private static readonly Random Random = new();

        public override bool Increment(double timeIncrement)
        {
            bool stillGoing = false;
            foreach (var pandp in ProjectilesAndPaths)
            {
                if (pandp.Done)
                    continue;

                stillGoing = true;
                pandp.PreviousPosition = pandp.Projectile.Position;
                pandp.PreviousParameter = pandp.CurrentParameter;
            }

            Projectiles.Shuffle(Random);

            bool shouldContinue = base.Increment(timeIncrement);

            foreach (var pandp in ProjectilesAndPaths)
            { 
                if (pandp.Projectile.Parameter >= pandp.Projectile.Path.FinalParameter)
                    pandp.Done = true;
            }

            RemovedPoints.Clear();

            // Award points
            foreach (var pandp in ProjectilesAndPaths)
            {
                if (pandp.Done)
                    continue;

                Score(pandp);
            }

            // Removed points that were touched
            foreach (var point in RemovedPoints)
            {
                Points.Remove(point);
            }

            return shouldContinue && stillGoing;
        }

        /// <summary>
        /// Calculates the score for a ProjectileAndPath object, based on current and past positions
        /// </summary>
        private void Score(ProjectileAndPath pnp)
        {
            Vector direction = pnp.Projectile.Position - pnp.PreviousPosition;
            double distance = direction.Magnitude;
            if (distance == 0)
                return;
            direction /= distance;

            var alreadyHit = new HashSet<Point>();

            for (double increase = 0; increase < distance; increase += MinimumDistance / 2)
            {
                Vector iPosition = pnp.PreviousPosition + direction * increase;

                foreach (var point in Points)
                {
                    double distance2 = Vector.Distance2(point.Position, iPosition);
                    if (distance2 <= MinimumDistanceSquared && !alreadyHit.Contains(point))
                    {
                        pnp.Score += point.IsSignal ? 1 : -1;
                        if (RemovePointsAfterHit)
                        {
                            RemovedPoints.Add(point);
                        }
                        alreadyHit.Add(point);
                    }
                }
            }
        }
    }
}
