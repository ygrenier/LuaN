using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.DllWrapper
{

    /// <summary>
    /// Wrapper of LuaKFunction
    /// </summary>
    public class LuaKFunctionWrapper
    {

        /// <summary>
        /// Wrapper for a .Net K function
        /// </summary>
        public LuaKFunctionWrapper(LuaKFunction function)
        {
            if (function == null) throw new ArgumentNullException("function");
            this.KFunction = function;
            this.NativeFunction = InvokeKFunction;
        }

        /// <summary>
        /// Native invoker for the KFunction
        /// </summary>
        int InvokeKFunction(IntPtr L, int status, Int64 ctx)
        {
            var state = LuaState.FindState(L, true);
            if (state != null)
                return KFunction(state, status, ctx);
            return 0;
        }

        /// <summary>
        /// .Net K function
        /// </summary>
        public LuaKFunction KFunction { get; private set; }

        /// <summary>
        /// Lua native K function
        /// </summary>
        public LuaDll.lua_KFunction NativeFunction { get; private set; }
    }

}
