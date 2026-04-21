using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Arena;
using DongUtility;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ArenaVisualizer
{
    /// <summary>
    /// The heart of the interface of the WPF window to the C++ DLL underneath.
    /// </summary>
    public partial class ArenaCoreInterface : HwndHost, IArenaDisplay
    {
        /// <summary>
        /// Important constants for window creation.
        /// </summary>
        private const int
                    WsChild = 0x40000000,
                    WsVisible = 0x10000000,
                    LbsNotify = 0x00000001,
                    HostId = 0x00000002,
                    WsBorder = 0x00800000;

        /// <summary>
        /// Height of the host window
        /// </summary>
        public int HostHeight { get; set; }
        /// <summary>
        /// Width of the host window
        /// </summary>
        public int HostWidth { get; set; }
        /// <summary>
        /// A pointer to the host window
        /// </summary>
        private IntPtr hwndHost;

        /// <summary>
        /// The logical height of the arena
        /// </summary>
        public double ArenaHeight { get; set; }
        /// <summary>
        /// The logical width of the arena
        /// </summary>
        public double ArenaWidth { get; set; }

        /// <summary>
        /// The current maximum layer that has been added to the display
        /// </summary>
        private int currentMaxLayer = -1;

        public ArenaCoreInterface()
        { }

        public ArenaCoreInterface(double windowWidth, double windowHeight, double arenaWidth, double arenaHeight)
        {
            SetWindowDimensions(windowWidth, windowHeight, arenaWidth, arenaHeight);
        }

        /// <summary>
        /// Code that runs after the window starts.
        /// Adds all the initial graphics from the arena
        /// </summary>
        public void AfterStartup(ArenaEngine arena)
        {
            var allGI = arena.Registry.GetAllGraphicInfo();
            for (int i = 0; i < allGI.Count; ++i)
            {
                AddToRegistry(arena.Registry, allGI[i], i);
            }

            RedrawDX();
        }

        /// <summary>
        /// Adds a GraphicsInfo object to the registry at a given index
        /// </summary>
        private void AddToRegistry(Registry registry, GraphicInfo gi, int index)
        {
            // Scale the sizes to be between 0 and 1
            var scaledSizes = ConvertTo1Max(gi.XSize, gi.YSize);
            string fullPath = registry.ImageDirectory + gi.Filename;

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File " + gi.Filename + " not found!");
            }

            AddToRegistryDX(fullPath, scaledSizes.X, scaledSizes.Y, index);
            addedGraphicsCodes.Add(index);
        }

        /// <summary>
        /// Keep track of all graphics code that this class has added to the registry
        /// </summary>
        private readonly List<int> addedGraphicsCodes = [];

        /// <summary>
        /// Convert the coordinates to go from 0 to 1
        /// </summary>
        private Point ConvertTo1Max(double xSize, double ySize)
        {
            return new Point(xSize / ArenaWidth, ySize / ArenaHeight);
        }

        /// <summary>
        /// Convert the coordinates to go from 0 to 1
        /// </summary>
        private Point ConvertTo1Max(Vector2D original)
        {
            return ConvertTo1Max(original.X, original.Y);
        }

        /// <summary>
        /// Convert the coordinates to go from 0 to 1
        /// </summary>
        private Point ConvertTo1Max(Geometry.Geometry2D.Point original)
        {
            return ConvertTo1Max(original.X, original.Y);
        }

        /// <summary>
        /// The handle to the listbox that is used to display the arena
        /// </summary>
        public IntPtr HwndListBox { get; private set; }

        /// <summary>
        /// Creates the window for the display on the low-level Windows side.
        /// </summary>
        /// <param name="hwndParent">The handle to the parent window from WPF</param>
        /// <returns>A reference to the new window</returns>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            HwndListBox = IntPtr.Zero;
            hwndHost = IntPtr.Zero;

            string windowName = $"internalWindow";
            RegisterWindow(windowName);

            // Scale for 4k and other displays
            var source = PresentationSource.FromVisual(this);
            var xScale = source.CompositionTarget.TransformToDevice.M11;
            var yScale = source.CompositionTarget.TransformToDevice.M22;

            // Make the internal window
            hwndHost = CreateWindowEx(0, "static", "",
                WsChild | WsVisible,
                0, 0,
                (int)(HostHeight), (int)(HostWidth),
                hwndParent.Handle,
                (IntPtr)HostId,
                IntPtr.Zero,
                0);

            // Make the full window
            HwndListBox = MakeWindow(windowName,
                WsChild | WsVisible | LbsNotify | WsBorder,
                (int)(HostHeight * xScale),
                (int)(HostWidth * yScale),
                hwndHost, xScale, yScale);

            return new HandleRef(this, hwndHost);
        }

        /// <summary>
        /// Override the message loop for the window.  Just returns 0.
        /// </summary>
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        /// <summary>
        /// Destroys the window.  This is called when the window is closed.
        /// </summary>
        public void Destroy()
        {
            DestroyWindowCore(new HandleRef(this, hwndHost));
        }

        /// <summary>
        /// Destroys the window.  This is called when the window is closed.
        /// </summary>
        /// <param name="hwnd"></param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        /// <summary>
        /// The location of the DLL that contains the C++ code.
        /// </summary>
        private const string dllName = @"..\..\..\..\ArenaVisualizer\ArenaCore.dll";


        [DllImport(dllName, EntryPoint = "ResetWindow", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void ResetWindow();

        [DllImport(dllName, EntryPoint = "RegisterWindow", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern bool RegisterWindow(string ClassName);

        [DllImport(dllName, EntryPoint = "CheckRegistration", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern bool CheckRegistration(string ClassName);

        [DllImport(dllName, EntryPoint = "MakeWindow", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern IntPtr MakeWindow(string ClassName, int style, int height, int width, IntPtr parent,
            double widthFactor, double heightFactor);


        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x, int y,
            int width, int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInst,
            [MarshalAs(UnmanagedType.IUnknown)] object pvParam);

        [LibraryImport("user32.dll", EntryPoint = "DestroyWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(IntPtr hwnd);

        [LibraryImport(dllName, EntryPoint = "AddToRegistry", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddToRegistryDX([MarshalAs(UnmanagedType.LPWStr)] string filename, double width,
            double height, int index);

        [LibraryImport(dllName, EntryPoint = "AddVisualLayer")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddVisualLayerDX();

        [LibraryImport(dllName, EntryPoint = "AddObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddObjectDX(int layer, int graphicIndex, int index,
            double x, double y);

        [LibraryImport(dllName, EntryPoint = "MoveObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void MoveObjectDX(int layer, int index,
            double x, double y);

        [LibraryImport(dllName, EntryPoint = "RotateObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void RotateObjectDX(int layer, int index, double rotation);

        [LibraryImport(dllName, EntryPoint = "RemoveObject")]
        [UnmanagedCallConv(CallConvs =[typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void RemoveObjectDX(int layer, int index);

        [LibraryImport(dllName, EntryPoint = "ChangeGraphic")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ChangeGraphicObjectDX(int layer, int index,
            int newGraphicInstance);

        [LibraryImport(dllName, EntryPoint = "ResizeDisplay")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ResizeDisplayDX(int newX, int newY);

        [LibraryImport(dllName, EntryPoint = "Redraw")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void RedrawDX();

        [LibraryImport(dllName, EntryPoint = "Zoom")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ZoomDX(double xScale, double yScale, double xCenter, double yCenter);

        public void AddObject(Registry registry, int layer, int graphicCode, int objCode, Geometry.Geometry2D.Point coord)
        {
            while (currentMaxLayer < layer)
            {
                AddVisualLayerDX();
                ++currentMaxLayer;
            }

            var newCoord = ConvertTo1Max(coord);

            if (!addedGraphicsCodes.Contains(graphicCode))
            {
                AddToRegistry(registry, registry.GetInfo(graphicCode), graphicCode);
            }

            AddObjectDX(layer, graphicCode, objCode, newCoord.X, newCoord.Y);
        }

        public void MoveObject(int layer, int objCode, Geometry.Geometry2D.Point newCoord)
        {
            var newCoordX = ConvertTo1Max(newCoord);
            MoveObjectDX(layer, objCode, newCoordX.X, newCoordX.Y);
        }

        public void RotateObject(int layer, int objCode, double rotation)
        {
            RotateObjectDX(layer, objCode, rotation);
        }

        public void RemoveObject(int layer, int objCode)
        {
            RemoveObjectDX(layer, objCode);
        }

        public void ChangeObjectGraphic(int layer, int objCode, int graphicCode)
        {
            ChangeGraphicObjectDX(layer, objCode, graphicCode);
        }

        public void ScaleDisplay(int newWidth, int newHeight)
        {
            ResizeDisplayDX(newWidth, newHeight);
        }

        public void Redraw()
        {
            RedrawDX();
        }

        public void Zoom(double xScale, double yScale, double xCenter, double yCenter)
        {
            ZoomDX(xScale, yScale, xCenter, yCenter);
        }

        public void SetWindowDimensions(double windowWidth, double windowHeight, double arenaWidth, double arenaHeight)
        {
            HostHeight = (int)windowHeight;
            HostWidth = (int)windowWidth;
            ArenaWidth = arenaWidth;
            ArenaHeight = arenaHeight;
        }
    }
}
