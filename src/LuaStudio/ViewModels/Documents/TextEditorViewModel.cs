using ICSharpCode.AvalonEdit.Document;
using LuaStudio.Resources;
using LuaStudio.Services;
using LuaStudio.TextEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels.Documents
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

        protected void Save(String filename)
        {
            using (var wrt = new StreamWriter(filename))
                TextContent.WriteTextTo(wrt);
            Filename = filename;
            IsDirty = false;
        }

        /// <summary>
        /// Save the content
        /// </summary>
        /// <returns></returns>
        public override bool Save()
        {
            if (String.IsNullOrWhiteSpace(Filename))
                return SaveAs();
            Save(Filename);
            return true;
        }

        /// <summary>
        /// Save the content as a new name
        /// </summary>
        public override bool SaveAs()
        {
            var dial = AppContext.Current.GetService<Services.IDialogService>();
            var filename = dial.FileSave(Locales.SaveFileTitle);
            if (!String.IsNullOrWhiteSpace(filename))
            {
                Save(filename);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ask to close this document
        /// </summary>
        public override bool Close()
        {
            if (IsDirty)
            {
                var dlg = AppContext.Current.GetService<Services.IDialogService>();
                var res = dlg.Confirm(
                    "Some changes are not save yet. Do you want to save it ?",
                    String.Format("{0} : Text changed", Title),
                    DialogButton.YesNoCancelButtons()
                        .IsDefault(DialogButton.YesButtonId)
                        .IsCancel(DialogButton.CancelButtonId)
                    );
                if (res != null && res.ButtonId == DialogButton.NoButtonId)
                    return true;
                else if (res != null && res.ButtonId == DialogButton.CancelButtonId)
                    return false;
                return Save();
            }
            return true;
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
                    CanSaveChanged();
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
            protected set {
                if (SetProperty(ref _IsDirty, value, () => IsDirty))
                    CanSaveChanged();
            }
        }
        private bool _IsDirty = false;

        /// <summary>
        /// Current text definition
        /// </summary>
        public ITextDefinition TextDefinition
        {
            get { return _TextDefinition; }
            set
            {
                SetProperty(ref _TextDefinition, value, () => TextDefinition);
            }
        }
        private ITextDefinition _TextDefinition;

        /// <summary>
        /// Can save when the document is dirty or filename
        /// </summary>
        public override bool CanSave { get { return IsDirty || String.IsNullOrWhiteSpace(Filename); } }

        /// <summary>
        /// Can save as new name
        /// </summary>
        public override bool CanSaveAs { get { return true; } }
    }

}
