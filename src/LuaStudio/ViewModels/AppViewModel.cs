using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
                      OpenNewEditorCommand(d);
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
        public DocumentViewModel OpenNewEditorCommand(TextEditors.ITextDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            var result = new TextEditorViewModel();
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
        
    }
}
