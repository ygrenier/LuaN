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
            if (item is ViewModels.Documents.TextEditorViewModel)
                return TextEditorStyle;
            if (item is ViewModels.ToolViewModel)
                return ToolWindowStyle;
            return base.SelectStyle(item, container);
        }

        public Style TextEditorStyle { get; set; }

        public Style ToolWindowStyle { get; set; }

    }
}
