using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Represents a Lua userdata
    /// </summary>
    public interface ILuaUserData : ILuaValue, IDisposable
    {

        /// <summary>
        /// Call this userdata
        /// </summary>
        object[] Call(params object[] args);

        /// <summary>
        /// Call this userdata
        /// </summary>
        object[] Call(object[] args, Type[] typedResult);

        /// <summary>
        /// Access to the named fields
        /// </summary>
        object this[String field] { get; set; }

        /// <summary>
        /// Access to the fields
        /// </summary>
        object this[object index] { get; set; }

    }

}
