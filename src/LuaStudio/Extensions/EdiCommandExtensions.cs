using LuaStudio.EdiCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio
{
    public static class EdiCommandExtensions
    {
        public static bool IsCommand(this IEdiCommandDefinition definition, EdiCommand command)
        {
            if (definition == null || command == null) return false;
            return String.Equals(definition.Name, command.Command, StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsCommand(this IEdiCommandDefinition definition, String command)
        {
            if (definition == null || command == null) return false;
            return String.Equals(definition.Name, command, StringComparison.OrdinalIgnoreCase);
        }
    }
}
