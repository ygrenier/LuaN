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
        /// Document title
        /// </summary>
        public String Title
        {
            get { return _Title; }
            protected set { SetProperty(ref _Title, value, () => Title); }
        }
        private String _Title;

    }

}
