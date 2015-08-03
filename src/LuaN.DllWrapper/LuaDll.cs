using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaN.DllWrapper
{
    /// <summary>
    /// Lua DLL bindings
    /// </summary>
    public static partial class LuaDll
    {
        /// <summary>
        /// The Lua DLL name
        /// </summary>
        public const String LuaDllName = "lua53.dll";

        /// <summary>
        /// Dynamic load library
        /// </summary>
        static LuaDll()
        {
            var path = FindLibrary(new String[] { 
                System.IO.Path.GetDirectoryName(typeof(LuaDll).Assembly.Location),
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

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    }
}
