using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.DllWrapper
{

    /// <summary>
    /// Wrapper of LuaCFunction
    /// </summary>
    public class LuaCFunctionWrapper
    {

        /// <summary>
        /// Wrapper for a .Net C function
        /// </summary>
        public LuaCFunctionWrapper(LuaCFunction function)
        {
            if (function == null) throw new ArgumentNullException("function");
            this.CFunction = function;
            this.NativeFunction = InvokeCFunction;
        }

        /// <summary>
        /// Wrapper for a native C function
        /// </summary>
        public LuaCFunctionWrapper(LuaDll.lua_CFunction function)
        {
            if (function == null) throw new ArgumentNullException("function");
            this.NativeFunction = function;
            this.CFunction = InvokeNativeFunction;
        }

        /// <summary>
        /// Native invoker for the CFunction
        /// </summary>
        int InvokeCFunction(IntPtr L)
        {
            var state = LuaState.FindState(L, true);
            if (state != null)
                return CFunction(state);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        int InvokeNativeFunction(ILuaState L)
        {
            LuaState state = L as LuaState;
            if(state!= null)
                return NativeFunction(state.NativeState);
            return 0;
        }

        /// <summary>
        /// .Net C function
        /// </summary>
        public LuaCFunction CFunction { get; private set; }

        /// <summary>
        /// Lua native C function
        /// </summary>
        public LuaDll.lua_CFunction NativeFunction { get; private set; }
    }

}
