using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Default ILuaFunction implementation
    /// </summary>
    public class LuaFunction : LuaValue, ILuaFunction
    {

        /// <summary>
        /// Create a new function reference
        /// </summary>
        public LuaFunction(Lua lua, int reference, bool ownRef = true)
        {
            this.Lua = lua;
            this.Reference = reference;
            this.ReferenceOwned = ownRef;
            this.Function = null;
        }

        /// <summary>
        /// Create a new function
        /// </summary>
        public LuaFunction(Lua lua, LuaCFunction function)
        {
            this.Lua = lua;
            this.Reference = LuaRef.NoRef;
            this.ReferenceOwned = false;
            this.Function = function;
        }

        /// <summary>
        /// Call this userdata
        /// </summary>
        public object[] Call(params object[] args)
        {
            return Lua.CallFunction(this, args);
        }

        /// <summary>
        /// Call this userdata
        /// </summary>
        public object[] Call(object[] args, Type[] typedResult)
        {
            return Lua.CallFunction(this, args, typedResult);
        }

        /// <summary>
        /// Push the function
        /// </summary>
        protected internal override void Push()
        {
            if (Function != null)
                Lua.State.LuaPushCFunction(Function);
            else
                base.Push();
        }

        /// <summary>
        /// Function
        /// </summary>
        public LuaCFunction Function { get; private set; }
    }

}
