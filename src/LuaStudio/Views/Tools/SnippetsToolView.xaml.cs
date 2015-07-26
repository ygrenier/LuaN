using LuaStudio.TextEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LuaStudio.Views.Tools
{
    /// <summary>
    /// Logique d'interaction pour SnippetsToolView.xaml
    /// </summary>
    public partial class SnippetsToolView : UserControl
    {
        public SnippetsToolView()
        {
            InitializeComponent();
        }

        private void listSnippets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var snippet = listSnippets.SelectedItem as SnippetDefinition;
            if (snippet == null) return;
            var vm = DataContext as ViewModels.Tools.SnippetsToolViewModel;
            if (vm != null && vm.InsertSnippetCommand != null)
            {
                if (vm.InsertSnippetCommand.CanExecute(snippet))
                    vm.InsertSnippetCommand.Execute(snippet);
            }
        }
    }
}
