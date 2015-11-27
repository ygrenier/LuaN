using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.TextEditors
{
    /// <summary>
    /// Text type definition
    /// </summary>
    public interface ITextDefinition
    {
        /// <summary>
        /// Get the highlighting definition for this text
        /// </summary>
        /// <returns></returns>
        IHighlightingDefinition GetHighlightDefinition();

        /// <summary>
        /// Name of the definition
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Capion of the definition
        /// </summary>
        String Caption { get; }
        /// <summary>
        /// File extensions
        /// </summary>
        String Extensions { get; }
    }
}
