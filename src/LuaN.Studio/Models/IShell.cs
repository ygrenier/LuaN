using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Models
{

    /// <summary>
    /// Shell interface
    /// </summary>
    public interface IShell
    {

        /// <summary>
        /// Returns a document
        /// </summary>
        IDocument GetDocument(int idx);

        /// <summary>
        /// Number of opened documents
        /// </summary>
        int DocumentCount { get; }

        /// <summary>
        /// Return a tool
        /// </summary>
        ITool GetTool(int idx);

        /// <summary>
        /// Find a tool by is name
        /// </summary>
        ITool FindTool(String name);

        /// <summary>
        /// Nomber of tools
        /// </summary>
        int ToolCount { get; }

    }

}
