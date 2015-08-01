using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{

    /// <summary>
    /// Wrap and maintain a relation between lua_KFunction and LuaKFunction
    /// </summary>
    class LuaKFunctionProxy
    {
        static List<LuaKFunctionProxy> _Proxies = new List<LuaKFunctionProxy>();

        /// <summary>
        /// Hide the constructor
        /// </summary>
        LuaKFunctionProxy() { }

        /// <summary>
        /// Find the proxy for a function
        /// </summary>
        public static LuaKFunctionProxy FindProxy(LuaKFunction function)
        {
            if (function == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.ManagedFunction == function);
        }

        /// <summary>
        /// Find the proxy for a C function
        /// </summary>
        public static LuaKFunctionProxy FindProxy(Lua.lua_KFunction function)
        {
            if (function == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.UnmanagedFunction == function);
        }

        /// <summary>
        /// Find or create a proxy for a function
        /// </summary>
        public static LuaKFunctionProxy GetProxy(LuaKFunction function)
        {
            if (function == null) return null;
            LuaKFunctionProxy result = null;
            result = FindProxy(function);
            if (result == null)
            {
                result = new LuaKFunctionProxy()
                {
                    ManagedFunction = function
                };
                result.UnmanagedFunction = result.InvokeManagementFunction;
                lock (_Proxies)
                    _Proxies.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Find or create a proxy for a C function
        /// </summary>
        public static LuaKFunctionProxy GetProxy(Lua.lua_KFunction function)
        {
            if (function == null) return null;
            LuaKFunctionProxy result = null;
            result = FindProxy(function);
            if (result == null)
            {
                result = new LuaKFunctionProxy()
                {
                    UnmanagedFunction = function
                };
                result.ManagedFunction = result.InvokeUnmanagedFunction;
                lock (_Proxies)
                    _Proxies.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Function to invoke the CFunction
        /// </summary>
        int InvokeUnmanagedFunction(ILuaState state, int status, long ctx)
        {
            LuaState ls = state as LuaState;
            if (UnmanagedFunction != null && ls != null)
            {
                return UnmanagedFunction(ls.NativeState, status, ctx);
            }
            return 0;
        }

        /// <summary>
        /// C Function to invoke the Function
        /// </summary>
        int InvokeManagementFunction(IntPtr state, int status, long ctx)
        {
            LuaState ls = LuaState.FindState(state, true);
            if (ManagedFunction != null && ls != null)
            {
                return ManagedFunction(ls, status, ctx);
            }
            return 0;
        }

        /// <summary>
        /// Managed function
        /// </summary>
        public LuaKFunction ManagedFunction { get; private set; }

        /// <summary>
        /// Unmanaged function
        /// </summary>
        public Lua.lua_KFunction UnmanagedFunction { get; private set; }

    }

}
