using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels
{

    /// <summary>
    /// Base of the viewmodel for the document
    /// </summary>
    public abstract class DocumentViewModel : ViewModel, IDocument
    {
        private static int numdoc = 1;
        private bool _IsDirty;
        private string _Title;

        protected readonly int NumDocument;
        private string _FileName;

        /// <summary>
        /// New document
        /// </summary>
        public DocumentViewModel()
        {
            NumDocument = numdoc++;
            Title = "New document " + NumDocument.ToString();
            IsDirty = false;
        }

        /// <summary>
        /// Title of the document
        /// </summary>
        public virtual String Title {
            get { return _Title; }
            protected set { SetProperty(ref _Title, value, () => Title); }
        }

        /// <summary>
        /// Current file name
        /// </summary>
        public virtual String FileName {
            get { return _FileName; }
            protected set { SetProperty(ref _FileName, value, () => FileName); }
        }

        /// <summary>
        /// Indicates if the document have changes
        /// </summary>
        public virtual bool IsDirty {
            get { return this._IsDirty; }
            protected set { this.SetProperty(ref _IsDirty, value, () => IsDirty); }
        }

    }

}
