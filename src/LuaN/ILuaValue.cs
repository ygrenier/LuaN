using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Represents a Lua value
    /// </summary>
    public interface ILuaValue
    {
        /// <summary>
        /// Push the value
        /// </summary>
        void Push(ILuaState state);
    }
}
