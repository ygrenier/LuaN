using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.TextEditors
{
    /// <summary>
    /// Folding strategy
    /// </summary>
    public interface ITextFoldingStrategy
    {
        /// <summary>
        /// Build flodings
        /// </summary>
        IEnumerable<NewFolding> BuildFoldings(FoldingManager foldingManager, TextDocument document);
    }
}
