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
        }

        #region Implements IShell

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

    }

}
