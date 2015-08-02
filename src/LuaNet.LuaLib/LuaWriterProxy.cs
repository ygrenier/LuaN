using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaNet.LuaLib
{

    /// <summary>
    /// Wrap and maintain a relation between lua_Writer and LuaWriter
    /// </summary>
    class LuaWriterProxy
    {
        static List<LuaWriterProxy> _Proxies = new List<LuaWriterProxy>();

        /// <summary>
        /// Hide the constructor
        /// </summary>
        LuaWriterProxy() { }

        /// <summary>
        /// Find the proxy for a Writer
        /// </summary>
        public static LuaWriterProxy FindProxy(LuaWriter writer)
        {
            if (writer == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.ManagedWriter == writer);
        }

        ///// <summary>
        ///// Find the proxy for a lua writer
        ///// </summary>
        //public static LuaWriterProxy FindProxy(Lua.lua_Writer writer)
        //{
        //    if (writer == null) return null;
        //    lock (_Proxies)
        //    return _Proxies.FirstOrDefault(p => p.UnmanagedWriter == writer);
        //}

        /// <summary>
        /// Find or create a proxy for a writer
        /// </summary>
        public static LuaWriterProxy GetProxy(LuaWriter writer)
        {
            var result = FindProxy(writer);
            if (result == null && writer != null)
            {
                result = new LuaWriterProxy()
                {
                    ManagedWriter = writer
                };
                result.UnmanagedWriter = result.InvokeManagedWriter;
                lock (_Proxies)
                    _Proxies.Add(result);
            }
            return result;
        }

        ///// <summary>
        ///// Find or create a proxy for a lua writer
        ///// </summary>
        //public static LuaWriterProxy GetProxy(Lua.lua_Writer writer)
        //{
        //    if (writer == null) return null;
        //    var result = FindProxy(writer);
        //    if (result == null)
        //    {
        //        result = new LuaWriterProxy() {
        //            UnmanagedWriter = writer
        //        };
        //        result.ManagedWriter = result.InvokeUnmanagedWriter;
        //        lock (_Proxies)
        //            _Proxies.Add(result);
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Reader to invoke the lua Writer
        ///// </summary>
        //int InvokeUnmanagedWriter(ILuaState state, Byte[] p, Object ud)
        //{
        //    LuaState ls = state as LuaState;
        //    if (UnmanagedWriter != null && ls != null)
        //    {
        //        int res = 0;
        //        if (p != null)
        //        {
        //            var ptr = Marshal.AllocHGlobal(p.Length);
        //            Marshal.Copy(p, 0, ptr, p.Length);
        //            res = UnmanagedWriter(ls.NativeState, ptr, (UInt32)p.Length, UserDataRef.GetRef(ud));
        //            Marshal.FreeHGlobal(ptr);
        //        }
        //        else
        //        {
        //            res = UnmanagedWriter(ls.NativeState, IntPtr.Zero, 0, UserDataRef.GetRef(ud));
        //        }
        //        return res;
        //    }
        //    return 0;
        //}

        /// <summary>
        /// Lua writer to invoke the Writer
        /// </summary>
        int InvokeManagedWriter(IntPtr state, IntPtr ptr, UInt32 sz, IntPtr ud)
        {
            LuaState ls = LuaState.FindState(state, true);
            if (ManagedWriter != null && ls != null)
            {
                Byte[] buffer = ptr != IntPtr.Zero ? new Byte[sz] : null;
                if (ptr != IntPtr.Zero)
                {
                    Marshal.Copy(ptr, buffer, 0, (int)sz);
                }
                var res = ManagedWriter(ls, buffer, UserDataRef.GetData(ud));
                buffer = null;
                return res;
            }
            return 0;
        }

        /// <summary>
        /// Managed Writer
        /// </summary>
        public LuaWriter ManagedWriter { get; private set; }

        /// <summary>
        /// Unmanaged Writer
        /// </summary>
        public Lua.lua_Writer UnmanagedWriter { get; private set; }

    }

}
