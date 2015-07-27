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
    /// Logique d'interaction pour InteractiveLuaToolView.xaml
    /// </summary>
    public partial class InteractiveLuaToolView : UserControl
    {
        public InteractiveLuaToolView()
        {
            InitializeComponent();
            this.Loaded += InteractiveLuaToolView_Loaded;
        }

        private void InteractiveLuaToolView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) DataContext = new ViewModels.Tools.InteractiveLuaToolViewModel();
            ViewModel.Start();
        }

        private void tInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (ViewModel != null && ViewModel.IsStarted)
                {
                    ViewModel.DoString(tInput.Text);
                }
                tInput.Clear();
            }
        }

        public ViewModels.Tools.InteractiveLuaToolViewModel ViewModel { get { return DataContext as ViewModels.Tools.InteractiveLuaToolViewModel; } }

    }
}
