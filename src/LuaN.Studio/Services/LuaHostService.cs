using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{

    /// <summary>
    /// Lua host service
    /// </summary>
    public class LuaHostService : ILuaHostService
    {
        private ILuaState _Lua = null;
        private IServiceLocator _Services;

        /// <summary>
        /// Create a new Lua host service
        /// </summary>
        public LuaHostService(IServiceLocator services)
        {
            this._Services = services;
        }

        /// <summary>
        /// Create a new Lua state
        /// </summary>
        protected ILuaState CreateLua()
        {
            var result = new DllWrapper.LuaState();
            result.OnWriteString += Lua_OnWriteString;
            result.OnWriteLine += Lua_OnWriteString;
            result.OnWriteStringError += Lua_OnWriteStringError;

            // Register the app service
            result.PushNetObject(_Services.GetService<IAppService>());
            result.LuaSetGlobal("app");

            return result;
        }

        private void Lua_OnWriteString(object sender, WriteEventArgs e)
        {
        }

        private void Lua_OnWriteStringError(object sender, WriteEventArgs e)
        {
        }

        /// <summary>
        /// Execute a code
        /// </summary>
        public void Exec(String code)
        {
            if (!String.IsNullOrWhiteSpace(code))
                Lua.DoString(code, "script");
        }

        /// <summary>
        /// Evaluate an expression
        /// </summary>
        public object Eval(string expression)
        {
            if (String.IsNullOrWhiteSpace("expression")) return null;
            var result = Lua.DoString(String.Format("return {0}", expression));
            if (result == null) return null;
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Lua state
        /// </summary>
        public ILuaState Lua
        {
            get{ return _Lua ?? (_Lua = CreateLua()); }
        }

    }

}
