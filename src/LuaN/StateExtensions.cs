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
        /// Alias to LuaLLoadFile
        /// </summary>
        public static LuaStatus LoadFile(this ILuaState L, String fn)
        {
            return L.LuaLLoadFile(fn);
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
        /// <summary>
        /// Alias to LuaLLoadBuffer
        /// </summary>
        public static LuaStatus LoadBuffer(this ILuaState L, byte[] buffer, String name)
        {
            buffer = buffer ?? new byte[0];
            return L.LuaLLoadBuffer(buffer, buffer.Length, name);
        }
        /// <summary>
        /// Alias to LuaLLoadBufferX
        /// </summary>
        public static LuaStatus LoadBuffer(this ILuaState L, byte[] buffer, String name, String mode)
        {
            buffer = buffer ?? new byte[0];
            return L.LuaLLoadBufferX(buffer, buffer.Length, name, mode);
        }
        #endregion

        #region Reference management

        /// <summary>
        /// Get a reference using the registry
        /// </summary>
        public static int LuaRef(this ILuaState L)
        {
            return L.LuaLRef(L.RegistryIndex);
        }

        /// <summary>
        /// Push the value referenced by <paramref name="reference"/> on the registry
        /// </summary>
        public static LuaType LuaPushRef(this ILuaState L, int reference)
        {
            return L.LuaRawGetI(L.RegistryIndex, reference);
        }

        /// <summary>
        /// Push the value referenced by <paramref name="reference"/> 
        /// </summary>
        public static LuaType LuaPushRef(this ILuaState L, int table, int reference)
        {
            return L.LuaRawGetI(table, reference);
        }

        /// <summary>
        /// Remove a reference in the registry
        /// </summary>
        public static void LuaUnref(this ILuaState L, int reference)
        {
            L.LuaLUnref(L.RegistryIndex, reference);
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

        /// <summary>
        /// Returns the LuaToUserData typed
        /// </summary>
        public static T ToUserData<T>(this ILuaState L, int idx)
        {
            var obj = L.LuaToUserData(idx);
            return (obj is T) ? (T)obj : default(T);
        }

        #endregion

    }
}
