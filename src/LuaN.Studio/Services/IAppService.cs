using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{
    /// <summary>
    /// Application service
    /// </summary>
    public interface IAppService
    {

        /// <summary>
        /// Lua host
        /// </summary>
        ILuaHostService LuaHost { get; }

        /// <summary>
        /// Shell
        /// </summary>
        IShell Shell { get; }

    }
}
