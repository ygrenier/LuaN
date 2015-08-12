using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// .Net management provider
    /// </summary>
    public interface ILuaDotnet
    {
        /// <summary>
        /// Convert a Lua table to ILuaTable
        /// </summary>
        ILuaTable ToTable(int idx);

        /// <summary>
        /// Convert a Lua function or C function to ILuaFunction
        /// </summary>
        ILuaFunction ToFunction(int idx);

        /// <summary>
        /// Convert an userdata to ILuaUserData
        /// </summary>
        ILuaUserData ToUserData(int idx);

        /// <summary>
        /// Convert the Lua value at the index to a .Net object corresponding
        /// </summary>
        Object ToValue(int idx);

        /// <summary>
        /// Push a .Net object value converted to the Lua value corresponding
        /// </summary>
        void Push(Object value);
    }

}
