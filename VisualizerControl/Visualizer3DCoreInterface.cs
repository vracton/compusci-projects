using DongUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using Vector = DongUtility.Vector;

namespace VisualizerControl
{
    /// <summary>
    /// The crucial interface between the WPF and the C++ code
    /// </summary>
    public partial class Visualizer3DCoreInterface : HwndHost
    {
        internal const int
                    WsChild = 0x40000000,
                    WsVisible = 0x10000000,
                    LbsNotify = 0x00000001,
                    HostId = 0x00000002,
                    ListboxId = 0x00000001,
                    WsVscroll = 0x00200000,
                    WsBorder = 0x00800000;

        public int HostHeight { get; set; }
        public int HostWidth { get; set; }
        private IntPtr hwndHost;

        public Visualizer3DCoreInterface()
        { }

        public Visualizer3DCoreInterface(double windowWidth, double windowHeight)
        {
            SetWindowDimensions(windowWidth, windowHeight);
        }

        public IntPtr HwndListBox { get; private set; }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            HwndListBox = IntPtr.Zero;
            hwndHost = IntPtr.Zero;

            string windowName = "internalWindow";
            //string curDir = Directory.GetCurrentDirectory();
            RegisterWindow(windowName);

            var source = PresentationSource.FromVisual(this);
            double dpiX = 1;
            double dpiY = 1;
            if (source?.CompositionTarget != null)
            {
                var m = source.CompositionTarget.TransformToDevice;
                dpiX = m.M11;
                dpiY = m.M22;
            }

            hwndHost = CreateWindowEx(0, "static", "",
                WsChild | WsVisible,
                0, 0,
                HostHeight, HostWidth,
                hwndParent.Handle,
                HostId,
                IntPtr.Zero,
                0);

            HwndListBox = MakeWindow(windowName,
                WsChild | WsVisible | LbsNotify | WsBorder,
                (int)(HostHeight * dpiY),//* fourKScaleFactor * debugInternalScale),
                (int)(HostWidth * dpiX),//* fourKScaleFactor * debugInternalScale),
                hwndHost);

            return new HandleRef(this, hwndHost);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        private readonly Dictionary<string, int> shapeCodes = [];
        private readonly Dictionary<string, int> materialCodes = [];
        private int counter = 0;

        /// <summary>
        /// Adds an object to the visualizer, with an index specified by the caller
        /// </summary>
        internal void AddObject(Object3D obj, int externalIndex)
        {
            // Only add the shape if it is not currently in the dictionary
            var shape = obj.Shape;
            if (!shapeCodes.ContainsKey(shape.ShapeName))
            {
                AddShape(shape);
                // So each shape is added only once
            }

            var material = obj.Material;

            if (!materialCodes.ContainsKey(material.Name))
            {
                AddMaterial(material);
            }

            float[] position = ConvertVector(obj.Position);
            float[] scale = ConvertVector(obj.Scale);
            float[] rotation = ConvertMatrix(obj.Rotation.Value);
            AddObjectX(externalIndex, scale, rotation, position,
                shapeCodes[shape.ShapeName], materialCodes[material.Name]);
        }

        internal Vector GetObjectPosition(int index)
        {
            float[] response = new float[3];
            GetObjectPositionX(index, response);
            return new Vector(response[0], response[1], response[2]);
        }

        private static float[] ConvertVector(Vector3D vec)
        {
            float[] response = [(float)vec.X, (float)vec.Y, (float)vec.Z];
            return response;
        }

        private static float[] ConvertMatrix(Matrix3D mat)
        {
            float[] response =
            [
                (float)mat.M11,
                (float)mat.M21,
                (float)mat.M31,
                (float)mat.M12,
                (float)mat.M22,
                (float)mat.M32,
                (float)mat.M13,
                (float)mat.M23,
                (float)mat.M33,
            ];
            return response;
        }

        private int AddMaterial(BasicMaterial material)
        {
            var color = material.Color;
            float r = (float)color.R / 255;
            float g = (float)color.G / 255;
            float b = (float)color.B / 255;
            float a = (float)color.A / 255;
            int index = materialCodes.Count;
            materialCodes.Add(material.Name, index);
            AddMaterialX(index, r, g, b, a, (float)material.Fresnel, (float)material.Roughness);

            return index;
        }

        /// <summary>
        /// Changes the color of a material based on the change already taken place
        /// in the BasicMaterial object
        /// </summary>
        public void ChangeMaterialColor(BasicMaterial material)
        {
            var color = material.Color;
            float r = (float)color.R / 255;
            float g = (float)color.G / 255;
            float b = (float)color.B / 255;
            float a = (float)color.A / 255;

            int internalIndex = materialCodes[material.Name];
            ChangeMaterialColorX(internalIndex, r, g, b, a);
        }

