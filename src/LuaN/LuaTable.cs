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
    }
}
