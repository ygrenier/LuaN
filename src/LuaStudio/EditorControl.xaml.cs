using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Snippets;
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
using System.Windows.Threading;
using LuaStudio.TextEditors;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Threading.Tasks;

namespace LuaStudio
{
    /// <summary>
    /// Logique d'interaction pour EditorControl.xaml
    /// </summary>
    public partial class EditorControl : UserControl
    {
        readonly Object LockObject = new object();
        DispatcherTimer _FocusTimer;
        ITextFoldingStrategy _TextFolding;
        CompletionWindow _CompletionWindow;

        class CustomTabCommand : ICommand
        {
            public CustomTabCommand(EditorControl control, ICommand baseCommand)
            {
                this.Editor = control;
                this.TextEditor = control.teEditor;
                this.BaseCommand = baseCommand;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (TextEditor.SelectionLength == 0 && Editor.TextDefinition != null)
                {
                    int wordStart = DocumentUtilities.FindPrevWordStart(TextEditor.Document, TextEditor.CaretOffset);
                    if (wordStart >= 0)
                    {
                        string word = TextEditor.Document.GetText(wordStart, TextEditor.CaretOffset - wordStart);
                        var snippet = Editor.TextDefinition.FindSnippet(word);
                        if (snippet != null)
                        {
                            using (TextEditor.Document.RunUpdate())
                            {
                                TextEditor.Document.Remove(wordStart, TextEditor.CaretOffset - wordStart);
                                snippet.Insert(TextEditor.TextArea);
                            }
                            return;
                        }
                    }
                }
                BaseCommand.Execute(parameter);
            }

            public event EventHandler CanExecuteChanged { add { } remove { } }
            public EditorControl Editor { get; private set; }
            public TextEditor TextEditor { get; private set; }
            public ICommand BaseCommand { get; private set; }
        }
        class OpenCompletionCommand : ICommand
        {
            public OpenCompletionCommand(EditorControl control)
            {
                this.Editor = control;
                this.TextEditor = control.teEditor;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (TextEditor.SelectionLength == 0 && Editor.TextDefinition != null && Editor._CompletionWindow==null)
                {
                    String word = null;
                    int wordStart = DocumentUtilities.FindPrevWordStart(TextEditor.Document, TextEditor.CaretOffset);
                    if (wordStart > 0)
                    {
                        word = TextEditor.Document.GetText(wordStart, TextEditor.CaretOffset - wordStart);
                    }
                    else
                    {
                        wordStart = TextEditor.CaretOffset;
                    }
                    Editor.OpenCompletion(wordStart, word);
                }
            }

            public event EventHandler CanExecuteChanged { add { } remove { } }
            public EditorControl Editor { get; private set; }
            public TextEditor TextEditor { get; private set; }
        }
        public EditorControl()
        {
            InitializeComponent();

            this.Loaded += EditorControl_Loaded;

            teEditor.TextArea.TextEntered += TextArea_TextEntered;
            teEditor.TextArea.TextEntering += TextArea_TextEntering;

            _FocusTimer = new DispatcherTimer();
            _FocusTimer.Interval = TimeSpan.FromSeconds(1);
            _FocusTimer.Tick += _FocusTimer_Tick;

            var editingKeyBindings = teEditor.TextArea.DefaultInputHandler.Editing.InputBindings.OfType<KeyBinding>();
            var tabBinding = editingKeyBindings.Single(b => b.Key == Key.Tab && b.Modifiers == ModifierKeys.None);
            teEditor.TextArea.DefaultInputHandler.Editing.InputBindings.Remove(tabBinding);
            var newTabBinding = new KeyBinding(new CustomTabCommand(this, tabBinding.Command), tabBinding.Key, tabBinding.Modifiers);
            teEditor.TextArea.DefaultInputHandler.Editing.InputBindings.Add(newTabBinding);

            var kBind = new KeyBinding(new OpenCompletionCommand(this), Key.Space, ModifierKeys.Control);
            teEditor.TextArea.DefaultInputHandler.Editing.InputBindings.Add(kBind);
        }

        private void TextDefinitionChanged(ITextDefinition oldDef, ITextDefinition newDef)
        {
            if (newDef != null)
            {
                teEditor.SyntaxHighlighting = newDef.GetHighlightDefinition();
                _TextFolding = newDef.GetFoldingStrategy();
            }
            else
            {
                teEditor.SyntaxHighlighting = null;
                _TextFolding = null;
            }
            _FocusTimer.Stop();
            _FocusTimer.Start();
        }

        private void _FocusTimer_Tick(object sender, EventArgs e)
        {
            _FocusTimer.Stop();
            try
            {
                UpdateFolding();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetBaseException().Message);
            }
        }

