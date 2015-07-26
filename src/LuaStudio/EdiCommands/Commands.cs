using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{
    /// <summary>
    /// Standard commands
    /// </summary>
    public static class Commands
    {
        public static IEdiCommandDefinition InsertSnippet = new InsertSnippetEdiCommand();
    }
}
