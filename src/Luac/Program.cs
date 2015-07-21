using LuaNet.LuaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luac
{
    class Program
    {

        static void Main(string[] args)
        {
            var L = Lua.luaL_newstate();
            try
            {

            }
            finally
            {
                Lua.lua_close(L);
            }

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("\nPress a keu to quit...");
                Console.Read();
            }
#endif

        }

    }
}
