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
