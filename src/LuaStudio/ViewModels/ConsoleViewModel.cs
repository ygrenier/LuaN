using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels
{

    /// <summary>
    /// ViewModel to manage the lines text inputed or outputed on the console
    /// </summary>
    public class ConsoleViewModel : ViewModel
    {
        class OutputWriter : System.IO.TextWriter
        {
            public OutputWriter(ConsoleViewModel console, ConsoleLineType lineType)
            {
                Console = console;
                LineType = lineType;
            }
            public override void Write(char value)
            {
                Console.InsertText(LineType, value.ToString());
            }
            public override void Write(string value)
            {
                Console.InsertText(LineType, value);
            }
            public ConsoleViewModel Console { get; private set; }
            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public ConsoleLineType LineType { get; private set; }
        }

        ConsoleLine _CurrentLine = null;

        /// <summary>
        /// Create a new ViewModel
        /// </summary>
        public ConsoleViewModel()
        {
            Lines = new ObservableCollection<ConsoleLine>();
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
                if( (result = text.IndexOf("\r\n")) >= 0)
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
            // Find the end of line
            int eolSize, index;
            while((index=IndexOfEOL(text,out eolSize)) >= 0)
            {
                var line = text.Substring(0, index);
                // We need to create a new line ?
                if (_CurrentLine == null || _CurrentLine.LineType != lineType )
                {
                    Lines.Add(new ConsoleLine
                    {
                        Line = line,
                        LineType = lineType
                    });
                }
                else
                {
                    // We complete the current line
                    _CurrentLine.Line += line;
                    Lines[Lines.Count - 1] = _CurrentLine;
                    RaisePropertyChanged(String.Format("Lines[{0}]", Lines.Count - 1));
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
                            Line = text,
                            LineType = lineType
                        };
                        Lines.Add(_CurrentLine);
                    }
                    else
                    {
                        // We complete the current line
                        _CurrentLine.Line += text;
                        Lines[Lines.Count - 1] = _CurrentLine;
                        RaisePropertyChanged(String.Format("Lines[{0}]", Lines.Count - 1));
                    }
                }
                else
                {
                    // We create a new line
                    _CurrentLine = new ConsoleLine
                    {
                        Line = text,
                        LineType = lineType
                    };
                    Lines.Add(_CurrentLine);
                }
            }
            CleanUpLines();
        }

        public void InsertOutput(String text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                InsertText(ConsoleLineType.Output, text);
            }
        }

        public void InsertInput(String text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                InsertText(ConsoleLineType.Input, text);
            }
        }

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
                if (SetProperty(ref _MaxLines, value, () => MaxLines))
                {
                    CleanUpLines();
                }
            }
        }
        private int _MaxLines;

        /// <summary>
        /// Lines of text
        /// </summary>
        public ObservableCollection<ConsoleLine> Lines { get; private set; }

        /// <summary>
        /// Writer to the Output lines
        /// </summary>
        public TextWriter Output { get; private set; }

        /// <summary>
        /// Writer to the Input lines
        /// </summary>
        public TextWriter Input { get; private set; }

        /// <summary>
        /// Writer to the Error lines
        /// </summary>
        public TextWriter Error { get; private set; }
    }

    /// <summary>
    /// Line text of the console
    /// </summary>
    public class ConsoleLine
    {
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

}
