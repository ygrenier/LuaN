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

        /// <summary>
        /// Create a new ViewModel
        /// </summary>
        public AppViewModel()
        {
            Documents = new ObservableCollection<DocumentViewModel>();
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
            var result = new TextEditorViewModel();
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
            var result = new TextEditorViewModel();
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
        /// Document actually in edition
        /// </summary>
        public DocumentViewModel CurrentDocument
        {
            get { return _CurrentDocument; }
            set
            {
                if (_CurrentDocument != null)
                    _CurrentDocument.PropertyChanged -= CurrentDocument_PropertyChanged;
                if (SetProperty(ref _CurrentDocument, value, () => CurrentDocument))
                {
                    if (_CurrentDocument != null)
                        _CurrentDocument.PropertyChanged += CurrentDocument_PropertyChanged;
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
        /// Indicate if we can save the current document
        /// </summary>
        public bool CanSaveCurrentDocument { get { return CurrentDocument != null && CurrentDocument.CanSave; } }

        /// <summary>
        /// Indicate if we can save as a new name the current document
        /// </summary>
        public bool CanSaveAsCurrentDocument { get { return CurrentDocument != null && CurrentDocument.CanSaveAs; } }

    }
}
