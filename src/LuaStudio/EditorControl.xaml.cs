using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;
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

namespace LuaStudio
{
    /// <summary>
    /// Logique d'interaction pour EditorControl.xaml
    /// </summary>
    public partial class EditorControl : UserControl
    {
        DispatcherTimer _FocusTimer;
        ITextFoldingStrategy _TextFolding;

        public EditorControl()
        {
            InitializeComponent();

            this.Loaded += EditorControl_Loaded;

            teEditor.TextArea.TextEntered += TextArea_TextEntered;
            teEditor.TextArea.TextEntering += TextArea_TextEntering;

            _FocusTimer = new DispatcherTimer();
            _FocusTimer.Interval = TimeSpan.FromSeconds(1);
            _FocusTimer.Tick += _FocusTimer_Tick;
        }

        private void TextDefinitionChanged(ITextDefinition oldDef, ITextDefinition newDef)
        {
            if(newDef!= null)
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
            
        }

        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            //UpdateFolding();
            _FocusTimer.Stop();
            _FocusTimer.Start();
        }

        FoldingManager foldingManager;
        private void UpdateFolding()
        {
            if (foldingManager == null)
                foldingManager = FoldingManager.Install(teEditor.TextArea);
            var foldings = ((_TextFolding != null) ? _TextFolding.BuildFoldings(foldingManager, teEditor.Document) : Enumerable.Empty<NewFolding>())
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
