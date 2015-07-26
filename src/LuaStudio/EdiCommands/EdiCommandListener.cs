using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{
    public class EdiCommandListener
    {
        protected Predicate<EdiCommand> OnAcceptCommand { get; set; }
        public EdiCommandListener(Predicate<EdiCommand> onAccept = null)
        {
            OnAcceptCommand = onAccept;
        }
        public bool AcceptCommand(EdiCommand command)
        {
            if (command== null) return false;
            if (OnAcceptCommand != null)
                return OnAcceptCommand(command);
            return true;
        }
        public bool ReceiveCommand(EdiCommand command)
        {
            if (command == null) return false;
            EdiCommandEventArgs e = new EdiCommandEventArgs(command);
            var h = OnReceiveCommand;
            if (h != null)
                h(this, e);
            return e.Handled;
        }
        public event EventHandler<EdiCommandEventArgs> OnReceiveCommand;
    }
    public class EdiCommandEventArgs : EventArgs
    {
        public EdiCommandEventArgs(EdiCommand command)
        {
            this.Command = command;
            this.Handled = false;
        }
        public EdiCommand Command { get; private set; }
        public bool Handled { get; set; }
    }
}
