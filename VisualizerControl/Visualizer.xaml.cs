using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using WPFUtility;

namespace VisualizerControl
{
    /// <summary>
    /// Interaction logic for ArenaVisualizerStandalone.xaml
    /// </summary>
    public partial class Visualizer : UserControl
    {
        public Visualizer()
        {
            InitializeComponent();
        }

        private void Redraw(object sender, ElapsedEventArgs e)
        {
            //if (Display != null && ShowVisual)
            //Display.Redraw();
        }

        private Application? app;
        private IntPtr hwndListBox;
        private Window? myWindow;
        internal Visualizer3DCoreInterface? CoreInterface { get; set; }

        public bool UIinitialized = false;

        private void OnUIReady(object sender, EventArgs e)
        {
            if (!UIinitialized)
            {

                app = Application.Current;
                myWindow = app.MainWindow;
                //myWindow.SizeToContent = SizeToContent.WidthAndHeight;
                CoreInterface = new Visualizer3DCoreInterface(Visualizer3DCoreInterfaceHolder.ActualWidth,
                    Visualizer3DCoreInterfaceHolder.ActualHeight);
                Visualizer3DCoreInterfaceHolder.Child = CoreInterface;
                hwndListBox = CoreInterface.HwndListBox;

                UIinitialized = true;
            }
        }

        public void WhenLoaded(object? sender, EventArgs e)
        {
            var window = Window.GetWindow(this);
            if (!initialized)
            {
                // Call it as a task, or else everything hangs here
                Task.Run(() => Visualizer3DCoreInterface.SetupDirectX());

                initialized = true;
            }
            //InvalidateVisual();
        }
        private bool initialized = false;

        /// <summary>
        /// Adds a particle with a user-defined index for later manipulation
        /// </summary>
        public void AddParticle(Object3D part, int index)
        {
            CoreInterface?.AddObject(part, index);
        }

        public DongUtility.Vector GetParticlePosition(int index)
        {
            if (CoreInterface == null)
            {
                throw new InvalidOperationException("CoreInterface is not initialized.");
            }
            return CoreInterface.GetObjectPosition(index);
        }


        public void ChangeMaterialColor(BasicMaterial material)
        {
            CoreInterface?.ChangeMaterialColor(material);
        }

        /// <summary>
        /// Removes a particle with a given index
        /// </summary>
        public void RemoveParticle(int index)
        {
            CoreInterface?.RemoveObject(index);
        }

        public void MoveParticle(int index, Vector3D newPosition)
        {
            CoreInterface?.MoveObject(index, newPosition);
        }

        public void TransformParticle(int index, Vector3D newPosition,
            Vector3D newScale, Matrix3D newRotation)
        {
            CoreInterface?.TransformObject(index, newScale, newRotation, newPosition);
        }

        /// <summary>
        /// Clears all objects from the visualizer
        /// </summary>
        public void Clear()
        {
            CoreInterface?.Clear();
        }

        /// <summary>
        /// Moves and rotates the camera
        /// </summary>
        public void MoveCamera(Point3D newPosition, Vector3D newDirection, Vector3D upDirection)
        {
            CoreInterface?.MoveCamera(newPosition, newDirection, upDirection);
        }

        /// <summary>
        /// Moves camera without rotation
        /// </summary>
        /// <param name="newPosition"></param>
        public void MoveCamera(Point3D newPosition)
        {
            CoreInterface?.MoveCamera(newPosition);
        }

        /// <summary>
        /// Turns the camera to look at a given point
        /// </summary>
        public void LookAt(Point3D newPosition, Point3D target, Vector3D upDirection)
        {
            CoreInterface?.LookAt(newPosition, target, upDirection);
        }

        public void AdjustLens(double pointOfView, double aspectRatio, double nearZ, double farZ)
        {
            Visualizer3DCoreInterface.AdjustLens(pointOfView, aspectRatio, nearZ, farZ);
        }

        public void AutoCameraAdjust()
        {
            CoreInterface?.AutoCameraAdjust();
        }


        /// <summary>
        /// Activates auto-camera mode
        /// </summary>
        public bool AutoCamera
        {
            set
            {
                CoreInterface?.SetAutoCamera(value);
            }
        }

        public Vector3D CameraPosition => UtilityFunctions.ConvertToVector3D(Visualizer3DCoreInterface.CameraPosition);
        public Vector3D CameraLookDirection => UtilityFunctions.ConvertToVector3D(Visualizer3DCoreInterface.CameraLookDirection);
        public Vector3D CameraUpDirection => UtilityFunctions.ConvertToVector3D(Visualizer3DCoreInterface.CameraUpDirection);

        public bool Paused
        {
            get
            {
                if (CoreInterface == null)
                {
                    throw new InvalidOperationException("CoreInterface is not initialized.");
                }
                return CoreInterface.Paused;
            }
            set
            {
                if (CoreInterface == null)
                {
                    throw new InvalidOperationException("CoreInterface is not initialized.");
                }
                CoreInterface.Paused = value;
            }
        }
    }
}
