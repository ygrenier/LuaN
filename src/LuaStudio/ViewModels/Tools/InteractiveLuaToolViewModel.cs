using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels.Tools
{
    public class InteractiveLuaToolViewModel : ToolViewModel
    {

        public InteractiveLuaToolViewModel()
        {
            Title = "Lua Interactive";
        }

        /// <summary>
        /// Start a session
        /// </summary>
        public void Start()
        {
            if (!IsStarted)
            {
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
                IsStarted = false;
            }
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

    }
}
