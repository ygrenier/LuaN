using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LuaStudio
{
    public static class EditorExtensions
    {
        public static IHighlightingDefinition LoadHighlighter(this HighlightingManager manager, String name, String caption, IEnumerable<String> extensions)
        {
            IHighlightingDefinition result = null;
            using (Stream s = typeof(EditorExtensions).Assembly.GetManifestResourceStream(String.Format("LuaStudio.Syntax.{0}.xshd", name)))
            {
                if (s != null)
                {
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        result = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                            HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
            if (result != null)
            {
                manager.RegisterHighlighting(caption, extensions.ToArray(), result);
            }
            return result;
        }
    }
}