        /// <summary>
        /// Adds a shape prototype that can be used for multiple objects
        /// Is passed directly into the C++ code for use in the graphics card
        /// </summary>
        /// <param name="shape"></param>
        private void AddShape(Shapes.Shape3D shape)
        {
            var mesh = shape.Mesh;
            int nVertices = mesh.Positions.Count;
            // Vectors are packed into flat arrays
            // So there are 3 * nVertices points in vertices[] and normals[]
            int size = nVertices * 3;
            float[] vertices = new float[size];
            float[] normals = new float[size];
            for (int i = 0; i < nVertices; ++i)
            {
                var vertex = mesh.Positions[i];
                int index = i * 3;
                vertices[index] = (float)(vertex.X);
                vertices[index + 1] = (float)(vertex.Y);
                vertices[index + 2] = (float)(vertex.Z);
                var normal = mesh.Normals[i];
                normals[index] = (float)(normal.X);
                normals[index + 1] = (float)(normal.Y);
                normals[index + 2] = (float)(normal.Z);
            }

            int nTriangleIndices = mesh.TriangleIndices.Count;
            UInt32[] triangles = new UInt32[nTriangleIndices];
            for (int i = 0; i < nTriangleIndices; ++i)
            {
                triangles[i] = (UInt32)(mesh.TriangleIndices[i]);
            }

            shapeCodes.Add(shape.ShapeName, shapeCodes.Count);
            AddShapeX(shapeCodes[shape.ShapeName], nVertices, vertices, normals, nTriangleIndices,
                triangles);
        }

        /// <summary>
        /// Move an object of a given index to a new position
        /// </summary>
        internal void MoveObject(int index, Vector3D newPosition)
        {
            MoveObjectX(index, ConvertVector(newPosition));
        }

        /// <summary>
        /// Transform an object of a given index to a new position, scale, and rotation
        /// </summary>
        internal void TransformObject(int index, Vector3D newScale,
            Matrix3D newRotation, Vector3D newPosition)
        {
            float[] position = ConvertVector(newPosition);
            float[] scale = ConvertVector(newScale);
            float[] rotation = ConvertMatrix(newRotation);
            TransformObjectX(index, scale, rotation, position);
        }

        /// <summary>
        /// Remove an object of a given index from the visualizer
        /// </summary>
        internal void RemoveObject(int index)
        {
            RemoveObjectX(index);
        }

        /// <summary>
        /// Clear all internal data structures and the visualizer
        /// </summary>
        internal void Clear()
        {
            ClearX();
            counter = 0;
            shapeCodes.Clear();
            materialCodes.Clear();
        }

        /// <summary>
        /// Turn on auto camera mode, where the camera automatically adjusts to show all objects
        /// </summary>
        /// <param name="value"></param>
        internal void SetAutoCamera(bool value) => SetAutoCameraX(value);

        /// <summary>
        /// Run the auto camera adjustment algorithm once
        /// </summary>
        internal void AutoCameraAdjust() => AutoCameraAdjustX();

        /// <summary>
        /// Move the camera to a new position
        /// </summary>
        internal void MoveCamera(Point3D newPosition)
        {
            float[] position = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(newPosition));
            MoveCameraX(position);
        }

