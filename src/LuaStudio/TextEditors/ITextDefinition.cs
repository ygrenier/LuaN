using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.TextEditors
{

    /// <summary>
    /// Definition for an edition text
    /// </summary>
    public interface ITextDefinition
    {
        /// <summary>
        /// Indicates if a file name corresponding of this definition
        /// </summary>
        bool FileIsTypeOf(String filename);
        /// <summary>
        /// Get the highlighting definition for this text
        /// </summary>
        /// <returns></returns>
        IHighlightingDefinition GetHighlightDefinition();
        /// <summary>
        /// Get the folding strategy for this text
        /// </summary>
        ITextFoldingStrategy GetFoldingStrategy();
        /// <summary>
        /// List all snippets registered
        /// </summary>
        IEnumerable<SnippetDefinition> GetSnippets();
        /// <summary>
        /// Find a snippet for a word
        /// </summary>
        /// <returns></returns>
        SnippetDefinition FindSnippet(String word);
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
