using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaStudio.TextEditors
{
    /// <summary>
    /// Lua script definition
    /// </summary>
    [Export(typeof(ITextDefinition))]
    public class LuaDefinition : ITextDefinition
    {
        IHighlightingDefinition _HighlightingDefinition;

        /// <summary>
        /// Indicates if a file name corresponding of this definition
        /// </summary>
        public bool FileIsTypeOf(String filename)
        {
            if (String.IsNullOrWhiteSpace(filename)) return false;
            String fex = Path.GetExtension(filename);
            return Extensions.Split(',', ';').Any(e => String.Equals(e, fex, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get the highlighting definition for this texte
        /// </summary>
        /// <returns></returns>
        public IHighlightingDefinition GetHighlightDefinition()
        {
            if (_HighlightingDefinition == null)
                _HighlightingDefinition = HighlightingManager.Instance.LoadHighlighter(Name, Caption, Extensions.Split(',', ';'));
            return _HighlightingDefinition;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get { return "Lua"; } }

        /// <summary>
        /// Caption
        /// </summary>
        public string Caption { get { return Resources.Locales.TextDefinition_Lua_Caption; } }

        /// <summary>
        /// File extensions
        /// </summary>
        public string Extensions { get { return ".lua;.wlua"; } }
    }

}
