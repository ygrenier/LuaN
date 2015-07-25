using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LuaStudio.Views
{
    public class DockLayoutStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ViewModels.TextEditorViewModel)
                return TextEditorStyle;
            return base.SelectStyle(item, container);
        }

        public Style TextEditorStyle { get; set; }

    }
}
