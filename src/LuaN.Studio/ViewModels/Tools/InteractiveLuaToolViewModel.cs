using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels.Tools
{

    /// <summary>
    /// Interactive Lua tool
    /// </summary>
    public class InteractiveLuaToolViewModel : ToolViewModel, IInteractiveLuaToolViewModel
    {
        /// <summary>
        /// New ViewModel
        /// </summary>
        public InteractiveLuaToolViewModel(Services.ILuaHostService luaHost)
        {
            this.LuaHost = luaHost;
            this.LuaHost.Started += LuaHost_Started;
            this.InputHistory = new ObservableCollection<string>();
            this.HistoryIndex = 0;
        }

        private void LuaHost_Started(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => LuaHost);
            RaisePropertyChanged(() => Console);
        }

        /// <summary>
        /// Run a lua chunk
        /// </summary>
        public void DoString(String chunk)
        {
            if (String.IsNullOrWhiteSpace(chunk)) return;
            //Output += String.Format("> {0}\n", chunk);
            Console.Input.Write(chunk);
            Console.Input.Flush();
            InputHistory.Add(chunk);
            HistoryIndex = InputHistory.Count;
            if (chunk.StartsWith("="))
            {
                chunk = String.Format("return {0}", chunk.Substring(1));
            }
            LuaHost.Lua.LuaSetTop(0);
            // Try the command as "return <chunk>"
            if (LuaHost.Lua.LuaLoadBuffer("return " + chunk, "InteractiveLua") != LuaStatus.Ok)
            {
                LuaHost.Lua.LuaSetTop(0);
                if (LuaHost.Lua.LuaLoadBuffer(chunk, "InteractiveLua") != LuaStatus.Ok)
                {
                    LuaHost.Lua.LuaWriteStringError("error loading chunk : %s\n", LuaHost.Lua.LuaToString(-1));
                    return;
                }
            }
            LuaHost.Lua.LuaPCall(0, LuaHost.Lua.MultiReturns, 0);

            int n = LuaHost.Lua.LuaGetTop();
            if (n > 0)
            {
                LuaHost.Lua.LuaLCheckStack(LuaHost.Lua.MinStack, "too many results to print");
                LuaHost.Lua.LuaGetGlobal("print");
                LuaHost.Lua.LuaInsert(1);
                if (LuaHost.Lua.LuaPCall(n, 0, 0) != LuaStatus.Ok)
                    //l_message(L, progname, L.PushFString("error calling 'print' (%s)", L.ToString(-1)));
                    LuaHost.Lua.LuaWriteStringError("error calling 'print' (%s)\n", LuaHost.Lua.LuaToString(-1));
            }
            Console.Output.WriteLine();
            Console.Output.Flush();
        }

        /// <summary>
        /// Clear the history
        /// </summary>
        public bool ClearHistory()
        {
            if (InputHistory.Count > 0)
            {
                InputHistory.Clear();
                HistoryIndex = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Select the previous history
        /// </summary>
        public bool PrevHistory()
        {
            if (HistoryIndex > 0)
            {
                HistoryIndex--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Select the next history
        /// </summary>
        public bool NextHistory()
        {
            if (HistoryIndex < InputHistory.Count)
            {
                HistoryIndex++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Select the last entry in the history
        /// </summary>
        public bool SelectEndHistory()
        {
            if (HistoryIndex < InputHistory.Count)
            {
                HistoryIndex = InputHistory.Count;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Input history
        /// </summary>
        public ObservableCollection<String> InputHistory { get; private set; }

        /// <summary>
        /// Current selected history index
        /// </summary>
        public int HistoryIndex
        {
            get { return _HistoryIndex; }
            private set
            {
                if (SetProperty(ref _HistoryIndex, value, () => HistoryIndex))
                    RaisePropertyChanged(() => CurrentHistory);
            }
        }
        private int _HistoryIndex;

        /// <summary>
        /// Current history
        /// </summary>
        public String CurrentHistory { get { return HistoryIndex < 0 || HistoryIndex >= InputHistory.Count ? String.Empty : InputHistory[HistoryIndex]; } }

        /// <summary>
        /// Name
        /// </summary>
        public override string Name { get { return "InteractiveLua"; } }

        /// <summary>
        /// Title
        /// </summary>
        public override string Title { get { return "Interactive Lua"; } }

        /// <summary>
        /// Lua host
        /// </summary>
        public Services.ILuaHostService LuaHost { get; private set; }

        /// <summary>
        /// Console
        /// </summary>
        public Models.IConsole Console { get { return LuaHost.Console; } }
    }

}
