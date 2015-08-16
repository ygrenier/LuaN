using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Models
{

    /// <summary>
    /// Console
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Writer to the input stream
        /// </summary>
        TextWriter Input { get; }
        /// <summary>
        /// Writer to the output stream
        /// </summary>
        TextWriter Output { get; }
        /// <summary>
        /// Writer to the error stream
        /// </summary>
        TextWriter Error { get; }
        /// <summary>
        /// List of lines emitted and received
        /// </summary>
        ObservableCollection<ConsoleLine> Lines { get; }
        /// <summary>
        /// Event raised when a line is emitted or updated
        /// </summary>
        event EventHandler<ConsoleTextEmittedEventArgs> TextEmitted;
    }

    /// <summary>
    /// Line text of the console
    /// </summary>
    public class ConsoleLine
    {
        /// <summary>
        /// Date line created
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Text of the line
        /// </summary>
        public String Line { get; set; }
        /// <summary>
        /// Type of line
        /// </summary>
        public ConsoleLineType LineType { get; set; }
    }

    /// <summary>
    /// Type of line console
    /// </summary>
    public enum ConsoleLineType
    {
        /// <summary>
        /// Input line
        /// </summary>
        Input,
        /// <summary>
        /// Output line
        /// </summary>
        Output,
        /// <summary>
        /// Error line
        /// </summary>
        Error
    }

    /// <summary>
    /// Arguments for the OnTextEmitted is raised
    /// </summary>
    public class ConsoleTextEmittedEventArgs : EventArgs
    {
        /// <summary>
        /// Create new arguments
        /// </summary>
        public ConsoleTextEmittedEventArgs(ConsoleLine line, bool isNewLine, String text)
        {
            this.Line = line;
            this.IsNewLine = isNewLine;
            this.Text = text;
        }
        /// <summary>
        /// Console line 
        /// </summary>
        public ConsoleLine Line { get; private set; }
        /// <summary>
        /// If true it's a new console line, else it's a line updated
        /// </summary>
        public bool IsNewLine { get; private set; }
        /// <summary>
        /// Text emitted
        /// </summary>
        public String Text { get; private set; }
    }

}
