using LuaN.Studio.Models;
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
        /// Reset the Lua context
        /// </summary>
        void Restart();

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

        /// <summary>
        /// Console for IO lines
        /// </summary>
        IConsole Console { get; }

        /// <summary>
        /// Event raised when the host is started or restarted
        /// </summary>
        event EventHandler<EventArgs> Started;
    }

}
