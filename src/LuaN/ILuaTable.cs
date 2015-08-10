using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Represents a Lua value table
    /// </summary>
    public interface ILuaTable : ILuaValue, IDisposable
    {

        /// <summary>
        /// Access to the named fields
        /// </summary>
        object this[String field] { get; set; }

        /// <summary>
        /// Access to integer fields
        /// </summary>
        object this[int index] { get; set; }

        /// <summary>
        /// Access to the fields
        /// </summary>
        object this[object index] { get; set; }
    }

}
