using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Models
{
    /// <summary>
    /// Document
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// Title of the document
        /// </summary>
        String Title { get; }

        /// <summary>
        /// Current file name
        /// </summary>
        String FileName { get; }

        /// <summary>
        /// Indicates if the document have changes
        /// </summary>
        bool IsDirty { get; }
    }
}
