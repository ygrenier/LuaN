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
    /// <summary>
    /// Completion data
    /// </summary>
    public class KeywordCompletionData : ICompletionData
    {
        public KeywordCompletionData(String keyword)
        {
            this.Text = keyword;
            this.Description = keyword;
            this.Priority = 50;
        }

        public object Content { get { return Text; } }

        public object Description { get; set; }

        public ImageSource Image { get; set; }

        public double Priority { get; set; }

        public string Text { get; private set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
