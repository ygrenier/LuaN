using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaN.DllWrapper
{
    /// <summary>
    /// Wrapper of LuaWriter
    /// </summary>
    public class LuaWriterWrapper
    {

        /// <summary>
        /// Wrapper for a .Net writer
        /// </summary>
        public LuaWriterWrapper(LuaWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this.Writer = writer;
            this.NativeWriter = InvokeWriter;
        }

        /// <summary>
        /// Native invoker for the writer
        /// </summary>
        int InvokeWriter(IntPtr L, IntPtr ptr, UInt32 sz, IntPtr ud)
        {
            LuaState state = LuaState.FindState(L, true);
            if (state != null)
            {
                Byte[] buffer = ptr != IntPtr.Zero ? new Byte[sz] : null;
                if (ptr != IntPtr.Zero)
                {
                    Marshal.Copy(ptr, buffer, 0, (int)sz);
                }
                var res = Writer(state, buffer, state.GetUserData(ud));
                buffer = null;
                return res;
            }
            return 0;
        }

        /// <summary>
        /// .Net writer
        /// </summary>
        public LuaWriter Writer { get; private set; }

        /// <summary>
        /// Lua native writer
        /// </summary>
        public LuaDll.lua_Writer NativeWriter { get; private set; }

    }
}
