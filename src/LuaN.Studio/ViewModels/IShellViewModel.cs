using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels
{
    /// <summary>
    /// Shell ViewModel
    /// </summary>
    public interface IShellViewModel
    {

        /// <summary>
        /// List of opened documents
        /// </summary>
        ReadOnlyObservableCollection<DocumentViewModel> Documents { get; }

        /// <summary>
        /// List of the tools
        /// </summary>
        ReadOnlyObservableCollection<IToolViewModel> Tools { get; }
    }
}
