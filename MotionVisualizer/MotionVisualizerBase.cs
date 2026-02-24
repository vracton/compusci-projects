using GraphControl;
using GraphData;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Threading;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    /// <summary>
    /// The base class for anything that visualizes motion, regardless of where the data comes from or how many dimensions the visualization is in
    /// </summary>
    /// <typeparam name="TVisualizer">The type of visualizer</typeparam>
    /// <typeparam name="TCommand">The type of commands that are sent to the visualizer</typeparam>
    public class MotionVisualizerBase<TVisualizer, TCommand> : Window
        where TVisualizer : new()
        where TCommand : ICommand<TVisualizer>
    {
        /// <summary>
        /// The GraphDataManager which handles creation and updating of graphs and histograms
        /// </summary>
        public GraphDataManager Manager { get; } = new GraphDataManager();
        private IGraphDataInterface? GraphManagerInterface = null;
        private GraphManager? manager = null;


        private readonly EngineCore<TVisualizer, TCommand>? core = null;
        public TVisualizer? Visualizer { get; set; } = default;
        /// <summary>
        /// A collection of all the graph-like objects in this visualizer
        /// </summary>
        public CompositeGraph Graphs { get; } = new CompositeGraph();

        /// <summary>
        /// Constructor for real-time engine
        /// </summary>
        public MotionVisualizerBase(IEngine<TVisualizer, TCommand> engine, TVisualizer visualizer)
        {
            core = new RealTimeEngineCore<TVisualizer, TCommand>(engine, Manager);
            Visualizer = visualizer;
            FinishInitialization();
        }

        /// <summary>
        /// Constructor for reading from a file
        /// </summary>
        public MotionVisualizerBase(string filename, ICommandFileReader<TVisualizer> factory, TVisualizer visualizer)
        {
            core = new FromFileEngineCore<TVisualizer, TCommand>(filename, factory);
            Visualizer = visualizer;
            FinishInitialization();
        }

        /// <summary>
        /// Hybrid constructor that has some real-time elements and some from a file
        /// </summary>
        public MotionVisualizerBase(string filename, ICommandFileReader<TVisualizer> factory, IEngine<TVisualizer, TCommand> engine, TVisualizer visualizer)
        {
            core = new HybridEngineCore<TVisualizer, TCommand>(filename, factory, engine, Manager);
            Visualizer = visualizer;
            FinishInitialization();
        }

        private void FinishInitialization()
        {
            ContentRendered += LinkManager;
        }

        /// <summary>
        /// The event handler for when the initialization of the visualization is complete
        /// </summary>
        public event EventHandler? FinishedInitialization;

        /// <summary>
        /// Connects the GraphManager to the GraphDataManager and initializes everything
        /// </summary>
        public void LinkManager(object? sender, EventArgs e)
        {
            if (Visualizer == null || core == null)
            {
                throw new Exception("Visualizer or core is null in MotionVisualizerBase");
            }
            GraphManagerInterface = core.Initialize(Visualizer);
            manager = new GraphManager(GraphManagerInterface, Graphs);

            manager.Initialize();
            OnFinishedInitialization(EventArgs.Empty);
            AlreadyFinishedInitialization = true;
        }

        /// <summary>
        /// Checks whether initialization has already been finished
        /// </summary>
        public bool AlreadyFinishedInitialization { get; private set; } = false;

        /// <summary>
        /// The method that raises the FinishedInitialization event
        /// </summary>
        protected virtual void OnFinishedInitialization(EventArgs e)
        {
            FinishedInitialization?.Invoke(this, e);
        }

        /// <summary>
        /// Updates the time display - overriden in derived classes
        /// </summary>
        virtual public void UpdateTime(double time)
        { }

        // To keep time
        private readonly Stopwatch timer = new();

        // For multithreading communications
        private readonly BufferBlock<PackagedCommands<TVisualizer>> turnBuffer = new();

        /// <summary>
        /// Gets and sets whether the simulation is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return timer.IsRunning;
            }
            set
            {
                if (value)
                    timer.Start();
                else
                    timer.Stop();
            }
        }

        /// <summary>
        /// The maximum buffer size - otherwise you get memory overruns.
        /// When you reach this many packets in the buffer, the engine will pause until the buffer drops down to SizeToResume
        /// </summary>
        virtual public int MaxBufferSize { get; set; } = 500;

        /// <summary>
        /// The size at which the engine will resume running after being paused for buffer overflow
        /// </summary>
        virtual public int SizeToResume { get; set; } = 300;

        /// <summary>
        /// The current time to display
        /// </summary>
        virtual public double DisplayTime { get; private set; } = 0;

        /// <summary>
        /// The time increment for each step of the engine
        /// </summary>
        virtual public double TimeIncrement { get; set; } = 0;

        private double time = 0;

        /// <summary>
        /// Sets the initial time for the simulation
        /// </summary>
        public void SetInitialTime(double timeValue)
        {
            time = timeValue;
        }

        /// <summary>
        /// Start or continue the engine running
        /// </summary>
        private void RunEngine()
        {
            while (IsRunning)
            {
                time += TimeIncrement;

                var package = core?.NextCommand(time);

                // Stop in an empty loop until the turnBuffer has dropped down in size.
                // Otherwise you get memory overruns
                if (turnBuffer.Count > MaxBufferSize)
                {
                    while (turnBuffer.Count > SizeToResume)
                    {
                        // This will break out if things are paused
                        // Otherwise it runs forever
                        if (!IsRunning)
                            return;
                    }
                }
                // Send package to the buffer
                if (package != null)
                {
                    turnBuffer.Post(package);
                }

                if (core == null || !core.Continue)
                {
                    turnBuffer.Complete();
                    //IsRunning = false;
                    break;
                }
            }
        }

        /// <summary>
        /// This how often to update the graphics display, at a minimum
        /// Measured in seconds
        /// </summary>
        virtual public double FlushTime { get; set; } = 1;

        /// <summary>
        /// The scale of the time display relative to real time.
        /// This affects the time delay which is added to slow down the simulation to match real time
        /// If the simulation takes too long to run, this will not help it run any faster
        /// </summary>
        virtual public double TimeScale { get; set; } = 1;

        /// <summary>
        /// Forces the visualizer to draw every frame, which can be useful for debugging but slows things down a lot
        /// </summary>
        virtual public bool SlowDraw { get; set; } = false;

        /// <summary>
        /// Removes any delays for drawing, so the visualization runs as fast as possible
        /// </summary>
        virtual public bool FastDraw { get; set; } = false;

        /// <summary>
        /// Updates visualization
        /// </summary>
        private async Task UpdateVisualAsync()
        {
            double timeOfLastDraw = 0;
            while (await turnBuffer.OutputAvailableAsync() && IsRunning)
            {
                if (Visualizer == null)
                {
                    throw new Exception("Visualizer is null in UpdateVisualAsync");
                }

                var turn = turnBuffer.Receive();
                // Process commands
                turn.Commands.ProcessAll(Visualizer);

                //Update time display
                DisplayTime = turn.Time;
                UpdateTime(DisplayTime);

                // Update graphs
                Graphs.Update(turn.Data);

                if (!FastDraw)
                {
                    //Check if delay is needed
                    var timeDiff = DisplayTime / TimeScale - timer.Elapsed.TotalSeconds;
                    if (timeDiff > 0)
                    {
                        // This delays it so the clocks will line up
                        int delay = (int)(timeDiff * 1000); // Convert to milliseconds
                        await Task.Delay(delay);
                    }
                }

                // Add delay if needed
                double timeSinceLastDraw = timer.Elapsed.TotalSeconds - timeOfLastDraw;
                if (timeSinceLastDraw > FlushTime || SlowDraw)
                {
                    InvalidateVisual();
                    WaitForDrawing();
                    timeOfLastDraw = timer.Elapsed.TotalSeconds;
                }
            }
        }

        /// <summary>
        /// Waits until all drawing is done - needed to keep the visuals displaying sometimes
        /// </summary>
        private void WaitForDrawing()
        {
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
        }

        private Task? engineTask = null;
        private Task? visualizerTask = null;

        /// <summary>
        /// Starts all tasks
        /// </summary>
        public void StartAll()
        {
            visualizerTask = UpdateVisualAsync();
            engineTask = Task.Run(() => RunEngine());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsRunning = false;
        }

    }
}
