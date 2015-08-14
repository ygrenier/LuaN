using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{

    /// <summary>
    /// Embedded Lua host service
    /// </summary>
    public interface ILuaHostService
    {
        /// <summary>
        /// Execute a code
        /// </summary>
        void Exec(String code);

        /// <summary>
        /// Evaluate an expression in the current host
        /// </summary>
        Object Eval(String expression);

        /// <summary>
        /// Current Lua state
        /// </summary>
        ILuaState Lua { get; }
    }

}
