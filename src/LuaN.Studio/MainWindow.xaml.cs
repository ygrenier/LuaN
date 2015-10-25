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

namespace LuaN.Studio
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = AppContext.Current.ServiceLocator.GetService<ViewModels.ShellViewModel>();
        }

        /// <summary>
        /// Current viewmodel
        /// </summary>
        public ViewModels.ShellViewModel ViewModel { get { return (ViewModels.ShellViewModel)DataContext; } }

    }
}
