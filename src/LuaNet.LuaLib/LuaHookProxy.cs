using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaNet.LuaLib
{

    /// <summary>
    /// Wrap and maintain a relation between lua_Hook and LuaHook
    /// </summary>
    class LuaHookProxy
    {
        static List<LuaHookProxy> _Proxies = new List<LuaHookProxy>();

        /// <summary>
        /// Hide the constructor
        /// </summary>
        LuaHookProxy() { }

        /// <summary>
        /// Find the proxy for a reader
        /// </summary>
        public static LuaHookProxy FindProxy(LuaHook hook)
        {
            if (hook == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.ManagedHook == hook);
        }

        /// <summary>
        /// Find the proxy for a lua hook
        /// </summary>
        public static LuaHookProxy FindProxy(Lua.lua_Hook hook)
        {
            if (hook == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.UnmanagedHook == hook);
        }

        /// <summary>
        /// Find or create a proxy for a hook
        /// </summary>
        public static LuaHookProxy GetProxy(LuaHook hook)
        {
            if (hook == null) return null;
            var result = FindProxy(hook);
            if (result == null)
            {
                result = new LuaHookProxy() {
                    ManagedHook = hook
                };
                result.UnmanagedHook = result.InvokeManagedHook;
                lock (_Proxies)
                _Proxies.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Find or create a proxy for a lua hook
        /// </summary>
        public static LuaHookProxy GetProxy(Lua.lua_Hook hook)
        {
            if (hook == null) return null;
            var result = FindProxy(hook);
            if (result == null)
            {
                result = new LuaHookProxy() {
                    UnmanagedHook = hook
                };
                result.ManagedHook = result.InvokeUnmanagedHook;
                lock (_Proxies)
                    _Proxies.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Hook to invoke the lua hook
        /// </summary>
        void InvokeUnmanagedHook(ILuaState state, ILuaDebug d)
        {
            LuaState ls = state as LuaState;
            //if (UnmanagedHook != null && ls != null)
            //{
            //    Lua.lua_Debug ar = new Lua.lua_Debug()
            //    {
            //        currentline = d.currentline,
            //        evnt = (int)d.evnt,
            //        istailcall = d.istailcall,
            //        isvararg = d.isvararg,
            //        lastlinedefined = d.lastlinedefined,
            //        linedefined = d.linedefined,
            //        name = d.name,
            //        namewhat = d.namewhat,
            //        nparams = d.nparams,
            //        nups = d.nups,
            //        short_src = d.short_src,
            //        source = d.source,
            //        what = d.what
            //    };
            //    //UnmanagedHook(ls.NativeState, ar);
            //    UnmanagedHook(ls.NativeState, IntPtr.Zero);
            //}
        }

        /// <summary>
        /// Lua hook to invoke the hook
        /// </summary>
        void InvokeManagedHook(IntPtr state, IntPtr ptr)
        {
            LuaState ls = LuaState.FindState(state, true);
            if (ManagedHook != null && ls != null)
            {
                ILuaDebug ar = new LuaDebugProxy(ptr, LuaGetInfoWhat.CurrentLine);
                ManagedHook(ls, ar);
            }
        }

        /// <summary>
        /// Managed hook
        /// </summary>
        public LuaHook ManagedHook { get; private set; }

        /// <summary>
        /// Unmanaged hook
        /// </summary>
        public Lua.lua_Hook UnmanagedHook { get; private set; }

    }

}
