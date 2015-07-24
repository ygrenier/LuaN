using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace LuaStudio.TextEditors
{
    /// <summary>
    /// Lua script definition
    /// </summary>
    [Export(typeof(ITextDefinition))]
    public class LuaDefinition : ITextDefinition
    {

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get { return "Lua"; } }

        /// <summary>
        /// Caption
        /// </summary>
        public string Caption { get { return Resources.Locales.TextDefinition_Lua_Caption; } }

        /// <summary>
        /// File extensions
        /// </summary>
        public string Extensions { get { return ".lua;.wlua"; } }
    }
}
