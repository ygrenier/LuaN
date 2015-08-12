using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    public static class LibraryExtensions
    {
        public static void OpenDotnet(this ILuaState L)
        {
            DotnetLibrary.Require(L);
        }
    }
}
