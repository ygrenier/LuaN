using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels
{

    /// <summary>
    /// Base of the viewmodel for the tools
    /// </summary>
    public abstract class ToolViewModel : ViewModel, ITool, IToolViewModel
    {
        /// <summary>
        /// Unique name of the tools
        /// </summary>
        public abstract String Name { get; }

        /// <summary>
        /// Title of the tools
        /// </summary>
        public abstract String Title { get; }
    }

}
