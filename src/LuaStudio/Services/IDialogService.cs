using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaStudio.Services
{
    /// <summary>
    /// Service providing more dialog access
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Display a selector to open a file
        /// </summary>
        IEnumerable<String> FileOpen(
            String title,
            String defaultPath = null,
            bool multiSelect = false,
            bool acceptAllFileTypes = true
            );

        /// <summary>
        /// Display a selector to save a file
        /// </summary>
        String FileSave(
            String title,
            String defaultFilename = null,
            String defaultPath = null,
            TextEditors.ITextDefinition definition = null,
            bool acceptAllFileTypes = true
            );

        /// <summary>
        /// Open a confirm dialog box
        /// </summary>
        IDialogButton Confirm(
            String message,
            String title,
            params IDialogButton[] buttons
            );

        /// <summary>
        /// Display an error
        /// </summary>
        bool DisplayError(String tite, Exception error);
    }
}
