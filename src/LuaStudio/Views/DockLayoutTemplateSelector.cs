using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LuaStudio.Views
{
    public class DockLayoutTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ViewModels.TextEditorViewModel)
                return TextEditorTemplate;
            return base.SelectTemplate(item, container);
        }

        public DataTemplate TextEditorTemplate { get; set; }

    }
}
