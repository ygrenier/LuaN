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

namespace LuaStudio
{
    /// <summary>
    /// Logique d'interaction pour EditorControl.xaml
    /// </summary>
    public partial class EditorControl : UserControl
    {
        public EditorControl()
        {
            InitializeComponent();

            var sh = HighlightingManager.Instance.LoadHighlighter("Lua", "Lua files", new String[] { ".lua", ".wlua" });
            var sh2 = HighlightingManager.Instance.GetDefinition("Lua");
            sh2 = HighlightingManager.Instance.GetDefinition("lua");

            teEditor.SyntaxHighlighting = sh;

            teEditor.TextArea.TextEntered += TextArea_TextEntered;
            teEditor.TextArea.TextEntering += TextArea_TextEntering;
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            
        }

        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            //UpdateFolding();
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
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(EditorControl), new PropertyMetadata(new TextDocument(), TextDocumentChanged));
        private static void TextDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.NewValue);
        }
    }
}
