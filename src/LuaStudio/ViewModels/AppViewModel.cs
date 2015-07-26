using LuaStudio.EdiCommands;
using LuaStudio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LuaStudio.ViewModels
{
    /// <summary>
    /// ViewModel for the application
    /// </summary>
    public class AppViewModel : ViewModel
    {
        ObservableCollection<TextEditors.ITextDefinition> _TextDefinitions = null;
        Services.IMessengerService Messenger;
        IMessageSubscription<EdiCommandMessage> _EdiCommandSub;

        /// <summary>
        /// Create a new ViewModel
        /// </summary>
        public AppViewModel()
        {
            Documents = new ObservableCollection<DocumentViewModel>();
            Documents.CollectionChanged += Documents_CollectionChanged;
            Tools = new ObservableCollection<ToolViewModel>();
            Tools.Add(new ViewModels.Tools.SnippetsToolViewModel());
            NewEditorCommand = new RelayCommand<TextEditors.ITextDefinition>(d =>
              {
                  d = d ?? TextDefinitions.FirstOrDefault();
                  if (d != null)
                      OpenNewEditor(d);
              });
            OpenFileCommand = new RelayCommand(() =>
              {
                  OpenFile();
              });
            SaveFileCommand = new RelayCommand(
                () => SaveCurrentDocument(),
                () => CanSaveCurrentDocument
                );
            SaveAsFileCommand = new RelayCommand(
                () => SaveAsCurrentDocument(),
                () => CanSaveAsCurrentDocument
                );
            SaveAllFilesCommand = new RelayCommand(
                () => SaveAllDocuments(),
                () => Documents.Count > 0
                );
            CloseDocumentCommand = new RelayCommand<DocumentViewModel>(
                doc => CloseDocument(doc),
                doc => doc != null && doc.CanClose
                );
            CloseCurrentDocumentCommand = new RelayCommand(
                () => CloseDocument(CurrentDocument),
                () => CanCloseCurrentDocument
                );
            CloseAllDocumentsCommand = new RelayCommand(() => CloseAllDocuments());
            RunCurrentDocumentCommand = new RelayCommand(
                () => RunDocument(CurrentDocument),
                () => CanRunCurrentDocument
                );
            Messenger = AppContext.Current.Messenger;
            _EdiCommandSub = Messenger.Subscribe<EdiCommandMessage>(OnEdiCommand);
        }

        private void OnEdiCommand(EdiCommandMessage message)
        {
            if (message == null) return;
            var ediCommand = message.ToEdiCommand();

            // Check if the command is for the EDI

            // Check the command for the current document
            if (CurrentDocument != null)
            {
                if (CurrentDocument.InvokeEdiCommand(ediCommand))
                    return;
            }
        }

        private void Documents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems!=null)
            {
                foreach (DocumentViewModel document in e.NewItems)
                {
                    document.AppViewModel = this;
                    Messenger.SendMessage(new DocumentNotifyMessage
                    {
                        Sender = this,
                        Document = document,
                        Notification = DocumentNotification.Added
                    });
                }
            }
            if (e.OldItems != null)
            {
                foreach (DocumentViewModel document in e.OldItems)
                {
                    document.AppViewModel = this;
                    Messenger.SendMessage(new DocumentNotifyMessage
                    {
                        Sender = this,
                        Document = document,
                        Notification = DocumentNotification.Removed
                    });
                }
            }
        }

        /// <summary>
        /// Open files from file selector
        /// </summary>
        public IEnumerable<DocumentViewModel> OpenFile()
        {
            var dial = AppContext.Current.GetService<Services.IDialogService>();
            var files = dial.FileOpen(Resources.Locales.OpenFileTitle, null, true, true).Result;
            return files.Select(f =>
            {
                return OpenFile(f);
            }).ToList();
        }

        /// <summary>
        /// Open file from a name
        /// </summary>
        public DocumentViewModel OpenFile(String filename)
        {
            var tdef = AppContext.Current.GetTextDefinitions().FirstOrDefault(td => td.FileIsTypeOf(filename));
            var result = new Documents.TextEditorViewModel();
            result.TextDefinition = tdef;
            result.Load(filename);
            Documents.Add(result);
            CurrentDocument = result;
            return result;
        }

        /// <summary>
        /// Save the current document
        /// </summary>
        public bool SaveCurrentDocument()
        {
            return CanSaveCurrentDocument && CurrentDocument.Save();
        }

        /// <summary>
        /// Save as a new name the current document
        /// </summary>
        public bool SaveAsCurrentDocument()
        {
            return CanSaveAsCurrentDocument && CurrentDocument.SaveAs();
        }

        /// <summary>
        /// Save all documents
        /// </summary>
        public void SaveAllDocuments()
        {
            foreach (var doc in Documents)
            {
                if (doc.CanSave) doc.Save();
            }
        }

        /// <summary>
        /// Close a document
        /// </summary>
        public bool CloseDocument(DocumentViewModel document)
        {
            if (document == null) return false;
            var res = document.CanClose && document.Close();
            if (res)
            {
                var idx = Documents.IndexOf(document);
                bool isCurrent = CurrentDocument == document;
                Documents.Remove(document);
                if (isCurrent)
                {
                    idx = Math.Min(idx, Documents.Count - 1);
                    if (idx < 0)
                    {
                        CurrentDocument = null;
                    }
                    else
                    {
                        CurrentDocument = Documents[idx];
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Close all documents
        /// </summary>
        /// <returns></returns>
        public bool CloseAllDocuments()
        {
            foreach (var doc in Documents.Reverse().ToArray())
            {
                if (!CloseDocument(doc)) return false;
            }
            return true;
        }

        /// <summary>
        /// Run a document
        /// </summary>
        public Task<int> RunDocument(DocumentViewModel document)
        {
            InRunMode = true;
            return Task.Factory.StartNew<int>(() =>
            {
                var tdoc = document as ViewModels.Documents.TextEditorViewModel;
                if (tdoc == null || tdoc.TextDefinition == null) return -1;
                var runner = tdoc.TextDefinition.GetCodeRunner();
                return runner.Run(tdoc.TextContent.CreateReader(), tdoc.Filename);
            }).ContinueWith(_ =>
            {
                InRunMode = false;
                return _.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Text editor Definitions
        /// </summary>
        public ObservableCollection<TextEditors.ITextDefinition> TextDefinitions { get
            {
                if (_TextDefinitions == null)
                {
                    _TextDefinitions = new ObservableCollection<TextEditors.ITextDefinition>();
                    foreach (var tdef in AppContext.Current.GetTextDefinitions())
                        _TextDefinitions.Add(tdef);
                }
                return _TextDefinitions;
            }
        }

        /// <summary>
        /// Open a new text editor
        /// </summary>
        public DocumentViewModel OpenNewEditor(TextEditors.ITextDefinition definition)
        {
            var result = new Documents.TextEditorViewModel();
            result.TextDefinition = definition;
            Documents.Add(result);
            CurrentDocument = result;
            return result;
        }

        /// <summary>
        /// List of documents
        /// </summary>
        public ObservableCollection<DocumentViewModel> Documents { get; private set; }

        /// <summary>
        /// List of tools
        /// </summary>
        public ObservableCollection<ToolViewModel> Tools { get; private set; }

        /// <summary>
        /// Active content
        /// </summary>
        public DockContentViewModel ActiveContent
        {
            get { return _ActiveContent; }
            set {
                if (SetProperty(ref _ActiveContent, value, () => ActiveContent))
                {
                    if (ActiveContent == null)
                    {
                        CurrentDocument = null;
                    }
                    else if (!(ActiveContent is ToolViewModel))
                    {
                        CurrentDocument = ActiveContent as DocumentViewModel;
                    }
                }
            }
        }
        private DockContentViewModel _ActiveContent;

        /// <summary>
        /// Document actually in edition
        /// </summary>
        public DocumentViewModel CurrentDocument
        {
            get { return _CurrentDocument; }
            set
            {
                if (_CurrentDocument != value)
                {
                    if (_CurrentDocument != null)
                        _CurrentDocument.PropertyChanged -= CurrentDocument_PropertyChanged;
                    _CurrentDocument = value;
                    if (_CurrentDocument != null)
                        _CurrentDocument.PropertyChanged += CurrentDocument_PropertyChanged;
                    RaisePropertyChanged(() => CurrentDocument);
                    RaisePropertyChanged(() => CanSaveCurrentDocument);
                    RaisePropertyChanged(() => CanSaveAsCurrentDocument);
                    RaisePropertyChanged(() => CanCloseCurrentDocument);
                    RaisePropertyChanged(() => CanRunCurrentDocument);
                    Messenger.SendMessage(new DocumentNotifyMessage
                    {
                        Sender = this,
                        Document = value,
                        Notification = DocumentNotification.IsActive
                    });
                    ActiveContent = value;
                }
            }
        }
        private DocumentViewModel _CurrentDocument;

        private void CurrentDocument_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (String.Equals(e.PropertyName, "CanSave", StringComparison.OrdinalIgnoreCase))
                RaisePropertyChanged(() => CanSaveCurrentDocument);
            else if (String.Equals(e.PropertyName, "CanSaveAs", StringComparison.OrdinalIgnoreCase))
                RaisePropertyChanged(() => CanSaveAsCurrentDocument);
            if (String.Equals(e.PropertyName, "CanClose", StringComparison.OrdinalIgnoreCase))
                RaisePropertyChanged(() => CanCloseCurrentDocument);
            if (String.Equals(e.PropertyName, "CanRun", StringComparison.OrdinalIgnoreCase))
                RaisePropertyChanged(() => CanRunCurrentDocument);
        }

        /// <summary>
        /// Command to create a new editor
        /// </summary>
        public RelayCommand<TextEditors.ITextDefinition> NewEditorCommand { get; private set; }
     
        /// <summary>
        /// Command to open a new file
        /// </summary>
        public RelayCommand OpenFileCommand { get; private set; }

        /// <summary>
        /// Command to save the current document
        /// </summary>
        public RelayCommand SaveFileCommand { get; private set; }

        /// <summary>
        /// Command to save as a new name the current document
        /// </summary>
        public RelayCommand SaveAsFileCommand { get; private set; }

        /// <summary>
        /// Command to save all files
        /// </summary>
        public RelayCommand SaveAllFilesCommand { get; private set; }

        /// <summary>
        /// Command to close a document
        /// </summary>
        public RelayCommand<DocumentViewModel> CloseDocumentCommand { get; private set; }

        /// <summary>
        /// Command to close the current document
        /// </summary>
        public RelayCommand CloseCurrentDocumentCommand { get; private set; }

        /// <summary>
        /// Command to close all documents
        /// </summary>
        public RelayCommand CloseAllDocumentsCommand { get; private set; }

        /// <summary>
        /// Indicate if we can save the current document
        /// </summary>
        public bool CanSaveCurrentDocument { get { return CurrentDocument != null && CurrentDocument.CanSave; } }

        /// <summary>
        /// Indicate if we can save as a new name the current document
        /// </summary>
        public bool CanSaveAsCurrentDocument { get { return CurrentDocument != null && CurrentDocument.CanSaveAs; } }

        /// <summary>
        /// Indicate if we can close the current document
        /// </summary>
        public bool CanCloseCurrentDocument { get { return CurrentDocument != null && CurrentDocument.CanClose; } }

        /// <summary>
        /// Command to run the courrent document
        /// </summary>
        public RelayCommand RunCurrentDocumentCommand { get; private set; }
        
        /// <summary>
        /// Indicate if wa can run the current document
        /// </summary>
        public bool CanRunCurrentDocument { get {
                var tdoc = CurrentDocument as ViewModels.Documents.TextEditorViewModel;
                return tdoc != null && tdoc.TextDefinition != null && tdoc.TextDefinition.GetCodeRunner() != null;
            }
        }

        /// <summary>
        /// Indicate the EDI is in run mode
        /// </summary>
        public bool InRunMode
        {
            get { return _InRunMode; }
            protected set { SetProperty(ref _InRunMode, value, () => InRunMode); }
        }
        private bool _InRunMode = false;

    }

    public class DocumentNotifyMessage : Message
    {
        public DocumentViewModel Document { get; set; }
        public DocumentNotification Notification { get; set; }
    }
    public enum DocumentNotification
    {
        Added,
        IsActive,
        Removed
    }
}
