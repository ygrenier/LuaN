using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels.Documents
{

    /// <summary>
    /// Document for text editor
    /// </summary>
    public class TextEditorDocumentViewModel : DocumentViewModel
    {
        private static int numdoc = 1;

        protected readonly int NumDocument;
        private string _FileName;

        /// <summary>
        /// New text editor
        /// </summary>
        public TextEditorDocumentViewModel()
        {
            NumDocument = numdoc++;
            Title = "New document " + NumDocument.ToString();
            IsDirty = false;
            TextContent = new TextDocument();
        }

        private void TextContent_Changed(object sender, DocumentChangeEventArgs e)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Current file name
        /// </summary>
        public virtual String FileName
        {
            get { return _FileName; }
            protected set { SetProperty(ref _FileName, value, () => FileName); }
        }

        /// <summary>
        /// Text content of the document
        /// </summary>
        public TextDocument TextContent
        {
            get { return _TextContent; }
            set
            {
                if (_TextContent != value)
                {
                    if (_TextContent != null)
                        _TextContent.Changed -= TextContent_Changed;
                    SetProperty(ref _TextContent, value ?? new TextDocument(), () => TextContent);
                    if (_TextContent != null)
                        _TextContent.Changed += TextContent_Changed;
                }
            }
        }
        private TextDocument _TextContent;

        /// <summary>
        /// The text definition for the current document
        /// </summary>
        public TextEditors.ITextDefinition TextDefinition
        {
            get { return _TextDefinition; }
            set { SetProperty(ref _TextDefinition, value, () => TextDefinition); }
        }
        private TextEditors.ITextDefinition _TextDefinition;

    }

}
