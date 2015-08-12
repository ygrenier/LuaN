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
        /// Returns a typed service
        /// </summary>
        public static T GetService<T>(this ILuaState L)
        {
            var o = L.GetService(typeof(T));
            return o != null ? (T)o : default(T);
        }

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
        public static LuaStatus LuaLoadFile(this ILuaState L, String fn)
        {
            return L.LuaLLoadFile(fn);
        }
        /// <summary>
        /// Alias to LuaLLoadString
        /// </summary>
        public static LuaStatus LuaLoadString(this ILuaState L, String s)
        {
            return L.LuaLLoadString(s);
        }
        /// <summary>
        /// Alias to LuaLDoFile
        /// </summary>
        public static LuaStatus LuaDoFile(this ILuaState L, String fn)
        {
            return L.LuaLDoFile(fn);
        }
        /// <summary>
        /// Alias to LuaLDoString
        /// </summary>
        public static LuaStatus LuaDoString(this ILuaState L, String s)
        {
            return L.LuaLDoString(s);
        }
        /// <summary>
        /// Alias to LuaLLoadBuffer
        /// </summary>
        public static LuaStatus LuaLoadBuffer(this ILuaState L, String buffer, String name)
        {
            var bbuffer = Encoding.UTF8.GetBytes(buffer ?? String.Empty) ?? new byte[0];
            return L.LuaLLoadBuffer(bbuffer, bbuffer.Length, name);
        }
        /// <summary>
        /// Alias to LuaLLoadBuffer
        /// </summary>
        public static LuaStatus LuaLoadBuffer(this ILuaState L, byte[] buffer, String name)
        {
            buffer = buffer ?? new byte[0];
            return L.LuaLLoadBuffer(buffer, buffer.Length, name);
        }
        /// <summary>
        /// Alias to LuaLLoadBufferX
        /// </summary>
        public static LuaStatus LuaLoadBuffer(this ILuaState L, byte[] buffer, String name, String mode)
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

        #region .Net object specific methods

        /// <summary>
        /// Pop the value at the top of the stack
        /// </summary>
        public static Object Pop(this ILuaState L)
        {
            if (L.LuaGetTop() > 0)
            {
                var result = L.ToValue(-1);
                L.LuaPop(1);
                return result;
            }
            return null;
        }

        /// <summary>
        /// Pop the typed value at the top of the stack
        /// </summary>
        public static T Pop<T>(this ILuaState L)
        {
            try
            {
                return (T)Convert.ChangeType(L.Pop(), typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Pop the n values at the top of the stack
        /// </summary>
        public static Object[] PopValues(this ILuaState L, int n)
        {
            List<Object> result = new List<object>();
            if (n > 0)
            {
                int top = L.LuaGetTop();
                int start = Math.Max(0, top - n);
                result.AddRange(LuaDotnetHelper.ExtractValues(L, start + 1, top, null));
                while (result.Count < n) result.Add(null);
                L.LuaSetTop(start);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Call the referenced value 
        /// </summary>
        public static Object[] CallValue(this ILuaState L, int reference, Object[] args, Type[] typedResult = null)
        {
            L.LuaPushRef(reference);
            return LuaDotnetHelper.Call(L, args, typedResult);
        }

        /// <summary>
        /// Call a function
        /// </summary>
        public static Object[] CallFunction(this ILuaState L, Object function, Object[] args, Type[] typedResult = null)
        {
            L.Push(function);
            return LuaDotnetHelper.Call(L, args, typedResult);
        }

        /// <summary>
        /// Convert a Lua value to a .Net table
        /// </summary>
        public static ILuaTable ToTable(this ILuaState L, int idx)
        {
            if (L == null) return null;
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
                return ldn.ToTable(idx);
            return LuaDotnetHelper.ToTable(L, idx);
        }

        /// <summary>
        /// Convert a lua value to a .Net userdata
        /// </summary>
        public static ILuaUserData ToUserData(this ILuaState L, int idx)
        {
            if (L == null) return null;
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
                return ldn.ToUserData(idx);
            return LuaDotnetHelper.ToUserData(L, idx);
        }

        /// <summary>
        /// Convert a lua value to a .Net function
        /// </summary>
        public static ILuaFunction ToFunction(this ILuaState L, int idx)
        {
            if (L == null) return null;
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
                return ldn.ToFunction(idx);
            return LuaDotnetHelper.ToFunction(L, idx);
        }

        /// <summary>
        /// Convert the Lua value at the index to a .Net object corresponding
        /// </summary>
        public static Object ToObject(this ILuaState L, int idx)
        {
            return L.ToValue(idx);
        }
        public static Object ToValue(this ILuaState L, int idx)
        {
            if (L == null) return null;
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
                return ldn.ToValue(idx);
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
                    return L.LuaToUserData(idx);
                case LuaType.UserData:
                    return L.ToUserData(idx);
                case LuaType.Table:
                    return L.ToTable(idx);
                case LuaType.Function:
                    return L.ToFunction(idx);
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
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
            {
                ldn.Push(value);
                return;
            }
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
            else if (value is ILuaNativeUserData)
                throw new InvalidOperationException("Can't push a userdata");
            else if (value is LuaCFunction)
                L.LuaPushCFunction((LuaCFunction)value);
            else if (value is ILuaState)
                if (value == L)
                    L.LuaPushThread();
                else
                    throw new InvalidOperationException("Can't push a different thread");
            else if (value is ILuaValue)
                ((ILuaValue)value).Push(L);
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
