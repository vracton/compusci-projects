using System;
using System.Runtime.InteropServices;

namespace WPFUtility
{
    /// <summary>
    /// Copied wholesale from StackOverflow at https://stackoverflow.com/questions/31978826/is-it-possible-to-have-a-wpf-application-print-console-output/31978833
    /// </summary>
    public static partial class ConsoleManager
    {
        [LibraryImport(@"kernel32.dll", EntryPoint = "AllocConsole")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool AllocConsole();

        [LibraryImport(@"kernel32.dll", EntryPoint = "GetConsoleWindow")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.SysInt)]
        internal static partial IntPtr GetConsoleWindow();

        [LibraryImport(@"user32.dll", EntryPoint = "ShowWindow")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SwHide = 0;
        const int SwShow = 5;


        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SwShow);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SwHide);
        }
    }
}
