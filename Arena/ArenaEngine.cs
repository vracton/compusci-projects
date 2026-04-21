#define PARALLEL
#define NOEXCEPTIONS
#define GRID

using Arena.GraphicTurns;
using DongUtility;
using Geometry.Geometry2D;
using VisualizerBaseClasses;

namespace Arena
{
    /// <summary>
    /// The master engine base class for a 2-D arena
    /// </summary>
    abstract public class ArenaEngine : IEngine<IArenaDisplay, GraphicTurn>
    {
        /// <summary>
        /// The random number generator used for the arena
        /// </summary>
        static public Random Random => ThreadSafeRandom.Random(); 

        // This is in game coordinates
        public double Width { get; }
        public double Height { get; }

        /// <summary>
        /// A registry to keep track of all the bitmap files
        /// </summary>
        public Registry Registry { get; } = new Registry();

        /// <summary>
        /// All objects that remain in the background, like the backdrop
        /// </summary>
        public List<BackgroundObject> BackgroundObjects { get; } = [];
        /// <summary>
        /// All objects that are not moving, like walls
        /// </summary>
        public List<ArenaObject> StationaryObjects { get; } = [];
        /// <summary>
        /// All objects that move during most turned, like animals
        /// </summary>
        public List<MovingObject> UpdatingObjects { get; } = [];

