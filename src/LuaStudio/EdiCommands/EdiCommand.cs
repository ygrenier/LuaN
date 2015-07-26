using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{
    public class EdiCommand
    {
        public Object Sender { get; set; }
        public String Command { get; set; }
        public String Parameter { get; set; }
    }
}
