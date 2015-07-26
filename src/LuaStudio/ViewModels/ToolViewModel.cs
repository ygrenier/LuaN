using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels
{

    public class ToolViewModel : DockContentViewModel
    {
        public bool IsVisible
        {
            get { return _IsVisible; }
            set { SetProperty(ref _IsVisible, value, () => IsVisible); }
        }
        private bool _IsVisible;
    }

}
