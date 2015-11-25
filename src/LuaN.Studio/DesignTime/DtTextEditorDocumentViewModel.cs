#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.DesignTime
{
    public class DtTextEditorDocumentViewModel : ViewModels.Documents.TextEditorDocumentViewModel
    {
        public DtTextEditorDocumentViewModel()
        {
            this.TextContent = new ICSharpCode.AvalonEdit.Document.TextDocument();
            this.TextContent.Text = @"
-- Lua content for design

function write(a)
 print(a)
end

b = 2

write(""b = "" .. b)

";
        }
    }
}
#endif