using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet
{
    /// <summary>
    /// Exception raised when a LuaState call the atpanic function
    /// </summary>
    public class LuaAtPanicException : Exception
    {
        /// <summary>
        /// Create a new AtPanic exception
        /// </summary>
        public LuaAtPanicException(String message)
            : base(message)
        {
        }

    }
}
