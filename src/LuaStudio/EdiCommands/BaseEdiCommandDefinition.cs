using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.EdiCommands
{
    public class BaseEdiCommandDefinition : IEdiCommandDefinition
    {
        public BaseEdiCommandDefinition(String name, String caption = null, String description = null)
        {
            this.Name = name;
            this.Caption = caption ?? name;
            this.Description = description;
        }
        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string Description { get; private set; }

    }
}
