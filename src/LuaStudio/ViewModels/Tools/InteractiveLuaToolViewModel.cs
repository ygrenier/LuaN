using LuaNet;
using System;
using System.Collections.Generic;
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
            if (chunk.StartsWith("="))
            {
                chunk = String.Format("return {0}", chunk.Substring(1));
            }
            _CurrentState.SetTop(0);
            // Try the command as "return <chunk>"
            if (_CurrentState.LoadBuffer("return " + chunk, "InteractiveLua") != LuaStatus.OK)
            {
                _CurrentState.SetTop(0);
                if (_CurrentState.LoadBuffer(chunk, "InteractiveLua") != LuaStatus.OK)
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
                if (_CurrentState.PCall(n, 0, 0) != LuaStatus.OK)
                    //l_message(L, progname, L.PushFString("error calling 'print' (%s)", L.ToString(-1)));
                    _CurrentState.WriteStringError("error calling 'print' (%s)\n", _CurrentState.ToString(-1));
            }
            Write(Environment.NewLine);
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

        //public String Output
        //{
        //    get { return _Output; }
        //    private set { SetProperty(ref _Output, value, () => Output); }
        //}
        //private String _Output;

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