        /// <summary>
        /// Move the camera to a new position, looking in a new direction with a new up direction
        /// </summary>
        internal void MoveCamera(Point3D newPosition, Vector3D newLookDirection, Vector3D newUpDirection)
        {
            float[] position = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(newPosition));
            float[] lookDirection = ConvertVector(newLookDirection);
            float[] upDirection = ConvertVector(newUpDirection);
            MoveAndTurnCameraX(position, lookDirection, upDirection);
        }

        /// <summary>
        /// Move the camera to look at a specific target from a new position, with a new up direction
        /// </summary>
        internal void LookAt(Point3D newPosition, Point3D target, Vector3D newUpDirection)
        {
            float[] position = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(newPosition));
            float[] newTarget = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(target));
            float[] upDirection = ConvertVector(newUpDirection);
            LookAtX(position, newTarget, upDirection);
        }

        /// <summary>
        /// Adjust the camera zoom parameters
        /// </summary>
        /// <param name="fieldOfViewY">The field of view in the y direction(top to bottom of the screen)</param>
        /// <param name="aspectRatio">The aspect ratio between x and y directions</param>
        /// <param name="nearZ">The closest objects can be to the camera along its axis to still be displayed</param>
        /// <param name="farZ">The farthest objects can be to the camera along its axis to still be displayed</param>
        internal static void AdjustLens(double fieldOfViewY, double aspectRatio, double nearZ, double farZ)
        {
            AdjustLensX((float)fieldOfViewY, (float)aspectRatio, (float)nearZ, (float)farZ);
        }

        internal static Vector CameraPosition => GetCameraInfo().Item1;
        internal static Vector CameraLookDirection => GetCameraInfo().Item2;
        internal static Vector CameraUpDirection => GetCameraInfo().Item3;

        private static Tuple<Vector, Vector, Vector> GetCameraInfo()
        {
            var position = new float[3];
            var lookDirection = new float[3];
            var upDirection = new float[3];
            GetCameraPositionX(position, lookDirection, upDirection);
            var positionVec = new Vector(position[0], position[1], position[2]);
            var lookVec = new Vector(lookDirection[0], lookDirection[1], lookDirection[2]);
            var upVec = new Vector(upDirection[0], upDirection[1], upDirection[2]);
            return new Tuple<Vector, Vector, Vector>(positionVec, lookVec, upVec);
        }

        private bool paused = true;
        /// <summary>
        /// Whether the visualizer is currently paused
        /// </summary>
        internal bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
                SetPauseDrawingStateX(value);
            }
        }

        private const string dllName = @"..\..\..\..\VisualizerControl\Visualizer3DCore.dll";

        [LibraryImport(dllName, EntryPoint = "RegisterWindow", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RegisterWindow(string ClassName);

        [LibraryImport(dllName, EntryPoint = "MakeWindow", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial IntPtr MakeWindow(string ClassName, int style, int height, int width, IntPtr parent);

        [LibraryImport(dllName, EntryPoint = "SetupDirectX")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void SetupDirectX();

        [LibraryImport(dllName, EntryPoint = "AddShape")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddShapeX(int index, int nVertices, [In] float[] vertices,
        [In] float[] normals, int nTriangleIndices, [In] UInt32[] triangles);

        [LibraryImport(dllName, EntryPoint = "AddMaterial")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddMaterialX(int index, float colorR,
            float colorG, float colorB, float alpha, float fresnel, float roughness);

        [LibraryImport(dllName, EntryPoint = "ChangeMaterialColor")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ChangeMaterialColorX(int index, float colorR,
            float colorG, float colorB, float alpha);

        [LibraryImport(dllName, EntryPoint = "AddObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial int AddObjectX(int index, [In] float[] scale, [In] float[] rotation,
        [In] float[] position, int shape, int material);

        [LibraryImport(dllName, EntryPoint = "GetObjectPosition")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void GetObjectPositionX(int index, [Out] float[] position);

        [LibraryImport(dllName, EntryPoint = "MoveObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void MoveObjectX(int index, [In] float[] newPosition);

        [LibraryImport(dllName, EntryPoint = "TransformObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void TransformObjectX(int index, [In] float[] scale,
        [In] float[] rotation, [In] float[] position);

        [LibraryImport(dllName, EntryPoint = "RemoveObject")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void RemoveObjectX(int index);

        [LibraryImport(dllName, EntryPoint = "Clear")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ClearX();

        [LibraryImport(dllName, EntryPoint = "SetAutoCamera")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void SetAutoCameraX([MarshalAs(UnmanagedType.Bool)] bool value);

        [LibraryImport(dllName, EntryPoint = "AutoCameraAdjust")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AutoCameraAdjustX();

        [LibraryImport(dllName, EntryPoint = "MoveCamera")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void MoveCameraX([In] float[] newPosition);

        [LibraryImport(dllName, EntryPoint = "MoveAndTurnCamera")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void MoveAndTurnCameraX([In] float[] newPosition, [In] float[] lookDirection, [In] float[] upDirection);

        [LibraryImport(dllName, EntryPoint = "LookAt")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void LookAtX([In] float[] newPosition, [In] float[] target, [In] float[] upDirection);

        [LibraryImport(dllName, EntryPoint = "AdjustLens")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AdjustLensX(float fieldOfViewY, float aspectRatio, float nearZ, float farZ);

        [LibraryImport(dllName, EntryPoint = "SetPauseDrawingState")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void SetPauseDrawingStateX([MarshalAs(UnmanagedType.Bool)] bool value);

        [LibraryImport(dllName, EntryPoint = "GetCameraPosition")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void GetCameraPositionX([Out] float[] position, [Out] float[] lookDirection, [Out] float[] upDirection);

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(int dwExStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x, int y,
            int width, int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInst,
            IntPtr pvParam);

        [LibraryImport("user32.dll", EntryPoint = "DestroyWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(IntPtr hwnd);

        public void SetWindowDimensions(double windowWidth, double windowHeight)
        {
            HostHeight = (int)windowHeight;
            HostWidth = (int)windowWidth;
        }
    }
}
