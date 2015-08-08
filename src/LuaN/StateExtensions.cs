using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Common state methods
    /// </summary>
    public static class StateExtensions
    {
        /// <summary>
        /// Alias to LuaToNumberX
        /// </summary>
        public static Double LuaToNumber(this ILuaState L, int idx, out Boolean isnum)
        {
            return L.LuaToNumberX(idx, out isnum);
        }
        /// <summary>
        /// Alias to LuaToIntegerX
        /// </summary>
        public static Int64 LuaToInteger(this ILuaState L, int idx, out Boolean isnum)
        {
            return L.LuaToIntegerX(idx, out isnum);
        }
        /// <summary>
        /// Alias to LuaToUnsignedX
        /// </summary>
        public static UInt64 LuaToUnsigned(this ILuaState L, int idx, out Boolean isnum)
        {
            return L.LuaToUnsignedX(idx, out isnum);
        }
        /// <summary>
        /// Alias to LuaLLoadString
        /// </summary>
        public static LuaStatus LoadString(this ILuaState L, String s)
        {
            return L.LuaLLoadString(s);
        }
        /// <summary>
        /// Alias to LuaLDoFile
        /// </summary>
        public static LuaStatus DoFile(this ILuaState L, String fn)
        {
            return L.LuaLDoFile(fn);
        }
        /// <summary>
        /// Alias to LuaLDoString
        /// </summary>
        public static LuaStatus DoString(this ILuaState L, String s)
        {
            return L.LuaLDoString(s);
        }
    }
}
