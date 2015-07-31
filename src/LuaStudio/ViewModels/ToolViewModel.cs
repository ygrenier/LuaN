using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

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

        public virtual AnchorSide PreferredSide { get { return AnchorSide.Right; } }

    }

}
