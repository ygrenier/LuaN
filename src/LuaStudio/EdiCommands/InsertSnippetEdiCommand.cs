using LuaStudio.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{
    class InsertSnippetEdiCommand : BaseEdiCommandDefinition
    {
        public InsertSnippetEdiCommand() : base(
            "TextEditor.Snippets.Insert", 
            Locales.EdiCommand_InsertSnippet_Caption, 
            Locales.EdiCommand_InsertSnippet_Description
            )
        {
        }
    }
}
