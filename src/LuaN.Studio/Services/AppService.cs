using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{

    /// <summary>
    /// Service application
    /// </summary>
    public class AppService : IAppService
    {
        IServiceLocator _Services;
        ILuaHostService _LuaHost;
        IShell _Shell;

        /// <summary>
        /// Create a new service
        /// </summary>
        public AppService(IServiceLocator services)
        {
            this._Services = services;
        }

        /// <summary>
        /// Lua host
        /// </summary>
        public ILuaHostService LuaHost { get { return _LuaHost ?? (_LuaHost = _Services.GetService<ILuaHostService>()); } }

        /// <summary>
        /// Shell
        /// </summary>
        public IShell Shell { get { return _Shell ?? (_Shell = _Services.GetService<IShell>()); } }

    }

}
