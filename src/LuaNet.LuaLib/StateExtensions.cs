using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet
{
    /// <summary>
    /// State extensions
    /// </summary>
    public static class StateExtensions
    {
        /// <summary>
        /// Try to convert a value to a number
        /// </summary>
        public static bool TryToNumber(this ILuaState state, int index, out Double result)
        {
            bool res;
            result = state.ToNumber(index, out res);
            return res;
        }

        /// <summary>
        /// Try to convert a value to a integer
        /// </summary>
        public static bool TryToInteger(this ILuaState state, int index, out Int32 result)
        {
            bool res;
            result = state.ToInteger(index, out res);
            return res;
        }

        /// <summary>
        /// lua_tolstring simulation
        /// </summary>
        public static String ToLString(this ILuaState state, int index, out int len)
        {
            var s = state.ToString(index);
            len = s.Length;
            return s;
        }

        public static LuaStatus LoadBuffer(this ILuaState state, String s, String n) { return state.LoadBuffer(s, s.Length, n); }

        public static String PushLString(this ILuaState state, String s, int len)
        {
            s = s.Substring(0, len);
            state.PushString(s);
            return s;
        }

        public static T ToUserData<T>(this ILuaState state, int index)
        {
            var o = state.ToUserData(index);
            return o is T ? (T)o : default(T);
        }


    }
}
