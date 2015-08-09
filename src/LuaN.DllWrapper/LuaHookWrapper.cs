using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.DllWrapper
{

    /// <summary>
    /// Wrapper of LuaHook
    /// </summary>
    public class LuaHookWrapper
    {

        /// <summary>
        /// Wrapper for a .Net hook
        /// </summary>
        public LuaHookWrapper(LuaHook hook)
        {
            if (hook == null) throw new ArgumentNullException("hook");
            this.Hook = hook;
            this.NativeHook = InvokeHook;
        }

        /// <summary>
        /// Wrapper for a native hook
        /// </summary>
        public LuaHookWrapper(LuaDll.lua_Hook hook)
        {
            if (hook == null) throw new ArgumentNullException("hook");
            this.NativeHook = hook;
            this.Hook = InvokeNativeHook;
        }

        /// <summary>
        /// Native invoker for the hook
        /// </summary>
        void InvokeHook(IntPtr state, IntPtr pAr)
        {
            LuaState ls = LuaState.FindState(state, true);
            if (Hook != null && ls != null)
            {
                ILuaDebug ar = new LuaDebugWrapper(pAr, LuaGetInfoWhat.None);
                Hook(ls, ar);
            }
        }

        /// <summary>
        /// Managed invoker for the C hook
        /// </summary>
        void InvokeNativeHook(ILuaState L, ILuaDebug ar)
        {
            LuaState state = L as LuaState;
            LuaDebugWrapper arw = ar as LuaDebugWrapper;
            if (state != null && arw != null)
                NativeHook(state.NativeState, arw.NativePointer);
        }

        /// <summary>
        /// .Net hook
        /// </summary>
        public LuaHook Hook { get; private set; }

        /// <summary>
        /// Lua native hook
        /// </summary>
        public LuaDll.lua_Hook NativeHook { get; private set; }
    }

}
