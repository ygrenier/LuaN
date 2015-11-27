using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Highlighting;

namespace LuaN.Studio.TextEditors
{
    /// <summary>
    /// Text definition for Lua
    /// </summary>
    public class LuaTextDefinition : ITextDefinition
    {
        IHighlightingDefinition _HighlightingDefinition;

        /// <summary>
        /// Get the highlight definition
        /// </summary>
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
        public string Caption { get { return Locales.SR.TextDefinition_Lua_Caption; } }

        /// <summary>
        /// File extensions
        /// </summary>
        public string Extensions { get { return ".lua;.wlua"; } }

    }
}
