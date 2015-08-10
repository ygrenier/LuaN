using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Represents a Lua function
    /// </summary>
    public interface ILuaFunction : ILuaValue, IDisposable
    {

        /// <summary>
        /// Call this userdata
        /// </summary>
        object[] Call(params object[] args);

        /// <summary>
        /// Call this userdata
        /// </summary>
        object[] Call(object[] args, Type[] typedResult);

    }

}
