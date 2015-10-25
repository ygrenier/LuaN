using LuaN.Studio.Models;
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

namespace LuaN.Studio.Controls
{
    /// <summary>
    /// Logique d'interaction pour ConsoleControl.xaml
    /// </summary>
    public partial class ConsoleControl : UserControl
    {
        public ConsoleControl()
        {
            InitializeComponent();
            this.Loaded += ConsoleControl_Loaded;
        }

        private void ConsoleControl_Loaded(object sender, RoutedEventArgs e)
        {
            consoleView.TextChanged += ConsoleView_TextChanged;
        }

        private void ConsoleView_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if ((consoleView.VerticalOffset + consoleView.ViewportHeight) == consoleView.ExtentHeight || consoleView.ExtentHeight < consoleView.ViewportHeight)
            consoleView.ScrollToEnd();
        }

        void AddConsoleLine(ConsoleLine line)
        {
            var p = new Paragraph()
            {
                DataContext = line
            };
            switch (line.LineType)
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
            p.Inlines.Add(line.Line);
            consoleView.Document.Blocks.Add(p);
        }

        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Update ?
            if (e.NewStartingIndex == e.OldStartingIndex)
            {
                int pidx = e.NewStartingIndex;
                foreach (ConsoleLine item in e.NewItems)
                {
                    var p = consoleView.Document.Blocks.ElementAtOrDefault(pidx) as Paragraph;
                    if (p != null && p.Inlines.Count > 0)
                    {
                        p.Inlines.Remove(p.Inlines.Last());
                        p.Inlines.Add(item.Line);
                    }
                    pidx++;
                }
            }
            else
            {
                if (e.NewItems != null)
                {
                    foreach (ConsoleLine item in e.NewItems)
                        AddConsoleLine(item);
                }
                if (e.OldItems != null)
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldStartingIndex == 0)
                    {
                        foreach (ConsoleLine item in e.OldItems)
                        {
                            consoleView.Document.Blocks.Remove(consoleView.Document.Blocks.FirstBlock);
                        }
                    }
                }
            }
            //consoleView.ScrollToEnd();
        }

        private void ConsoleChanged(DependencyPropertyChangedEventArgs e)
        {
            var cons = e.OldValue as IConsole;
            if (cons != null)
                cons.Lines.CollectionChanged -= Lines_CollectionChanged;
            consoleView.Document.Blocks.Clear();
            cons = e.NewValue as IConsole;
            if (cons != null)
            {
                cons.Lines.CollectionChanged += Lines_CollectionChanged;
                foreach (ConsoleLine line in cons.Lines)
                    AddConsoleLine(line);
            }
        }

        /// <summary>
        /// Console
        /// </summary>
        public IConsole Console
        {
            get { return (IConsole)GetValue(ConsoleProperty); }
            set { SetValue(ConsoleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Console.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConsoleProperty =
            DependencyProperty.Register("Console", typeof(IConsole), typeof(ConsoleControl), new PropertyMetadata(null, ConsolePropertyChanged));
        private static void ConsolePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConsoleControl)d).ConsoleChanged(e);
        }

    }
}
