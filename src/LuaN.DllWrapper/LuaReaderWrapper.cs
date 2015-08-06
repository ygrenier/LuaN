using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaN.DllWrapper
{
    /// <summary>
    /// Wrapper of LuaReader
    /// </summary>
    public class LuaReaderWrapper : IDisposable
    {
        IntPtr _LastReadBuffer = IntPtr.Zero;
        UInt64 _LastReadBufferSize = 0;

        /// <summary>
        /// Wrapper for a .Net reader
        /// </summary>
        public LuaReaderWrapper(LuaReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            this.Reader = reader;
            this.NativeReader = InvokeReader;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~LuaReaderWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// Internal release resource
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            ReleaseReadBuffer();
        }

        /// <summary>
        /// Release resource
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void ReleaseReadBuffer()
        {
            if (_LastReadBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_LastReadBuffer);
                _LastReadBuffer = IntPtr.Zero;
                _LastReadBufferSize = 0;
            }
        }

        /// <summary>
        /// Native invoker for the reader
        /// </summary>
        IntPtr InvokeReader(IntPtr L, IntPtr ud, ref UInt32 sz)
        {
            // If we have a buffer allocated, we freeing it
            ReleaseReadBuffer();
            LuaState state = LuaState.FindState(L, true);
            if (state != null)
            {
                var res = Reader(state, state.GetUserData(ud));
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
        /// .Net reader
        /// </summary>
        public LuaReader Reader { get; private set; }

        /// <summary>
        /// Lua native reader
        /// </summary>
        public LuaDll.lua_Reader NativeReader { get; private set; }

    }
}
