using LuaStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{

    public class EdiCommandMessage : Message
    {
        public EdiCommand ToEdiCommand()
        {
            return new EdiCommand
            {
                Sender = Sender,
                Command = Command,
                Parameter = Parameter
            };
        }
        public String Command { get; set; }
        public String Parameter { get; set; }
    }

}
