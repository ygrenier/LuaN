#if DEBUG
using LuaN.Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaN.Studio.Models;
using LuaN.Studio.Services;
using System.Collections.ObjectModel;

namespace LuaN.Studio.DesignTime
{
    public class DtInteractiveLuaToolViewModel : IToolViewModel, IInteractiveLuaToolViewModel
    {
        class DtLuaHost : ILuaHostService
        {
            public DtLuaHost()
            {
                Console = new Models.Console();
                Console.Output.WriteLine("First line output");
                Console.Output.WriteLine("Second line output");
                Console.Input.WriteLine("First line input");
                Console.Error.WriteLine("First error line");
                Console.Input.WriteLine("Second line input");
                Console.Output.WriteLine("Third line output");
                Console.Output.Flush();
            }
            public IConsole Console { get; private set; }
            public ILuaState Lua { get; private set; }
            public event EventHandler<EventArgs> Started;
            public object Eval(string expression)
            {
                return null;
            }
            public void Exec(string code)
            {
            }
            public void Restart()
            {
            }
        }
        public DtInteractiveLuaToolViewModel()
        {
            Name = "InteractiveLua";
            Title = "InteractiveLua";
            LuaHost = new DtLuaHost();
            Console = LuaHost.Console;
        }
        public IConsole Console { get; private set; }
        public ILuaHostService LuaHost { get; private set; }
        public string Name { get; private set; }
        public string Title { get; private set; }
        public ObservableCollection<string> InputHistory { get; set; }
        public int HistoryIndex { get; set; }
        public string CurrentHistory { get; set; }
        public void DoString(string chunk)
        {
        }
        public bool ClearHistory()
        {
            return true;
        }
        public bool PrevHistory()
        {
            return true;
        }
        public bool NextHistory()
        {
            return true;
        }
        public bool SelectEndHistory()
        {
            return true;
        }
    }
}
#endif