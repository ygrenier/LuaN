using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaStudio.EdiCommands;

namespace LuaStudio.ViewModels
{

    /// <summary>
    /// Base viewmodel for the document
    /// </summary>
    public abstract class DocumentViewModel: DockContentViewModel
    {
        /// <summary>
        /// New document view model
        /// </summary>
        public DocumentViewModel()
        {
            CloseDocumentCommand = new RelayCommand(
                () =>
                {
                    if (AppViewModel != null)
                    {
                        AppViewModel.CloseDocument(this);
                    }
                    else
                    {
                        Close();
                    }
                },
                () => CanClose
                );
        }

        /// <summary>
        /// Call for accept an edi command
        /// </summary>
        protected virtual bool AcceptEdiCommand(EdiCommand command)
        {
            return true;
        }

        /// <summary>
        /// Create a new EdiCommandListener
        /// </summary>
        /// <returns></returns>
        protected virtual EdiCommandListener CreateEdiCommandListener()
        {
            return new EdiCommands.EdiCommandListener(AcceptEdiCommand);
        }

        /// <summary>
        /// Invoke an EDI command
        /// </summary>
        public virtual bool InvokeEdiCommand(EdiCommands.EdiCommand command)
        {
            if (!EdiCommandListener.AcceptCommand(command))
                return false;
            return EdiCommandListener.ReceiveCommand(command);
        }

        /// <summary>
        /// Save the document
        /// </summary>
        public virtual bool Save() { return false; }

        /// <summary>
        /// Save the document as new file
        /// </summary>
        public virtual bool SaveAs() { return false; }

        /// <summary>
        /// Ask for close
        /// </summary>
        public virtual bool Close()
        {
            return true;
        }

        /// <summary>
        /// Raise property changed events for CanSave and CanSaveAs
        /// </summary>
        public virtual void CanSaveChanged()
        {
            RaisePropertyChanged(() => CanSave);
            RaisePropertyChanged(() => CanSaveAs);
        }

        /// <summary>
        /// AppViewModel
        /// </summary>
        public AppViewModel AppViewModel
        {
            get { return _AppViewModel; }
            internal set { SetProperty(ref _AppViewModel, value, () => AppViewModel); }
        }
        private AppViewModel _AppViewModel;

        /// <summary>
        /// Indicate if this document can be saved
        /// </summary>
        public virtual bool CanSave { get { return false; } }

        /// <summary>
        /// Indicate if this document can be saved as a new name
        /// </summary>
        public virtual bool CanSaveAs { get { return false; } }

        /// <summary>
        /// Indicate if thhis document can be require for closing
        /// </summary>
        public virtual bool CanClose { get { return true; } }

        /// <summary>
        /// Command to close this document
        /// </summary>
        public RelayCommand CloseDocumentCommand { get; private set; }

        /// <summary>
        /// EDI command listener
        /// </summary>
        public EdiCommandListener EdiCommandListener
        {
            get { return _EdiCommandListener ?? (_EdiCommandListener = CreateEdiCommandListener()); }
        }

        private EdiCommands.EdiCommandListener _EdiCommandListener;
    }

}
