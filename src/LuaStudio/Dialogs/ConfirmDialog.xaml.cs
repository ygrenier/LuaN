using LuaStudio.Services;
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
using System.Windows.Shapes;

namespace LuaStudio.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        private void dialogButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                bool? bresult = false;
                var dlb = btn.DataContext as IDialogButton;
                if (dlb != null)
                {
                    if (dlb.IsDefault)
                        bresult = true;
                    else if (dlb.IsCancel)
                        bresult = false;
                }
                DialogResult = bresult;
            }
        }

    }
}
