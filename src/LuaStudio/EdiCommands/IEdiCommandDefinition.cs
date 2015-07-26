using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{
    public interface IEdiCommandDefinition
    {
        String Name { get; }
        String Caption { get; }
        String Description{ get; }
    }
}
