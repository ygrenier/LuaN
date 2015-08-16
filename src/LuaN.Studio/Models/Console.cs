using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Models
{

    /// <summary>
    /// Implements a basic console I/O
    /// </summary>
    public class Console : IConsole
    {
        class ConsoleTextWriter : System.IO.TextWriter
        {
            StringBuilder _Buffer = new StringBuilder();
            public ConsoleTextWriter(Console console, ConsoleLineType lineType)
            {
                Console = console;
                LineType = lineType;
                NewLine = Environment.NewLine;
            }
            protected void CheckFlush()
            {
                if (_Buffer.Length > 5)
                    Flush();
            }
            public override void Flush()
            {
                if (_Buffer.Length > 0)
                {
                    Console.InsertText(LineType, _Buffer.ToString());
                    _Buffer.Clear();
                }
            }
            public override void Write(char value)
            {
                _Buffer.Append(value);
                if (value != '\n' && value != '\r')
                    CheckFlush();
            }
            //public override void Write(string value)
            //{
            //    _Buffer.Append(value);
            //    CheckFlush();
            //}
            //public override void WriteLine()
            //{
            //    _Buffer.AppendLine();
            //    Flush();
            //}
            //public override void WriteLine(string value)
            //{
            //    _Buffer.AppendLine(value);
            //    Flush();
            //}
            public Console Console { get; private set; }
            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public ConsoleLineType LineType { get; private set; }
        }
        ConsoleLine _CurrentLine;

        /// <summary>
        /// Create a new console
        /// </summary>
        public Console()
        {
            Lines = new ObservableCollection<ConsoleLine>();
            Input = new ConsoleTextWriter(this, ConsoleLineType.Input);
            Output = new ConsoleTextWriter(this, ConsoleLineType.Output);
            Error = new ConsoleTextWriter(this, ConsoleLineType.Error);
            MaxLines = 500;
        }

        /// <summary>
        /// Raise TextEmitted event
        /// </summary>
        protected void OnTextEmitted(ConsoleLine line, bool isNewLine, String text)
        {
            var h = TextEmitted;
            if (h != null)
                h(this, new ConsoleTextEmittedEventArgs(line, isNewLine, text));
        }

        /// <summary>
        /// Find the Index of the EOL
        /// </summary>
        static int IndexOfEOL(String text, out int eolSize)
        {
            int result = -1;
            eolSize = 0;
            if (!String.IsNullOrEmpty(text))
            {
                if ((result = text.IndexOf("\r\n")) >= 0)
                {
                    eolSize = 2;
                }
                else if ((result = text.IndexOf("\n")) >= 0)
                {
                    eolSize = 1;
                }
                else if ((result = text.IndexOf("\r")) >= 0)
                {
                    eolSize = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// Insert text on lines
        /// </summary>
        protected void InsertText(ConsoleLineType lineType, String text)
        {
            if (String.IsNullOrEmpty(text)) return;
            // Flush the current line
            if(_CurrentLine!=null && _CurrentLine.LineType != lineType)
            {
                switch (_CurrentLine.LineType)
                {
                    case ConsoleLineType.Output:
                        Output.Flush();
                        break;
                    case ConsoleLineType.Error:
                        Error.Flush();
                        break;
                    case ConsoleLineType.Input:
                    default:
                        Input.Flush();
                        break;
                }
            }
            // Find the end of line
            int eolSize, index;
            while ((index = IndexOfEOL(text, out eolSize)) >= 0)
            {
                var line = text.Substring(0, index);
                // We need to create a new line ?
                if (_CurrentLine == null || _CurrentLine.LineType != lineType)
                {
                    _CurrentLine = new ConsoleLine
                    {
                        Date = DateTime.Now,
                        Line = line,
                        LineType = lineType
                    };
                    Lines.Add(_CurrentLine);
                    OnTextEmitted(_CurrentLine, true, line);
                }
                else
                {
                    // We complete the current line
                    _CurrentLine.Line += line;
                    Lines[Lines.Count - 1] = _CurrentLine;
                    OnTextEmitted(_CurrentLine, false, line);
                }
                // No current line because we find EOL
                _CurrentLine = null;
                // Extract the remaining text to insert
                text = text.Substring(index + eolSize);
            }
            // If we have remaining text
            if (text != String.Empty)
            {
                // If we have a current line
                if (_CurrentLine != null)
                {
                    // We need to create a new line ?
                    if (_CurrentLine.LineType != lineType)
                    {
                        _CurrentLine = new ConsoleLine
                        {
                            Date = DateTime.Now,
                            Line = text,
                            LineType = lineType
                        };
                        Lines.Add(_CurrentLine);
                        OnTextEmitted(_CurrentLine, true, text);
                    }
                    else
                    {
                        // We complete the current line
                        _CurrentLine.Line += text;
                        Lines[Lines.Count - 1] = _CurrentLine;
                        OnTextEmitted(_CurrentLine, false, text);
                    }
                }
                else
                {
                    // We create a new line
                    _CurrentLine = new ConsoleLine
                    {
                        Date = DateTime.Now,
                        Line = text,
                        LineType = lineType
                    };
                    Lines.Add(_CurrentLine);
                    OnTextEmitted(_CurrentLine, true, text);
                }
            }
            CleanUpLines();
        }

        /// <summary>
        /// Insert text in Output
        /// </summary>
        public void InsertOutput(String text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                InsertText(ConsoleLineType.Output, text);
            }
        }

        /// <summary>
        /// Insert text in Input
        /// </summary>
        public void InsertInput(String text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                InsertText(ConsoleLineType.Input, text);
            }
        }

        /// <summary>
        /// Insert text in Error
        /// </summary>
        public void InsertError(String text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                InsertText(ConsoleLineType.Error, text);
            }
        }

        /// <summary>
        /// Clean up the lines
        /// </summary>
        protected void CleanUpLines()
        {
            while (Lines.Count > MaxLines)
                Lines.RemoveAt(0);
        }

        /// <summary>
        /// Indicates the maximum of lines displayed
        /// </summary>
        public int MaxLines
        {
            get { return _MaxLines; }
            set
            {
                var newValue = Math.Max(3, value);
                if (_MaxLines != newValue)
                {
                    _MaxLines = newValue;
                    CleanUpLines();
                }
            }
        }
        private int _MaxLines;

        /// <summary>
        /// Writer to the input stream
        /// </summary>
        public TextWriter Input { get; private set; }
        /// <summary>
        /// Writer to the output stream
        /// </summary>
        public TextWriter Output { get; private set; }
        /// <summary>
        /// Writer to the error stream
        /// </summary>
        public TextWriter Error { get; private set; }
        /// <summary>
        /// List of lines emitted and received
        /// </summary>
        public ObservableCollection<ConsoleLine> Lines { get; private set; }
        /// <summary>
        /// Event raised when a line is emitted or updated
        /// </summary>
        public event EventHandler<ConsoleTextEmittedEventArgs> TextEmitted;
    }

}
