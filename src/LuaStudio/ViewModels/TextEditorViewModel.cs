using ICSharpCode.AvalonEdit.Document;
using LuaStudio.Resources;
using LuaStudio.TextEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels
{

    /// <summary>
    /// Base of a viewmodel for the text editor
    /// </summary>
    public class TextEditorViewModel : DocumentViewModel
    {
        static int NextId = 1;

        /// <summary>
        /// Create a new ViewModel
        /// </summary>
        public TextEditorViewModel()
        {
            TextContent = new TextDocument();
            Title = String.Format(Locales.TextEditor_NoName_Caption, NextId++);
        }

        private void TextContent_Changed(object sender, DocumentChangeEventArgs e)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Load the text
        /// </summary>
        public void Load(string filename)
        {
            TextContent = new TextDocument(File.ReadAllText(filename));
            Filename = filename;
            IsDirty = false;
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
        /// Current filename
        /// </summary>
        public String Filename
        {
            get { return _Filename; }
            protected set {
                if(SetProperty(ref _Filename, value, () => Filename))
                {
                    Title = Path.GetFileName(Filename);
                }
            }
        }
        private String _Filename;

        /// <summary>
        /// Indicates if the text content is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return _IsDirty; }
            protected set { SetProperty(ref _IsDirty, value, () => IsDirty); }
        }
        private bool _IsDirty = false;

        /// <summary>
        /// Current text definition
        /// </summary>
        public ITextDefinition TextDefinition
        {
            get { return _TextDefinition; }
            set { SetProperty(ref _TextDefinition, value, () => TextDefinition); }
        }
        private ITextDefinition _TextDefinition;

        /// <summary>
        /// Command for saving the document
        /// </summary>
        public RelayCommand SaveCommand { get; private set; }

        /// <summary>
        /// Command for saving the document as another name
        /// </summary>
        public RelayCommand SaveAsCommand { get; private set; }

        /// <summary>
        /// Command for closing the document
        /// </summary>
        public RelayCommand CloseCommand { get; set; }

    }

}
