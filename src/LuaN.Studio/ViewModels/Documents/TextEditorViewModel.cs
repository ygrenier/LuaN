using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels.Documents
{

    /// <summary>
    /// Document for text editor
    /// </summary>
    public class TextEditorViewModel : DocumentViewModel
    {
        private static int numdoc = 1;

        protected readonly int NumDocument;
        private string _FileName;

        /// <summary>
        /// New text editor
        /// </summary>
        public TextEditorViewModel()
        {
            NumDocument = numdoc++;
            Title = "New document " + NumDocument.ToString();
            IsDirty = false;
        }

        /// <summary>
        /// Current file name
        /// </summary>
        public virtual String FileName
        {
            get { return _FileName; }
            protected set { SetProperty(ref _FileName, value, () => FileName); }
        }

    }

}
