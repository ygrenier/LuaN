﻿using LuaNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

namespace LuaStudio.ViewModels.Tools
{
    public class InteractiveLuaToolViewModel : ToolViewModel, IDisposable
    {
        ILuaState _CurrentState;

        public InteractiveLuaToolViewModel()
        {
            Title = "Lua Interactive";
            Console = new ConsoleViewModel();
            InputHistory = new ObservableCollection<string>();
            HistoryIndex = 0;
        }

        public void Dispose()
        {
            Stop();
        }

        ILuaState CreateNewState()
        {
            // Open all libs
            ILuaState result = new LuaNet.LuaLib.LuaState();
            result.OpenLibs();

            result.OnPrint += State_OnPrint;
            result.OnWriteLine += State_OnWriteLine;
            result.OnWriteString += State_OnWriteString;
            result.OnWriteStringError += State_OnWriteStringError;

            return result;
        }

        private void State_OnWriteStringError(object sender, WriteEventArgs e)
        {
            //Write(e.Text);
            Console.InsertError(e.Text);
            e.Handled = true;
        }

        private void State_OnWriteString(object sender, WriteEventArgs e)
        {
            //Write(e.Text);
            Console.InsertOutput(e.Text);
            e.Handled = true;
        }

        private void State_OnWriteLine(object sender, WriteEventArgs e)
        {
            //Write(Environment.NewLine);
            Console.InsertOutput(Environment.NewLine);
            e.Handled = true;
        }

        private void State_OnPrint(object sender, WriteEventArgs e)
        {
            Write(e.Text + Environment.NewLine);
            e.Handled = true;
        }

        private void Write(String s)
        {
            //Output += s;
            Console.InsertOutput(s);
        }

        /// <summary>
        /// Start a session
        /// </summary>
        public void Start()
        {
            if (!IsStarted)
            {
                _CurrentState = CreateNewState();
                
                _CurrentState.GetGlobal("print");
                _CurrentState.PushString(_CurrentState.LuaCopyright);
                var st = _CurrentState.PCall(1, 0, 0);

                IsStarted = true;
            }
        }

        /// <summary>
        /// Stop the current session
        /// </summary>
        public void Stop()
        {
            if (IsStarted)
            {
                try
                {
                    _CurrentState.OnPrint -= State_OnPrint;
                    _CurrentState.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.GetBaseException().Message);
                }
                _CurrentState = null;

                IsStarted = false;
            }
        }

        /// <summary>
        /// Run a lua chunk
        /// </summary>
        public void DoString(String chunk)
        {
            if (String.IsNullOrWhiteSpace(chunk) && !IsStarted) return;
            //Output += String.Format("> {0}\n", chunk);
            Console.InsertInput(chunk);
            InputHistory.Add(chunk);
            HistoryIndex = InputHistory.Count;
            if (chunk.StartsWith("="))
            {
                chunk = String.Format("return {0}", chunk.Substring(1));
            }
            _CurrentState.SetTop(0);
            // Try the command as "return <chunk>"
            if (_CurrentState.LoadBuffer("return " + chunk, "InteractiveLua") != LuaStatus.Ok)
            {
                _CurrentState.SetTop(0);
                if (_CurrentState.LoadBuffer(chunk, "InteractiveLua") != LuaStatus.Ok)
                {
                    _CurrentState.WriteStringError("error loading chunk : %s\n", _CurrentState.ToString(-1));
                    return;
                }
            }
            _CurrentState.PCall(0, _CurrentState.MultiReturns, 0);

            int n = _CurrentState.GetTop();
            if (n > 0)
            {
                _CurrentState.CheckStack(_CurrentState.MinStack, "too many results to print");
                _CurrentState.GetGlobal("print");
                _CurrentState.Insert(1);
                if (_CurrentState.PCall(n, 0, 0) != LuaStatus.Ok)
                    //l_message(L, progname, L.PushFString("error calling 'print' (%s)", L.ToString(-1)));
                    _CurrentState.WriteStringError("error calling 'print' (%s)\n", _CurrentState.ToString(-1));
            }
            Write(Environment.NewLine);
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
        /// Indicate if the session is started
        /// </summary>
        public bool IsStarted
        {
            get { return _IsStarted; }
            protected set { SetProperty(ref _IsStarted, value, () => IsStarted); }
        }
        private bool _IsStarted;

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
            private set {
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
        /// Content of the console
        /// </summary>
        public ConsoleViewModel Console { get; private set; }

        /// <summary>
        /// Preferred bottom
        /// </summary>
        public override AnchorSide PreferredSide { get { return AnchorSide.Bottom; } }

    }

}
