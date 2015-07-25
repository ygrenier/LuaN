using ICSharpCode.AvalonEdit.Snippets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.TextEditors
{
    /// <summary>
    /// Create a new snippet definition
    /// </summary>
    public class SnippetDefinition : Snippet
    {
        /// <summary>
        /// Word linked to this snippet
        /// </summary>
        public String Word { get; set; }

        /// <summary>
        /// Snippet caption
        /// </summary>
        public String Caption { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public String Description { get; set; }
    }
}
