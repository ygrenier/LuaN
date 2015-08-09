using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Arguments for the OnPrint and OnWrite events
    /// </summary>
    public class WriteEventArgs : EventArgs
    {
        /// <summary>
        /// Create new arguments
        /// </summary>
        public WriteEventArgs(String text)
        {
            this.Text = text;
            this.Handled = false;
        }
        /// <summary>
        /// Text writting
        /// </summary>
        public String Text { get; private set; }

        /// <summary>
        /// Indicate if this arguments are handled
        /// </summary>
        public bool Handled { get; set; }

    }

}
