using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;

namespace LuaStudio.TextEditors
{
    /// <summary>
    /// Lua script definition
    /// </summary>
    [Export(typeof(ITextDefinition))]
    public class LuaDefinition : ITextDefinition, ITextFoldingStrategy
    {
        IHighlightingDefinition _HighlightingDefinition;

        class SecInfos
        {
            public HighlightedSection Section { get; set; }
            public String Keyword { get; set; }
            public String Caption { get; set; }
            public int LineNumber { get; set; }
        }

        IEnumerable<NewFolding> ITextFoldingStrategy.BuildFoldings(FoldingManager foldingManager, TextDocument document)
        {
            DocumentHighlighter dh = new DocumentHighlighter(document, GetHighlightDefinition());
            Stack<SecInfos> sections = new Stack<SecInfos>();
            for (int i = 1; i <= document.LineCount; i++)
            {
                //var line = document.GetLineByNumber(i);
                var line = dh.HighlightLine(i);
                var kws = line.Sections
                    .Where(s => s.Color != null && s.Color.Name == "Keyword");
                foreach (var kw in kws)
                {
                    var kn = document.GetText(kw.Offset, kw.Length);
                    if (kn == "function" || kn == "while" || kn == "if" || kn == "repeat" || kn == "for")
                    {
                        sections.Push(new SecInfos()
                        {
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
                        if (sections.Any(s => s.Keyword == "repeat"))
                        {
                            SecInfos s = null;
                            do { s = sections.Pop(); } while (s.Keyword != "repeat");
                            if (s.LineNumber < i)
                            {
                                yield return new NewFolding(s.Section.Offset + s.Section.Length, kw.Offset + kw.Length);
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
                                yield return new NewFolding(s.Section.Offset + s.Section.Length, kw.Offset + kw.Length);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if a file name corresponding of this definition
        /// </summary>
        public bool FileIsTypeOf(String filename)
        {
            if (String.IsNullOrWhiteSpace(filename)) return false;
            String fex = Path.GetExtension(filename);
            return Extensions.Split(',', ';').Any(e => String.Equals(e, fex, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get the highlighting definition for this texte
        /// </summary>
        /// <returns></returns>
        public IHighlightingDefinition GetHighlightDefinition()
        {
            if (_HighlightingDefinition == null)
                _HighlightingDefinition = HighlightingManager.Instance.LoadHighlighter(Name, Caption, Extensions.Split(',', ';'));
            return _HighlightingDefinition;
        }

        /// <summary>
        /// Get the folding strategy for this text
        /// </summary>
        public ITextFoldingStrategy GetFoldingStrategy() { return this; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get { return "Lua"; } }

        /// <summary>
        /// Caption
        /// </summary>
        public string Caption { get { return Resources.Locales.TextDefinition_Lua_Caption; } }

        /// <summary>
        /// File extensions
        /// </summary>
        public string Extensions { get { return ".lua;.wlua"; } }
    }

}
