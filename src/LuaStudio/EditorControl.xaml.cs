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
            }
            else
            {
                teEditor.SyntaxHighlighting = null;
            }
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

        class SecInfos
        {
            public HighlightedSection Section { get; set; }
            public String Keyword { get; set; }
            public String Caption { get; set; }
            public int LineNumber { get; set; }
        }
        FoldingManager foldingManager;
        private void UpdateFolding()
        {
            if(foldingManager==null)
                foldingManager = FoldingManager.Install(teEditor.TextArea);
            var doc = teEditor.Document;
            DocumentHighlighter dh = new DocumentHighlighter(doc, teEditor.SyntaxHighlighting);
            Stack<SecInfos> sections = new Stack<SecInfos>();
            List<NewFolding> foldings = new List<NewFolding>();
            for (int i = 1; i <= doc.LineCount; i++)
            {
                //var line = doc.GetLineByNumber(i);
                var line = dh.HighlightLine(i);
                var kws = line.Sections
                    .Where(s => s.Color != null && s.Color.Name == "Keyword");
                foreach (var kw in kws)
                {
                    var kn = doc.GetText(kw.Offset, kw.Length);
                    if (kn == "function" || kn == "while" || kn == "if" || kn == "repeat" || kn == "for")
                    {
                        sections.Push(new SecInfos() {
                            Section = kw,
                            Keyword = kn,
                            LineNumber = i
                        });
                    }
                    //else if (kn == "elseif" || kn == "else")
                    //{
                    //    if (sections.Any(s => s.Keyword == "if" || s.Keyword == "elseif"))
                    //    {
                    //        SecInfos s = null;
                    //        do { s = sections.Pop(); } while (s.Keyword != "if" && s.Keyword != "elseif");
                    //        if (s.LineNumber < i)
                    //        {
                    //            foldings.Add(new NewFolding(s.Section.Offset + s.Section.Length, kw.Offset + kw.Length));
                    //        }
                    //    }

                    //    sections.Push(new SecInfos() {
                    //        Section = kw,
                    //        Keyword = kn,
                    //        LineNumber = i
                    //    });
                    //}
                    else if (kn == "until")
                    {
                        if (sections.Any(s=>s.Keyword=="repeat"))
                        {
                            SecInfos s = null;
                            do { s = sections.Pop(); } while (s.Keyword != "repeat");
                            if (s.LineNumber < i)
                            {
                                foldings.Add(new NewFolding(s.Section.Offset + s.Section.Length, kw.Offset + kw.Length));
                            }
                        }
                    }
                    else if (kn == "end")
                    {
                        if (sections.Any(s => s.Keyword != "repeat"))
                        {
                            SecInfos s = null;
                            do { s = sections.Pop(); } while (s.Keyword == "repeat");
                            if (s.LineNumber < i)
                            {
                                foldings.Add(new NewFolding(s.Section.Offset+s.Section.Length, kw.Offset + kw.Length));
                            }
                        }
                    }
                }
                foldingManager.UpdateFoldings(foldings.OrderBy(f => f.StartOffset), -1);
                //FoldingManager.Uninstall(fManager);
            }
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