        /// <summary>
        /// All objects that can interact (stationary or updating objects)
        /// </summary>
        public IEnumerable<ArenaObject> AllInteractingObjects
        {
            get
            {
                foreach (var obj in StationaryObjects)
                {
                    yield return obj;
                }
                foreach (var obj in UpdatingObjects)
                {
                    yield return obj;
                }
            }
        }
        /// <summary>
        /// All objects in the arena, including the background
        /// </summary>
        public IEnumerable<ArenaObject> AllObjects
        {
            get
            {
                foreach (var obj in AllInteractingObjects)
                {
                    yield return obj;
                }
                foreach (var obj in BackgroundObjects)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Gets all objects with a specific name
        /// </summary>
        public IEnumerable<ArenaObject> GetObjects(string name)
        {
            foreach (var obj in AllObjects)
            {
                if (obj.Name == name)
                    yield return obj;
            }
        }

        /// <summary>
        /// Returns the number of objects with a specific name
        /// </summary>
        public int CountObjects(string name)
        {
            return GetObjects(name).Count();
        }

        /// <summary>
        /// Gets all objects within a certain radius of a point
        /// </summary>
        public IEnumerable<ArenaObject> GetNearbyObjects(Point position, double radius)
        {
            return GetNearbyObjects<ArenaObject>(position, radius);
        }

        /// <summary>
        /// Gets all objects of type T within a given radius of the current object
        /// </summary>
        public IEnumerable<T> GetNearbyObjects<T>(Point position, double radius) where T : ArenaObject
        {
#if GRID
            return grid.GetNearby<T>(position, radius);
#else
            return GetObjectsOfType<T>();
#endif
        }
#if GRID
        /// <summary>
        /// A grid for efficiency
        /// </summary>
        private readonly ArenaGrid grid;
#endif
        /// <summary>
        /// All objects to be added or removed at the end of the turn
        /// </summary>
        private readonly List<ArenaObject> toBeAdded = [];
        private readonly List<ArenaObject> toBeRemoved = [];

        /// <summary>
        /// The game time
        /// </summary>
        public double Time { get; set; } = 0;

        /// <summary>
        /// This ust be set to true to end the simulation
        /// </summary>
        private bool endSimulation = false;

        /// <summary>
        /// Used to pause the simulation
        /// </summary>
        public bool IsPaused { get; set; } = false;

        /// <summary>
        /// The current set of turns being executed or worked on 
        /// </summary>
        public GraphicTurnSet TurnSet { get; private set; } = new GraphicTurnSet();

        /// <summary>
        /// All the statistics for the current turn
        /// </summary>
        public TurnStatistics Statistics { get; } = new TurnStatistics();

        /// <summary>
        /// Tests whether a point is occupied already by another impassable object
        /// </summary>
        /// <param name="whoWantsToKnow">Your own object</param>
        public bool IsOccupied(Point location, ArenaObject? whoWantsToKnow = null)
        {
            double radius = whoWantsToKnow == null ? double.Epsilon
                : whoWantsToKnow.Shape.MaxRadius;
            foreach (var obj in GetNearbyObjects(location, radius))
            {
                if (obj != whoWantsToKnow && obj.Occupies(location, whoWantsToKnow))
                    return true;
            }
            return false;
        }

        public bool IsOccupied(Shape2D area, ArenaObject? whoWantsToKnow = null)
        {
            double radius = area.MaxRadius;
            foreach (var obj in GetNearbyObjects(area.Center, radius))
            {
                if (whoWantsToKnow != null && whoWantsToKnow.Code == obj.Code)
                    continue;
                if (!obj.IsPassable(whoWantsToKnow) && obj.Shape.Intersects(area))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if this point is unoccupied and within the arena bounds
        /// </summary>
        public bool IsValidLocation(Point point)
        {
            return !IsOccupied(point) && TestPoint(point);
        }

        /// <summary>
        /// Checks if this area is completely unoccupied and completely within the arena bounds
        /// </summary>
        public bool IsValidLocation(Shape2D area)
        {
            return !IsOccupied(area) && TestShape(area);
        }

        public ArenaEngine(double width, double height, string backgroundFilename, int xDivs = 10, int yDivs = 10)
        {
            Width = width;
            Height = height;
#if GRID
            grid = new ArenaGrid(this, xDivs, yDivs);
#endif

            BackgroundGraphicsCode = Registry.AddEntry(new GraphicInfo(backgroundFilename, width, height));
            AddObject(new BackgroundObject(BackgroundGraphicsCode), new Point(Width / 2, Height / 2));

            Initialize();
        }

        /// <summary>
        /// The graphics code for the background image
        /// </summary>
        public int BackgroundGraphicsCode { get; }

        /// <summary>
        /// Initializes the arena with any objects or settings that are needed
        /// </summary>
        abstract public void Initialize();

        /// <summary>
        /// Returns a set of commands to set the initial conditions of the arena
        /// </summary>
        public CommandSet<IArenaDisplay> Initialization()
        {
            var graphics = new GraphicTurnSet();

            foreach (var org in AllObjects)
            {
                graphics.AddCommand(new AddObject(org));
            }

            return graphics;
        }

        /// <summary>
        /// Returns true if the simulation should continue
        /// </summary>
        public bool Continue => !endSimulation;

        /// <summary>
        /// Adds a random object of type T to the arena at a random location
        /// </summary>
        public void AddObjectRandom<T>() where T : ArenaObject, new()
        {
            T newObj = new();
            AddObjectRandom(newObj);
        }

        /// <summary>
        /// Adds a number of random objects of type T to the arena at random locations
        /// </summary>
        public void AddObjectRandom<T>(int num) where T : ArenaObject, new()
        {
            for (int i = 0; i < num; ++i)
            {
                AddObjectRandom<T>();
            }
        }

        /// <summary>
        /// Adds a specific object at a random position
        /// </summary>
        public void AddObjectRandom(ArenaObject obj)
        {
            double x = Random.NextDouble(0, Width);
            double y = Random.NextDouble(0, Height);

            AddObject(obj, new Point(x, y));
        }

        /// <summary>
        /// Adds an object at the end of the turn
        /// </summary>
        public void AddObjectDelay(ArenaObject obj)
        {
            lock (toBeAdded)
            {
                toBeAdded.Add(obj);
            }
        }

        /// <summary>
        /// Removes an object at the end of the turn
        /// </summary>
        public void RemoveObjectDelay(ArenaObject obj)
        {
            lock (toBeRemoved)
            {
                toBeRemoved.Add(obj);
            }
        }

        /// <summary>
        /// Adds an object to the arena at a specific location
        /// </summary>
        public void AddObject(ArenaObject obj, Point location)
        {
            if (obj == null)
                return;

            obj.Position = location;
            obj.Arena = this;
            if (obj is MovingObject @object)
            {
                UpdatingObjects.Add(@object);
            }
            else
            {
                if (obj is BackgroundObject object1)
                {
                    BackgroundObjects.Add(object1);
                }
                else
                {
                    StationaryObjects.Add(obj);
                }
            }
#if GRID
            grid.AddObject(obj);
#endif
            TurnSet.AddCommand(new AddObject(obj));
        }

        /// <summary>
        /// Get all objects of a specific type
        /// </summary>
        public IEnumerable<T> GetObjectsOfType<T>() where T : ArenaObject
        {
            foreach (var obj in AllObjects)
                if (obj is T targetObj)
                    yield return targetObj;
        }

        /// <summary>
        /// Removes an object from the arena
        /// </summary>
        public void RemoveObject(ArenaObject obj)
        {
            if (obj == null) return;

            if (obj is MovingObject obj1)
                UpdatingObjects.Remove(obj1);
            else if (obj is BackgroundObject obj2)
                BackgroundObjects.Remove(obj2);
            else
                StationaryObjects.Remove(obj);
#if GRID
            grid.RemoveObject(obj);
#endif
            TurnSet.AddCommand(new RemoveObject(obj));
        }

        /// <summary>
        /// Moves an existing object to a specific point
        /// </summary>
        public bool MoveObject(ArenaObject obj, Point target)
        {
            var newShape = obj.Shape.TranslateToPoint(target);
            var rect = !TestShape(newShape);
            var occupied = IsOccupied(newShape, obj);
            if (obj == null || rect || occupied)
            {
                return false;
            }
            else
            {
#if GRID
                grid.MoveObject(obj, target);
#else
                obj.Position = target;
#endif

                TurnSet.AddCommand(new MoveObject(obj, target));
                return true;
            }
        }

        /// <summary>
        /// Rotates an object by a specific angle
        /// </summary>
        public bool RotateObject(ArenaObject obj, double angle)
        {
            var newShape = obj.Shape.Rotate(angle, obj.Shape.Center);
            var rect = !TestShape(newShape);
            var occupied = IsOccupied(newShape, obj);
            if (obj == null || rect || occupied)
            {
                return false;
            }
            else
            {
                obj.Shape = newShape;

                TurnSet.AddCommand(new RotateObject(obj, angle));
                return true;
            }
        }

        /// <summary>
        /// Ends the simulation
        /// </summary>
        public void EndSimulation()
        {
            endSimulation = true;
        }

        /// <summary>
        /// Advance time to the next tick
        /// </summary>
        /// <param name="newTime">The new time to arrive at</param>
        /// <returns>All commands that result from this tick</returns>
        public CommandSet<IArenaDisplay> Tick(double newTime)
        {
            Time = newTime;
            TurnSet = new GraphicTurnSet();
            Statistics.ClearStatistics();

            BeginningOfTurn();

            UpdatingObjects.Shuffle(Random);

            DoTurn();

            EndOfTurn();

            CleanUp();

            if (Done())
            {
                endSimulation = true;
            }

            return TurnSet;
        }

        /// <summary>
        /// Call all actions that have to execute before the next turn starts
        /// </summary>
        private void BeginningOfTurn()
        {
#if PARALLEL
            Parallel.ForEach(UpdatingObjects, obj => DoWithExceptions(() => obj.BeginningOfTurn()));
#else
            foreach (var obj in UpdatingObjects)
            {
                DoWithExceptions(() => obj.BeginningOfTurn());
            }
#endif
            UserDefinedBeginningOfTurn();
        }

        /// <summary>
        /// An optional function to allow a user to define an action at the beginning of a turn
        /// </summary>
        protected virtual void UserDefinedBeginningOfTurn() { }
        /// <summary>
        /// An optional function to allow a user to define an action at the end of a turn
        /// </summary>
        protected virtual void UserDefinedEndOfTurn() { }

        /// <summary>
        /// Make all updating objects choose their turn, then execute it (in a shuffled order)
        /// </summary>
        private void DoTurn()
        {
#if PARALLEL
            Parallel.ForEach(UpdatingObjects, obj =>
            {
                DoWithExceptions(() => obj.ChooseAction());
            });
#else
            foreach (var obj in UpdatingObjects)
            {
                //Console.WriteLine("updating object " + obj.Name + " " + obj.Position);
                DoWithExceptions(() => obj.ChooseAction());
            }
#endif
            foreach (var obj in UpdatingObjects)
            {
                DoWithExceptions(() => obj.ExecuteAction());
            }
        }

        /// <summary>
        /// Execute end-of-turn actions for all objects and the arena
        /// </summary>
        private void EndOfTurn()
        {

#if PARALLEL
            Parallel.ForEach(UpdatingObjects, obj => DoWithExceptions(() => obj.EndOfTurn()));
#else
            foreach (var obj in UpdatingObjects)
            {
                DoWithExceptions(() => obj.EndOfTurn());
            }
#endif
            UserDefinedEndOfTurn();
        }

        /// <summary>
        /// Final clean up of objects to be added and removed at the end of the turn
        /// </summary>
        private void CleanUp()
        {
            foreach (var obj in toBeRemoved)
            {
                if (obj != null)
                    DoWithExceptions(() => RemoveObject(obj));
            }
            toBeRemoved.Clear();

            foreach (var obj in toBeAdded)
            {
                if (obj != null)
                    DoWithExceptions(() => AddObject(obj, obj.Position));
            }
            toBeAdded.Clear();
        }

        /// <summary>
        /// A very simple delegate to allow for exception handling
        /// </summary>
        private delegate void GenericFunction();

        /// <summary>
        /// Executes a function and catches any exceptions that occur,so student code doesn't crash the program
        /// </summary>
        /// <param name="func"></param>
        private static void DoWithExceptions(GenericFunction func)
        {
#if NOEXCEPTIONS
            try
            {
#endif
            func();
#if NOEXCEPTIONS
            }
            catch (Exception)
            { Console.WriteLine("Caught exception!"); }
#endif
        }

        /// <summary>
        /// Checks if the simulation is done; always returns false by default
        /// </summary>
        virtual protected bool Done()
        {
            return false;
        }

        /// <summary>
        /// Tests if a point is within the arena bounds
        /// </summary>
        public bool TestPoint(Point coord)
        {
            return coord.X >= 0 && coord.X <= Width && coord.Y >= 0 && coord.Y <= Height;
        }

        /// <summary>
        /// Tests if a shape is within the arena bounds
        /// </summary>
        public bool TestShape(Shape2D shape)
        {
            // Just treat the arena as a huge rectangle
            var arenaShape = new AlignedRectangle(new Point(0, 0), new Point(Width, Height));
            return arenaShape.Contains(shape);
        }
    }
}
