using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Models
{
    /// <summary>
    /// Tool
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Unique name of the tools
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Title of the tools
        /// </summary>
        String Title { get; }
    }
}
