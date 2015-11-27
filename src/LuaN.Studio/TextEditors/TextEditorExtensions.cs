using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LuaN.Studio.TextEditors
{
    /// <summary>
    /// Some helpers to manage the text definitions
    /// </summary>
    public static class TextEditorExtensions
    {
        /// <summary>
        /// Load an highlighting definition from the resources
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static IHighlightingDefinition LoadHighlighter(this HighlightingManager manager, String name, String caption, IEnumerable<String> extensions)
        {
            IHighlightingDefinition result = null;
            using (Stream s = typeof(TextEditorExtensions).Assembly.GetManifestResourceStream(String.Format("LuaN.Studio.Syntax.{0}.xshd", name)))
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
