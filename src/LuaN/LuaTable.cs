using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Default ILuaTable implementation
    /// </summary>
    public class LuaTable : LuaValue, ILuaTable
    {
        /// <summary>
        /// Create a new table reference
        /// </summary>
        public LuaTable(Lua lua, int reference, bool ownRef= true)
        {
            this.Lua = lua;
            this.Reference = reference;
            this.ReferenceOwned = ownRef;
        }

        /// <summary>
        /// Access to the named fields
        /// </summary>
        public object this[String field]
        {
            get
            {
                var oldTop = Lua.State.LuaGetTop();
                Lua.State.LuaPushRef(Reference);
                Lua.State.LuaGetField(-1, field);
                var result = Lua.ToValue(-1);
                Lua.State.LuaSetTop(oldTop);
                return result;
            }
            set
            {
                var oldTop = Lua.State.LuaGetTop();
                Lua.State.LuaPushRef(Reference);
                Lua.Push(value);
                Lua.State.LuaSetField(-2, field);
                Lua.State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Access to integer fields
        /// </summary>
        public object this[int index]
        {
            get
            {
                var oldTop = Lua.State.LuaGetTop();
                Lua.State.LuaPushRef(Reference);
                Lua.State.LuaGetI(-1, index);
                var result = Lua.ToValue(-1);
                Lua.State.LuaSetTop(oldTop);
                return result;
            }
            set
            {
                var oldTop = Lua.State.LuaGetTop();
                Lua.State.LuaPushRef(Reference);
                Lua.Push(value);
                Lua.State.LuaSetI(-2, index);
                Lua.State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Access to the fields
        /// </summary>
        public object this[object index]
        {
            get
            {
                var oldTop = Lua.State.LuaGetTop();
                Lua.State.LuaPushRef(Reference);
                Lua.Push(index);
                Lua.State.LuaGetTable(-2);
                var result = Lua.ToValue(-1);
                Lua.State.LuaSetTop(oldTop);
                return result;
            }
            set
            {
                var oldTop = Lua.State.LuaGetTop();
                Lua.State.LuaPushRef(Reference);
                Lua.Push(index);
                Lua.Push(value);
                Lua.State.LuaSetTable(-3);
                Lua.State.LuaSetTop(oldTop);
            }
        }

    }

}
