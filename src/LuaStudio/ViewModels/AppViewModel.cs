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
        /// Open files from file selector
        /// </summary>
        public IEnumerable<DocumentViewModel> OpenFile()
        {
            var dial = AppContext.Current.GetService<Services.IDialogService>();
            var files = dial.FileOpen(Resources.Locales.OpenFileTitle, null, true, true).Result;
            return files.Select(f =>
            {
                return OpenFile(f);
            });
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
        /// List of documents
        /// </summary>
        public ObservableCollection<DocumentViewModel> Documents { get; private set; }

        /// <summary>
        /// Document actually in edition
        /// </summary>
        public DocumentViewModel CurrentDocument
        {
            get { return _CurrentDocument; }
            set { SetProperty(ref _CurrentDocument, value, () => CurrentDocument); }
        }
        private DocumentViewModel _CurrentDocument;

        /// <summary>
        /// Command to create a new editor
        /// </summary>
        public RelayCommand<TextEditors.ITextDefinition> NewEditorCommand { get; private set; }
     
        /// <summary>
        /// Command to open a new file
        /// </summary>
        public RelayCommand OpenFileCommand { get; private set; }
           
    }
}
