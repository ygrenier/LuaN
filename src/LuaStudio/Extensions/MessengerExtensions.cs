using LuaStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio
{
    public static class MessengerExtensions
    {
        public static void SendEdiMessage(this IMessengerService messenger, object sender, String command, String parameter = null)
        {
            if (messenger != null)
                messenger.SendMessage(new EdiCommands.EdiCommandMessage { Sender = sender, Command = command, Parameter = parameter });
        }
        public static void SendEdiMessage(this IMessengerService messenger, object sender, EdiCommands.IEdiCommandDefinition command, String parameter = null)
        {
            if (messenger != null && command != null)
                messenger.SendMessage(new EdiCommands.EdiCommandMessage { Sender = sender, Command = command.Name, Parameter = parameter });
        }
    }
}
