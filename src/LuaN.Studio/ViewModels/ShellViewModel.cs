using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels
{

    /// <summary>
    /// Main ViewModel
    /// </summary>
    public class ShellViewModel : ViewModel, IShellViewModel, IShell
    {
        private DocumentViewModel _CurrentDocument;

        /// <summary>
        /// Source of the documents list
        /// </summary>
        protected ObservableCollection<DocumentViewModel> SourceDocuments { get; private set; }

        /// <summary>
        /// Source of the tools list
        /// </summary>
        protected ObservableCollection<IToolViewModel> SourceTools { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ShellViewModel(Services.IServiceLocator services, Services.IAppService appService)
        {
            this.App = appService;
            this.SourceDocuments = new ObservableCollection<DocumentViewModel>();
            this.Documents = new ReadOnlyObservableCollection<DocumentViewModel>(SourceDocuments);
            this.SourceTools = new ObservableCollection<IToolViewModel>();
            this.Tools = new ReadOnlyObservableCollection<IToolViewModel>(SourceTools);

            //foreach (var tool in services.GetServices<IToolViewModel>())
            //{
            //    SourceTools.Add(tool);
            //}
            SourceTools.Add(new Tools.InteractiveLuaToolViewModel(App.LuaHost));

            NewScriptCommand = new RelayCommand(() => NewLuaScript());
        }

        #region Implements IShell

        /// <summary>
        /// Open a new Lua script document
        /// </summary>
        ViewModels.Documents.ILuaScriptDocumentViewModel IShell.NewLuaScript() { return this.NewLuaScript(); }

        /// <summary>
        /// Returns a document
        /// </summary>
        IDocument IShell.GetDocument(int idx) { return Documents[idx]; }

        /// <summary>
        /// Number of opened documents
        /// </summary>
        int IShell.DocumentCount { get { return Documents.Count; } }

        /// <summary>
        /// Return a tool
        /// </summary>
        ITool IShell.GetTool(int idx) { return Tools[idx]; }

        /// <summary>
        /// Find a tool by is name
        /// </summary>
        ITool IShell.FindTool(String name) { return Tools.FirstOrDefault(t => String.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)); }

        /// <summary>
        /// Nomber of tools
        /// </summary>
        int IShell.ToolCount { get { return Tools.Count; } }

        #endregion

        /// <summary>
        /// Open a new Lua script document
        /// </summary>
        public Documents.LuaScriptDocumentViewModel NewLuaScript()
        {
            var result = new Documents.LuaScriptDocumentViewModel();
            this.SourceDocuments.Add(result);
            this.CurrentDocument = result;
            return result;
        }

        /// <summary>
        /// Application
        /// </summary>
        public Services.IAppService App { get; private set; }

        /// <summary>
        /// List of opened documents
        /// </summary>
        public ReadOnlyObservableCollection<DocumentViewModel> Documents { get; }

        /// <summary>
        /// List of the tools
        /// </summary>
        public ReadOnlyObservableCollection<IToolViewModel> Tools { get; }

        /// <summary>
        /// The current active document 
        /// </summary>
        public DocumentViewModel CurrentDocument {
            get { return _CurrentDocument; }
            set { SetProperty(ref _CurrentDocument, value, () => CurrentDocument); }
        }

        /// <summary>
        /// Command for the new script action
        /// </summary>
        public RelayCommand NewScriptCommand { get; private set; }
    }

}
