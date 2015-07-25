using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaStudio.Services
{

    /// <summary>
    /// Dialog service implementation
    /// </summary>
    [Export(typeof(IDialogService))]
    class DialogService : IDialogService
    {
        /// <summary>
        /// Display a selector to open a file
        /// </summary>
        public Task<IEnumerable<String>> FileOpen(
            String title,
            String defaultPath = null,
            bool multiSelect = false,
            bool acceptAllFileTypes = true
            )
        {
            var dlg = new OpenFileDialog();

            dlg.Title = title;
            dlg.Multiselect = multiSelect;
            if (!String.IsNullOrWhiteSpace(defaultPath))
                dlg.InitialDirectory = defaultPath;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;

            // Extract filters from text editor definition
            StringBuilder sb = new StringBuilder();
            foreach (var def in AppContext.Current.GetTextDefinitions())
            {
                String[] exts = def.Extensions.Split(',', ';').Select(e => "*" + e).ToArray();
                String filters = String.Join(";", exts);
                if (sb.Length > 0) sb.Append('|');
                sb.AppendFormat("{0} ({1})|{1}", def.Caption, filters);
            }
            if (acceptAllFileTypes)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.AppendFormat("{0} (*.*)|*.*", Resources.Locales.OpenFileDialog_AllTypesFiles_Caption);
            }
            dlg.Filter = sb.ToString();

            TaskCompletionSource<IEnumerable<String>> result = new TaskCompletionSource<IEnumerable<String>>();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                result.SetResult(dlg.FileNames);
            }
            else
            {
                result.SetResult(Enumerable.Empty<String>());
            }
            return result.Task;
        }
    }

}
