using Fluent;
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
using System.ComponentModel;

namespace LuaStudio
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.AppViewModel();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (ViewModel != null)
            {
                ViewModel.Shutdown();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (ViewModel != null)
            {
                if (!ViewModel.CloseAllDocuments())
                {
                    e.Cancel = true;
                }
            }
        }

        public ViewModels.AppViewModel ViewModel { get { return (ViewModels.AppViewModel)DataContext; } }

    }
}
