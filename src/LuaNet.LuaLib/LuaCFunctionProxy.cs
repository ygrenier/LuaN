using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{

    /// <summary>
    /// Wrap and maintain a relation between lua_CFunction and LuaFunction
    /// </summary>
    class LuaCFunctionProxy
    {
        static List<LuaCFunctionProxy> _Proxies = new List<LuaCFunctionProxy>();

        /// <summary>
        /// Hide the constructor
        /// </summary>
        LuaCFunctionProxy() { }

        /// <summary>
        /// Find the proxy for a function
        /// </summary>
        public static LuaCFunctionProxy FindProxy(LuaFunction function)
        {
            if (function == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.ManagedFunction == function);
        }

        /// <summary>
        /// Find the proxy for a C function
        /// </summary>
        public static LuaCFunctionProxy FindProxy(Lua.lua_CFunction function)
        {
            if (function == null) return null;
            lock (_Proxies)
            return _Proxies.FirstOrDefault(p => p.UnmanagedFunction == function);
        }

        /// <summary>
        /// Find or create a proxy for a function
        /// </summary>
        public static LuaCFunctionProxy GetProxy(LuaFunction function)
        {
            if (function == null) return null;
            LuaCFunctionProxy result = null;
            result = FindProxy(function);
            if (result == null)
            {
                result = new LuaCFunctionProxy()
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
        public static LuaCFunctionProxy GetProxy(Lua.lua_CFunction function)
        {
            if (function == null) return null;
            LuaCFunctionProxy result = null;
            result = FindProxy(function);
            if (result == null)
            {
                result = new LuaCFunctionProxy()
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
        int InvokeUnmanagedFunction(ILuaState state)
        {
            LuaState ls = state as LuaState;
            if (UnmanagedFunction != null && ls != null)
            {
                return UnmanagedFunction(ls.NativeState);
            }
            return 0;
        }

        /// <summary>
        /// C Function to invoke the Function
        /// </summary>
        int InvokeManagementFunction(IntPtr state)
        {
            LuaState ls = LuaState.FindState(state, true);
            if (ManagedFunction != null && ls != null)
            {
                return ManagedFunction(ls);
            }
            return 0;
        }

        /// <summary>
        /// Managed function
        /// </summary>
        public LuaFunction ManagedFunction { get; private set; }

        /// <summary>
        /// Unmanaged function
        /// </summary>
        public Lua.lua_CFunction UnmanagedFunction { get; private set; }

    }

}
