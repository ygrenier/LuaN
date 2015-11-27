using ICSharpCode.AvalonEdit.Document;
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
using LuaN.Studio.TextEditors;

namespace LuaN.Studio.Controls
{
    /// <summary>
    /// Logique d'interaction pour TextEditorControl.xaml
    /// </summary>
    public partial class TextEditorControl : UserControl
    {
        public TextEditorControl()
        {
            InitializeComponent();
            this.Loaded += TextEditorControl_Loaded;
        }

        private void TextEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            try { teEditor.Focus(); } catch { }
            if (TextDefinition != null)
                teEditor.SyntaxHighlighting = TextDefinition.GetHighlightDefinition();
        }

        private void TextDefinitionChanged(ITextDefinition oldDefinition, ITextDefinition nextDefinition)
        {
            if (nextDefinition != null)
            {
                teEditor.SyntaxHighlighting = TextDefinition.GetHighlightDefinition();
            }
            else
            {
                teEditor.SyntaxHighlighting = null;
            }
        }

        /// <summary>
        /// Text document edited
        /// </summary>
        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(TextEditorControl), new PropertyMetadata(new TextDocument()));

        /// <summary>
        /// Current text definition
        /// </summary>
        public TextEditors.ITextDefinition TextDefinition
        {
            get { return (TextEditors.ITextDefinition)GetValue(TextDefinitionProperty); }
            set { SetValue(TextDefinitionProperty, value); }
        }
        public static readonly DependencyProperty TextDefinitionProperty =
            DependencyProperty.Register("TextDefinition", typeof(TextEditors.ITextDefinition), typeof(TextEditorControl), new PropertyMetadata(null, TextDefinitionPropertyChanged));
        private static void TextDefinitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextEditorControl)d).TextDefinitionChanged(e.OldValue as TextEditors.ITextDefinition, e.NewValue as TextEditors.ITextDefinition);
        }

    }
}
