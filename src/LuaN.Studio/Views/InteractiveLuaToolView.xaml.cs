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

namespace LuaN.Studio.Views
{
    /// <summary>
    /// Logique d'interaction pour InteractiveLuaToolView.xaml
    /// </summary>
    public partial class InteractiveLuaToolView : UserControl
    {
        public InteractiveLuaToolView()
        {
            InitializeComponent();
        }

        private void tInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel == null) return;
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                ViewModel.DoString(tInput.Text);
                tInput.Clear();
                tInput.Text = ViewModel.CurrentHistory;
            }
            else if (e.Key == Key.Escape)
            {
                if (ViewModel.SelectEndHistory())
                {
                    tInput.Text = ViewModel.CurrentHistory;
                }
            }
        }

        private void tInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel == null) return;
            if (e.Key == Key.Up)
            {
                if (ViewModel.PrevHistory())
                    tInput.Text = ViewModel.CurrentHistory;
            }
            else if (e.Key == Key.Down)
            {
                if (ViewModel.NextHistory())
                    tInput.Text = ViewModel.CurrentHistory;
            }
        }

        public ViewModels.IInteractiveLuaToolViewModel ViewModel { get { return DataContext as ViewModels.IInteractiveLuaToolViewModel; } }

    }
}
