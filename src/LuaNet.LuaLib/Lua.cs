using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaNet.LuaLib
{
    /// <summary>
    /// Lua DLL bindings
    /// </summary>
    public static partial class Lua
    {
        /// <summary>
        /// The Lua DLL name
        /// </summary>
        public const String LuaDllName = "lua53.dll";

        /// <summary>
        /// Dynamic load library
        /// </summary>
        static Lua()
        {
            var path = FindLibrary(new String[] { 
                System.IO.Path.GetDirectoryName(typeof(Lua).Assembly.Location),
                Environment.CurrentDirectory
            });
            if (path == null)
                throw new InvalidProgramException(String.Format("Lua library not found."));
            if (LoadLibrary(path) == IntPtr.Zero)
                throw new InvalidProgramException("Fail to load Lua libary.");
        }
        static String FindLibrary(IEnumerable<String> paths)
        {
            foreach (var path in paths)
            {
                String libPath;
                if (IntPtr.Size == 8)
                    libPath = System.IO.Path.Combine(path, "x64");
                else
                    libPath = System.IO.Path.Combine(path, "x86");
                libPath = System.IO.Path.Combine(libPath, LuaDllName);
                if (System.IO.File.Exists(libPath)) return libPath;
            }
            return null;
        }

        //#region Tools

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        //[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        //private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        //[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool FreeLibrary(IntPtr hModule);

        ///// <summary>
        ///// Load an exported data
        ///// </summary>
        //private static String LoadExportedData(String entryPoint)
        //{
        //    IntPtr hDLL = IntPtr.Zero;
        //    try
        //    {
        //        // Load the DLL
        //        hDLL = LoadLibrary(LuaDllName);
        //        // Perform action only if we are able to load it
        //        if (hDLL != IntPtr.Zero)
        //        {
        //            // Obtain the runtime address of the exported data
        //            IntPtr exportedData = GetProcAddress(hDLL, entryPoint);
        //            if (exportedData == IntPtr.Zero)
        //                return null;
        //            // Convert the result to string
        //            return Marshal.PtrToStringAnsi(exportedData);
        //        }
        //        return null;
        //    }
        //    finally
        //    {
        //        if (hDLL != IntPtr.Zero)
        //            FreeLibrary(hDLL);
        //        hDLL = IntPtr.Zero;
        //    }
        //}

        //#endregion

    }
}
