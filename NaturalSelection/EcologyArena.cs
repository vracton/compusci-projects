using System;
using System.Collections.Generic;
using System.Linq;
using Arena;
using DongUtility;

namespace NaturalSelection
{
    public class EcologyArena : ArenaEngine
    {
        public EcologyArena(double x, double y) :
            base(x, y, "dirt.jpg")
        {
            Registry.Initialize(@"NaturalSelection\", @"Images\");

            Registry.AddEntry(new GraphicInfo("dirttex.jpg", 1, 1));
            Registry.AddEntry(new GraphicInfo("dry.jpg", 1, 1));
            Registry.AddEntry(new GraphicInfo("lowGrasstex.jpg", 1, 1));
            Registry.AddEntry(new GraphicInfo("mediumGrasstex.jpg", 1, 1));
            Registry.AddEntry(new GraphicInfo("highGrasstex.jpg", 1, 1));

            Registry.AddEntry(new GraphicInfo("rabbit.png", .2, .2));
            Registry.AddEntry(new GraphicInfo("lynx.png", .2, .2));

            for (int ix = 0; ix < Width; ++ix)
                for (int iy = 0; iy < Height; ++iy)
                {
                    AddCell(ix, iy);
                }
        }

        public void AddAnimals<T>(int nAnimals) where T : EcologyAnimal, new()
        {
            for (int i = 0; i < nAnimals; ++i)
            {
                var newAnimal = new T();
                newAnimal.SetRandomAge();
                AddObjectRandom(newAnimal);
            }
        }

        private readonly Dictionary<Coordinate2D, FoodCell> cells = [];

        private void AddCell(int x, int y)
        {
            var cell = new FoodCell();
            AddObject(cell, new Geometry.Geometry2D.Point(x + .5, y + .5));
            cells.Add(new Coordinate2D(x, y), cell);
        }

        public override void Initialize()
        { }

        public FoodCell CurrentCell(Vector2D location)
        {
            var coord = new Coordinate2D((int)Math.Floor(location.X), (int)Math.Floor(location.Y));
            return cells[coord];
        }

        protected override void UserDefinedBeginningOfTurn()
        {
            foreach (var cell in cells.Values)
            {
                cell.BeginningOfTurnGrowth();
            }

            base.UserDefinedBeginningOfTurn();
        }

        protected override bool Done()
        {
            return !GetObjectsOfType<MovingObject>().Any();
        }
    }
}
