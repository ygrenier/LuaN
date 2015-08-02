using LuaStudio.ViewModels;
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
            this.DataContextChanged += InteractiveLuaToolView_DataContextChanged;
        }

        private void InteractiveLuaToolView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.OldValue as ViewModels.Tools.InteractiveLuaToolViewModel;
            if (vm != null)
                vm.Console.Lines.CollectionChanged -= Lines_CollectionChanged;
            vm = e.NewValue as ViewModels.Tools.InteractiveLuaToolViewModel;
            if(vm!= null)
                vm.Console.Lines.CollectionChanged += Lines_CollectionChanged;
        }

        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems!= null)
            {
                foreach (ConsoleLine item in e.NewItems)
                {
                    var p = new Paragraph()
                    {
                        DataContext = item
                    };
                    switch (item.LineType)
                    {
                        case ConsoleLineType.Input:
                            p.Foreground = Brushes.BlueViolet;
                            p.Inlines.Add(new Run()
                            {
                                Text = "> "
                            });
                            break;
                        case ConsoleLineType.Error:
                            p.Foreground = Brushes.Red;
                            break;
                        case ConsoleLineType.Output:
                        default:
                            break;
                    }
                    p.Inlines.Add(item.Line);
                    consoleView.Document.Blocks.Add(p);
                }
            }
            if (e.OldItems != null)
            {
                if(e.Action==System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldStartingIndex == 0)
                {
                    foreach (ConsoleLine item in e.OldItems)
                    {
                        consoleView.Document.Blocks.Remove(consoleView.Document.Blocks.FirstBlock);
                    }
                }
            }
            //consoleView.ScrollToEnd();
        }

        private void InteractiveLuaToolView_Loaded(object sender, RoutedEventArgs e)
        {
            consoleView.TextChanged += ConsoleView_TextChanged;
            if (ViewModel == null) DataContext = new ViewModels.Tools.InteractiveLuaToolViewModel();
            ViewModel.Console.MaxLines = 80;
            ViewModel.Start();
        }

        private void ConsoleView_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if ((consoleView.VerticalOffset + consoleView.ViewportHeight) == consoleView.ExtentHeight || consoleView.ExtentHeight < consoleView.ViewportHeight)
                consoleView.ScrollToEnd();
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

        public ViewModels.Tools.InteractiveLuaToolViewModel ViewModel { get { return DataContext as ViewModels.Tools.InteractiveLuaToolViewModel; } }

    }
}
