using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LuaN.Studio.Views
{
    public class DockLayoutStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            Style result = null;
            if (item is ViewModels.Documents.TextEditorDocumentViewModel)
                result = TextEditorStyle;
            else if (item is ViewModels.ToolViewModel)
                result = ToolWindowStyle;
            return result ?? base.SelectStyle(item, container);
        }

        public Style TextEditorStyle { get; set; }

        public Style ToolWindowStyle { get; set; }
    }
}
