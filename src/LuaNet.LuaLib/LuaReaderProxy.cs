using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaNet.LuaLib
{

    /// <summary>
    /// Wrap and maintain a relation between lua_Reader and LuaReader
    /// </summary>
    class LuaReaderProxy
    {
        static List<LuaReaderProxy> _Proxies = new List<LuaReaderProxy>();

        /// <summary>
        /// Hide the constructor
        /// </summary>
        LuaReaderProxy() { }

        /// <summary>
        /// Find the proxy for a reader
        /// </summary>
        public static LuaReaderProxy FindProxy(LuaReader reader)
        {
            if (reader == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.ManagedReader == reader);
        }

        /// <summary>
        /// Find the proxy for a lua reader
        /// </summary>
        public static LuaReaderProxy FindProxy(Lua.lua_Reader reader)
        {
            if (reader == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.UnmanagedReader == reader);
        }

        /// <summary>
        /// Find or create a proxy for a reader
        /// </summary>
        public static LuaReaderProxy GetProxy(LuaReader reader)
        {
            if (reader == null) return null;
            var result = FindProxy(reader);
            if (result == null)
            {
                result = new LuaReaderProxy() {
                    ManagedReader = reader
                };
                result.UnmanagedReader = result.InvokeManagementReader;
                lock (_Proxies)
                _Proxies.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Find or create a proxy for a lua reader
        /// </summary>
        public static LuaReaderProxy GetProxy(Lua.lua_Reader reader)
        {
            if (reader == null) return null;
            var result = FindProxy(reader);
            if (result == null)
            {
                result = new LuaReaderProxy() {
                    UnmanagedReader = reader
                };
                result.ManagedReader = result.InvokeUnmanagedReader;
                lock (_Proxies)
                    _Proxies.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Readder to invoke the lua reader
        /// </summary>
        Byte[] InvokeUnmanagedReader(ILuaState state, Object ud)
        {
            LuaState ls = state as LuaState;
            if (UnmanagedReader != null && ls != null)
            {
                UInt32 sz = 0;
                var res = UnmanagedReader(ls.NativeState, UserDataRef.GetRef(ud), ref sz);
                if (sz <= 0 || res == IntPtr.Zero) return null;
                Byte[] buffer = new Byte[sz];
                Marshal.Copy(res, buffer, 0, (int)sz);
                return buffer;
            }
            return null;
        }

        IntPtr _LastReadBuffer = IntPtr.Zero;
        UInt64 _LastReadBufferSize = 0;

        /// <summary>
        /// Lua reader to invoke the reader
        /// </summary>
        IntPtr InvokeManagementReader(IntPtr state, IntPtr ud, ref UInt32 sz)
        {
            if (_LastReadBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_LastReadBuffer);
                _LastReadBuffer = IntPtr.Zero;
                _LastReadBufferSize = 0;
            }
            LuaState ls = LuaState.FindState(state, true);
            if (ManagedReader != null && ls != null)
            {
                var res = ManagedReader(ls, UserDataRef.GetData(ud));
                if (res != null && res.Length > 0)
                {
                    sz = (UInt32)res.Length;
                    _LastReadBuffer = Marshal.AllocHGlobal((int)sz);
                    _LastReadBufferSize = sz;
                    Marshal.Copy(res, 0, _LastReadBuffer, (int)sz);
                    return _LastReadBuffer;
                }
            }
            sz = 0;
            return IntPtr.Zero;
        }

        /// <summary>
        /// Managed reader
        /// </summary>
        public LuaReader ManagedReader { get; private set; }

        /// <summary>
        /// Unmanaged reader
        /// </summary>
        public Lua.lua_Reader UnmanagedReader { get; private set; }

    }

}
