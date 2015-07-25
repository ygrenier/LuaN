using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Snippets;

namespace LuaStudio.TextEditors
{
    /// <summary>
    /// Lua script definition
    /// </summary>
    [Export(typeof(ITextDefinition))]
    public class LuaDefinition : ITextDefinition, ITextFoldingStrategy
    {
        IHighlightingDefinition _HighlightingDefinition;
        List<SnippetDefinition> _Snippets;

        #region Folding management
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
        #endregion

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
        /// List all snippets registered
        /// </summary>
        public IEnumerable<SnippetDefinition> GetSnippets()
        {
            if (_Snippets == null)
            {
                _Snippets = new List<SnippetDefinition>();
                //var loopCounter = new SnippetReplaceableTextElement { Text = "i" };
                //Snippet snippet = new Snippet
                //{
                //    Elements = {
                //                 new SnippetTextElement { Text = "for(int " },
                //                 new SnippetBoundElement { TargetElement = loopCounter },
                //                 new SnippetTextElement { Text = " = " },
                //                 new SnippetReplaceableTextElement { Text = "0" },
                //                 new SnippetTextElement { Text = "; " },
                //                 loopCounter,
                //                 new SnippetTextElement { Text = " < " },
                //                 new SnippetReplaceableTextElement { Text = "end" },
                //                 new SnippetTextElement { Text = "; " },
                //                 new SnippetBoundElement { TargetElement = loopCounter },
                //                 new SnippetTextElement { Text = "++) { \n" },
                //                 new SnippetCaretElement(),
                //                 new SnippetTextElement { Text = "\n }" }
                //             }
                //};
                SnippetDefinition snippet = new SnippetDefinition
                {
                    Word = "while",
                    Caption = "While loop",
                    Description = "Insert a while loop",
                    Elements = {
                        new SnippetTextElement {Text="while " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" do \n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" },
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "repeat",
                    Caption = "Repeat loop",
                    Description = "Insert a repeat loop",
                    Elements = {
                        new SnippetTextElement {Text="repeat\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nuntil " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text="\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "if",
                    Caption = "If statement",
                    Description = "Insert a simple if statement",
                    Elements = {
                        new SnippetTextElement {Text="if " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" then\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "ifelse",
                    Caption = "If Else statement",
                    Description = "Insert an if-else-end statement",
                    Elements = {
                        new SnippetTextElement {Text="if " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" then\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nelse\n\t" },
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "ifelseif",
                    Caption = "If ElseIf statement",
                    Description = "Insert an if-elseif-end statement",
                    Elements = {
                        new SnippetTextElement {Text="if " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" then\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nelseif " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" then\n\t" },
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "ifelseifelse",
                    Caption = "If ElseIf Else statement",
                    Description = "Insert an if-elseif-else-end statement",
                    Elements = {
                        new SnippetTextElement {Text="if " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" then\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nelseif " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" then\n\t" },
                        new SnippetTextElement {Text="\nelse\n\t" },
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "for",
                    Caption = "For loop",
                    Description = "Insert for 'a' to 'b' loop",
                    Elements = {
                        new SnippetTextElement {Text="for " },
                        new SnippetReplaceableTextElement { Text = "i" },
                        new SnippetTextElement {Text=" = " },
                        new SnippetReplaceableTextElement { Text = "start" },
                        new SnippetTextElement {Text=" , " },
                        new SnippetReplaceableTextElement { Text = "end" },
                        new SnippetTextElement {Text=" do\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "fors",
                    Caption = "For loop with step",
                    Description = "Insert for 'a' to 'b' by 'step' loop",
                    Elements = {
                        new SnippetTextElement {Text="for " },
                        new SnippetReplaceableTextElement { Text = "i" },
                        new SnippetTextElement {Text=" = " },
                        new SnippetReplaceableTextElement { Text = "start" },
                        new SnippetTextElement {Text=" , " },
                        new SnippetReplaceableTextElement { Text = "end" },
                        new SnippetTextElement {Text=" , " },
                        new SnippetReplaceableTextElement { Text = "step" },
                        new SnippetTextElement {Text=" do\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "forin",
                    Caption = "ForIn loop",
                    Description = "Insert for-in loop",
                    Elements = {
                        new SnippetTextElement {Text="for " },
                        new SnippetReplaceableTextElement { Text = "i" },
                        new SnippetTextElement {Text=" in " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" do\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "func",
                    Caption = "Define a function",
                    Description = "Insert a function definition with parameters",
                    Elements = {
                        new SnippetTextElement {Text="function " },
                        new SnippetReplaceableTextElement { Text = "name" },
                        new SnippetTextElement {Text=" (" },
                        new SnippetReplaceableTextElement { Text = "params" },
                        new SnippetTextElement {Text=")\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "funce",
                    Caption = "Define a function without parameters",
                    Description = "Insert a function definition without parameters",
                    Elements = {
                        new SnippetTextElement {Text="function " },
                        new SnippetReplaceableTextElement { Text = "name" },
                        new SnippetTextElement {Text="\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "do",
                    Caption = "Code block",
                    Description = "Insert a do-end block",
                    Elements = {
                        new SnippetTextElement {Text="do\n\t" },
                        new SnippetCaretElement(),
                        new SnippetTextElement {Text="\nend\n" }
                    }
                };
                _Snippets.Add(snippet);
                snippet = new SnippetDefinition
                {
                    Word = "local",
                    Caption = "Local variable",
                    Description = "Insert a local statement",
                    Elements = {
                        new SnippetTextElement {Text="local " },
                        new SnippetReplaceableTextElement { Text = "var" },
                        new SnippetTextElement {Text=" = " },
                        new SnippetReplaceableTextElement { Text = "exp" },
                        new SnippetTextElement {Text=" " },
                        new SnippetCaretElement(),
                    }
                };
                _Snippets.Add(snippet);

            }
            return _Snippets;
        }

        /// <summary>
        /// Find a snippet for a word
        /// </summary>
        /// <returns></returns>
        public SnippetDefinition FindSnippet(String word)
        {
            return GetSnippets().FirstOrDefault(s => String.Equals(s.Word, word, StringComparison.Ordinal));
        }

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
