using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels
{

    /// <summary>
    /// Main ViewModel
    /// </summary>
    public class ShellViewModel : ViewModel, IShellViewModel, IShell
    {

        /// <summary>
        /// 
        /// </summary>
        public ShellViewModel(Services.IAppService appService)
        {
            this.App = appService;
        }

        /// <summary>
        /// Application
        /// </summary>
        public Services.IAppService App { get; private set; }

    }

}
