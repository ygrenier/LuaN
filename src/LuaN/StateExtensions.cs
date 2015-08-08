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

        #region Aliases
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
        #endregion

        #region .Net object

        /// <summary>
        /// Convert the Lua value at the index to a .Net object corresponding
        /// </summary>
        public static Object ToObject(this ILuaState L, int idx)
        {
            var tp = L.LuaType(idx);
            switch (tp)
            {
                case LuaType.Boolean:
                    return L.LuaToBoolean(idx);
                case LuaType.Number:
                    return L.LuaToNumber(idx);
                case LuaType.String:
                    return L.LuaToString(idx);
                case LuaType.LightUserData:
                case LuaType.UserData:
                    return L.LuaToUserData(idx);
                case LuaType.Table:
                    throw new NotImplementedException();
                case LuaType.Function:
                    throw new NotImplementedException();
                case LuaType.Thread:
                    return L.LuaToThread(idx);
                case LuaType.None:
                case LuaType.Nil:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Push a .Net object value converted to the Lua value corresponding
        /// </summary>
        public static void Push(this ILuaState L, Object value)
        {
            if (value == null)
                L.LuaPushNil();
            else if (value is Boolean)
                L.LuaPushBoolean((Boolean)value);
            else if (value is Single || value is Double || value is Decimal)
                L.LuaPushNumber(Convert.ToDouble(value));
            else if (value is SByte || value is Byte || value is Int16 || value is UInt16 || value is Int32 || value is UInt16 || value is Int64 || value is UInt64)
                L.LuaPushInteger(Convert.ToInt64(value));
            else if (value is Char || value is String)
                L.LuaPushString(value.ToString());
            else if (value is ILuaUserData)
                throw new InvalidOperationException("Can't push a userdata");
            else if (value is LuaCFunction)
                L.LuaPushCFunction((LuaCFunction)value);
            else if (value is ILuaState)
                if (value == L)
                    L.LuaPushThread();
                else
                    throw new InvalidOperationException("Can't push a different thread");
            else
                L.LuaPushLightUserData(value);
            //case LuaType.LightUserData:
            //case LuaType.UserData:
            //    return L.LuaToUserData(idx);
            //case LuaType.Table:
            //    throw new NotImplementedException();
            //case LuaType.Function:
            //    throw new NotImplementedException();
        }

        #endregion

    }
}
