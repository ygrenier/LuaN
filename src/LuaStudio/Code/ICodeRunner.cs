using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaStudio.Code
{

    /// <summary>
    /// Executor interface
    /// </summary>
    public interface ICodeRunner
    {
        /// <summary>
        /// Run
        /// </summary>
        int Run(TextReader sourceReader, string sourceFile);
    }

}
