using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Media;

namespace LuaStudio.TextEditors
{
    public class SnippetCompletionData : ICompletionData
    {
        public SnippetCompletionData(SnippetDefinition snippet)
        {
            this.Snippet = snippet;
            this.Description = Snippet.Caption;
            this.Priority = 100;
        }

        public object Content { get { return Snippet.Word; } }

        public object Description { get; set; }

        public ImageSource Image { get; set; }

        public double Priority { get; set; }

        public string Text { get { return Snippet.Word; } }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Remove(completionSegment);
            Snippet.Insert(textArea);
        }

        /// <summary>
        /// Snippet
        /// </summary>
        public SnippetDefinition Snippet { get; private set; }
    }
}
