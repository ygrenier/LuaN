using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Lua Exception
    /// </summary>
    public class LuaException : Exception
    {
        /// <summary>
        /// New exception
        /// </summary>
        public LuaException() : base()
        {
        }
        /// <summary>
        /// New exception with a message
        /// </summary>
        public LuaException(String message) : base(message)
        {
        }
    }

}
