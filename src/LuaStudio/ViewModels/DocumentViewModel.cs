using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels
{

    /// <summary>
    /// Base viewmodel for the document
    /// </summary>
    public abstract class DocumentViewModel: ViewModel
    {

        /// <summary>
        /// Save the document
        /// </summary>
        public virtual bool Save() { return false; }

        /// <summary>
        /// Save the document as new file
        /// </summary>
        public virtual bool SaveAs() { return false; }

        /// <summary>
        /// Raise property changed events for CanSave and CanSaveAs
        /// </summary>
        public virtual void CanSaveChanged()
        {
            RaisePropertyChanged(() => CanSave);
            RaisePropertyChanged(() => CanSaveAs);
        }

        /// <summary>
        /// Document title
        /// </summary>
        public String Title
        {
            get { return _Title; }
            protected set { SetProperty(ref _Title, value, () => Title); }
        }
        private String _Title;

        /// <summary>
        /// Indicate if this document can be saved
        /// </summary>
        public virtual bool CanSave { get { return false; } }

        /// <summary>
        /// Indicate if this document can be saved as a new name
        /// </summary>
        public virtual bool CanSaveAs { get { return false; } }

    }

}
