using LuaN.Studio.Models;
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
            this.Console = new Models.Console();
            Start();
        }

        private void Lua_OnPrint(object sender, WriteEventArgs e)
        {
            Console.InsertOutput(e.Text + Environment.NewLine);
            e.Handled = true;
        }

        private void Lua_OnWriteString(object sender, WriteEventArgs e)
        {
            Console.InsertOutput(e.Text);
            e.Handled = true;
        }

        private void Lua_OnWriteStringError(object sender, WriteEventArgs e)
        {
            Console.InsertError(e.Text);
            e.Handled = true;
        }

        /// <summary>
        /// Start the context
        /// </summary>
        protected void Start()
        {
            if (_Lua != null) return;

            this.Console = new Models.Console();

            _Lua = new DllWrapper.LuaState();

            _Lua.OnPrint += Lua_OnPrint;
            _Lua.OnWriteString += Lua_OnWriteString;
            _Lua.OnWriteLine += Lua_OnWriteString;
            _Lua.OnWriteStringError += Lua_OnWriteStringError;

            _Lua.LuaLOpenLibs();
            _Lua.OpenDotnet();

            // Register the app service
            _Lua.PushNetObject(_Services.GetService<IAppService>());
            _Lua.LuaSetGlobal("app");

            // Register the help command
            _Lua.LuaRegister("help", HelpFunction);

            // Print copyright & help
            StringBuilder msg = new StringBuilder()
                .AppendLine(_Lua.Engine.LuaCopyright)
                .AppendLine("For display help type: help()")
                .AppendLine()
                ;
            _Lua.LuaGetGlobal("print");
            _Lua.LuaPushString(msg.ToString());
            _Lua.LuaPCall(1, 0, 0);

            // Raise event
            var h = Started;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        /// <summary>
        /// Help function
        /// </summary>
        private int HelpFunction(ILuaState state)
        {
            StringBuilder usage = new StringBuilder()
                .AppendLine("Lua Interactive Help")
                .AppendLine("- app : Access to the LuaN.Studio application object")
                .AppendLine(" * app.Shell : Access to the shell object")
                ;
            _Lua.LuaGetGlobal("print");
            _Lua.LuaPushString(usage.ToString());
            _Lua.LuaPCall(1, 0, 0);

            return 0;
        }

        /// <summary>
        /// Stop the context
        /// </summary>
        protected void Stop()
        {
            if (_Lua == null) return;

            _Lua.OnPrint -= Lua_OnPrint;
            _Lua.OnWriteString -= Lua_OnWriteString;
            _Lua.OnWriteLine -= Lua_OnWriteString;
            _Lua.OnWriteStringError -= Lua_OnWriteStringError;

            _Lua.Dispose();
            _Lua = null;
        }

        /// <summary>
        /// Reset the Lua context
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
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
            get
            {
                if (_Lua == null)
                    Start();
                return _Lua;
            }
        }

        /// <summary>
        /// Console
        /// </summary>
        public Models.Console Console { get; private set; }
        IConsole ILuaHostService.Console { get { return Console; } }

        /// <summary>
        /// Event raised when the host is started or restarted
        /// </summary>
        public event EventHandler<EventArgs> Started;

    }

}
