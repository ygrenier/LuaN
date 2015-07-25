using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels
{

    public class ToolViewModel : ViewModel
    {
        public String Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value, () => Title); }
        }
        private String _Title;

        public bool IsVisible
        {
            get { return _IsVisible; }
            set { SetProperty(ref _IsVisible, value, () => IsVisible); }
        }
        private bool _IsVisible;

    }

}
