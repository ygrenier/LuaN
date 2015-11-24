#if DEBUG
using LuaN.Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LuaN.Studio.DesignTime
{
    public class DtShellViewModel : ViewModels.IShellViewModel
    {
        public DtShellViewModel()
        {
            this.SourceDocuments = new ObservableCollection<DocumentViewModel>();
            this.Documents = new ReadOnlyObservableCollection<DocumentViewModel>(SourceDocuments);
            this.SourceTools = new ObservableCollection<IToolViewModel>();
            this.Tools = new ReadOnlyObservableCollection<IToolViewModel>(SourceTools);

            this.SourceTools.Add(new DtInteractiveLuaToolViewModel());

            this.NewScriptCommand = new RelayCommand(() => { });
        }
        protected ObservableCollection<DocumentViewModel> SourceDocuments { get; private set; }
        protected ObservableCollection<IToolViewModel> SourceTools { get; private set; }
        public ReadOnlyObservableCollection<DocumentViewModel> Documents { get; private set; }
        public ReadOnlyObservableCollection<IToolViewModel> Tools { get; private set; }
        public DocumentViewModel CurrentDocument { get; set; }
        public RelayCommand NewScriptCommand { get; private set; }
    }
}
#endif