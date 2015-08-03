using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Managed C function for Lua
    /// </summary>
    public delegate int LuaCFunction(ILuaState state);

    /// <summary>
    /// Managed continious function for Lua
    /// </summary>
    public delegate int LuaKFunction(ILuaState state, int status, Int64 ctx);

    /// <summary>
    /// Managed read function
    /// </summary>
    public delegate Byte[] LuaReader(ILuaState L, Object ud);

    /// <summary>
    /// Managed write function
    /// </summary>
    public delegate int LuaWriter(ILuaState L, Byte[] p, Object ud);

}
