using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LuaN.Studio.Views
{
    public class DockLayoutTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate result = null;
            if (item is ViewModels.Documents.TextEditorDocumentViewModel)
                result = TextEditorTemplate;
            else if (item is ViewModels.ToolViewModel)
                result = ToolWindowTemplate;
            return result ?? base.SelectTemplate(item, container);
        }

        public DataTemplate TextEditorTemplate { get; set; }

        public DataTemplate ToolWindowTemplate { get; set; }
    }
}
