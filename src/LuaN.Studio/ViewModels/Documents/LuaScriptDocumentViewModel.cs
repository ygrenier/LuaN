using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaN.Studio.ViewModels.Documents
{
    /// <summary>
    /// Lua script document viewmodel
    /// </summary>
    public class LuaScriptDocumentViewModel : TextEditorDocumentViewModel, ILuaScriptDocumentViewModel
    {
        private String _FileName;

        /// <summary>
        /// New script
        /// </summary>
        public LuaScriptDocumentViewModel()
        {
            this.Title = "NewScript" + NumDocument.ToString();
            this._FileName = null;
        }

        /// <summary>
        /// Current file name
        /// </summary>
        public override String FileName
        {
            get { return _FileName; }
            protected set {
                if(SetProperty(ref _FileName, value, () => FileName))
                {
                    String fn = Path.GetFileName(this.FileName);
                    if (!String.IsNullOrWhiteSpace(fn))
                        this.Title = fn;
                }
            }
        }

    }
}