        private void EditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            teEditor.Focus();
            _FocusTimer.Start();
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            //if (e.Text.Length > 0 && _CompletionWindow != null)
            //{
            //    if (!char.IsLetterOrDigit(e.Text[0]))
            //    {
            //        // Whenever a non-letter is typed while the completion window is open,
            //        // insert the currently selected element.
            //        _CompletionWindow.CompletionList.RequestInsertion(e);
            //    }
            //}
        }

        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            //UpdateFolding();
            _FocusTimer.Stop();
            _FocusTimer.Start();

            int wordStart = DocumentUtilities.FindPrevWordStart(teEditor.Document, teEditor.CaretOffset);
            if (TextDefinition!= null && _CompletionWindow == null && Char.IsLetterOrDigit(e.Text.FirstOrDefault()) && (teEditor.CaretOffset-wordStart)< 2)
            {
                OpenCompletion(wordStart, e.Text);
                //var td = TextDefinition;
                //var ta = teEditor.TextArea;
                
                //var cData = td.GetCompletionData(ta);
                //if (cData != null && cData.Any())
                //{
                //    CompletionWindow cw = new CompletionWindow(teEditor.TextArea)
                //    {
                //        CloseWhenCaretAtBeginning = true,
                //    };
                //    cw.CompletionList.IsFiltering = false;
                //    //cw.CompletionList.ListBox.ItemTemplateSelector
                //    cData = cData.OrderBy(d => d.Text).ThenBy(d => d.Priority);
                //    IList<ICompletionData> data = cw.CompletionList.CompletionData;
                //    foreach (var cd in cData)
                //        data.Add(cd);
                //    cw.Closed += delegate
                //    {
                //        lock (LockObject)
                //        _CompletionWindow = null;
                //    };
                //    cw.CompletionList.SelectionChanged += (cs, ce) =>
                //    {
                //        CompletionList cl = cs as CompletionList;
                //        if (cl != null && cl.SelectedItem == null && _CompletionWindow != null)
                //        {
                //            _CompletionWindow.Close();
                //            _CompletionWindow = null;
                //        }
                //    };
                //    cw.StartOffset = wordStart;
                //    cw.EndOffset = teEditor.CaretOffset;
                //    cw.CompletionList.SelectItem(e.Text);
                //    if (cw.CompletionList.SelectedItem != null)
                //    {
                //        _CompletionWindow = cw;
                //        cw.Show();
                //    }
                //    else
                //    {
                //        cw.Close();
                //    }
                //}
            }
        }

        void OpenCompletion(int wordStart, String selText)
        {
            var td = TextDefinition;
            var ta = teEditor.TextArea;

            var cData = td.GetCompletionData(ta);
            if (cData != null && cData.Any())
            {
                CompletionWindow cw = new CompletionWindow(teEditor.TextArea)
                {
                    CloseWhenCaretAtBeginning = true,
                };
                cw.CompletionList.IsFiltering = false;
                //cw.CompletionList.ListBox.ItemTemplateSelector
                cData = cData.OrderBy(d => d.Text).ThenBy(d => d.Priority);
                IList<ICompletionData> data = cw.CompletionList.CompletionData;
                foreach (var cd in cData)
                    data.Add(cd);
                cw.Closed += delegate
                {
                    lock (LockObject)
                    _CompletionWindow = null;
                };
                cw.CompletionList.SelectionChanged += (cs, ce) =>
                {
                    CompletionList cl = cs as CompletionList;
                    if (cl != null && cl.SelectedItem == null && _CompletionWindow != null)
                    {
                        _CompletionWindow.Close();
                        _CompletionWindow = null;
                    }
                };
                cw.StartOffset = wordStart;
                cw.EndOffset = teEditor.CaretOffset;
                if (!String.IsNullOrWhiteSpace(selText))
                    cw.CompletionList.SelectItem(selText);
                if (String.IsNullOrWhiteSpace(selText) || cw.CompletionList.SelectedItem != null)
                {
                    _CompletionWindow = cw;
                    cw.Show();
                }
                else
                {
                    cw.Close();
                }
            }
        }

        FoldingManager foldingManager;
        private void UpdateFolding()
        {
            if (foldingManager == null)
                foldingManager = FoldingManager.Install(teEditor.TextArea);
            var foldings = ((_TextFolding != null) ? _TextFolding.BuildFoldings(foldingManager, teEditor.TextArea) : Enumerable.Empty<NewFolding>())
                .OrderBy(f => f.StartOffset)
                .ToList();
            foldingManager.UpdateFoldings(foldings, -1);
            //FoldingManager.Uninstall(foldingManager);
        }

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(EditorControl), new PropertyMetadata(new TextDocument()));


        public TextEditors.ITextDefinition TextDefinition
        {
            get { return (TextEditors.ITextDefinition)GetValue(TextDefinitionProperty); }
            set { SetValue(TextDefinitionProperty, value); }
        }
        public static readonly DependencyProperty TextDefinitionProperty =
            DependencyProperty.Register("TextDefinition", typeof(TextEditors.ITextDefinition), typeof(EditorControl), new PropertyMetadata(null, TextDefinitionPropertyChanged));
        private static void TextDefinitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EditorControl)d).TextDefinitionChanged(e.OldValue as TextEditors.ITextDefinition, e.NewValue as TextEditors.ITextDefinition);
        }

    }

}
