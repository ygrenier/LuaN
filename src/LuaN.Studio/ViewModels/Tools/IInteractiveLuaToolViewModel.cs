using LuaN.Studio.Services;
using System;
using System.Collections.ObjectModel;

namespace LuaN.Studio.ViewModels
{
    public interface IInteractiveLuaToolViewModel
    {
        void DoString(String chunk);
        bool ClearHistory();
        bool PrevHistory();
        bool NextHistory();
        bool SelectEndHistory();
        ObservableCollection<String> InputHistory { get; }
        int HistoryIndex{get;}
        String CurrentHistory { get; }
        ILuaHostService LuaHost { get; }
        string Name { get; }
        string Title { get; }
        Models.IConsole Console { get; }
    }
}