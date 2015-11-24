using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels.Documents
{
    /// <summary>
    /// Lua script document viewmodel
    /// </summary>
    public class LuaScriptDocumentViewModel : DocumentViewModel, ILuaScriptDocumentViewModel
    {
        /// <summary>
        /// New script
        /// </summary>
        public LuaScriptDocumentViewModel()
        {
            this.Title = "NewScript" + NumDocument.ToString();
        }
    }
}
