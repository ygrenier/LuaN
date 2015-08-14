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
        /// Alias to LuaLToLString
        /// </summary>
        public static String LuaLToString(this ILuaState L, int idx)
        {
            uint len;
            return L.LuaLToLString(idx, out len);
        }
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

        #region Chunk management

        /// <summary>
        /// Load a chunk from a string and returns the function
        /// </summary>
        public static ILuaFunction LoadString(this ILuaState L, String chunk, String name)
        {
            var oldTop = L.LuaGetTop();
            if (L.LuaLoadBuffer(chunk, name) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);
            var result = L.ToFunction(-1);
            L.LuaPop(1);
            return result;
        }

        /// <summary>
        /// Load a chunk from a buffer and returns the function
        /// </summary>
        public static ILuaFunction LoadString(this ILuaState L, byte[] chunk, String name)
        {
            var oldTop = L.LuaGetTop();
            if (L.LuaLoadBuffer(chunk, name) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);
            var result = L.ToFunction(-1);
            L.LuaPop(1);
            return result;
        }

        /// <summary>
        /// Load a chunk from a file and returns the function
        /// </summary>
        public static ILuaFunction LoadFile(this ILuaState L, String filename)
        {
            var oldTop = L.LuaGetTop();
            if (L.LuaLoadFile(filename) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);
            var result = L.ToFunction(-1);
            L.LuaPop(1);
            return result;
        }

        /// <summary>
        /// Execute a Lua chunk from a string and returns all values in an array
        /// </summary>
        public static Object[] DoString(this ILuaState L, String chunk, String name = "script")
        {
            var oldTop = L.LuaGetTop();
            if (L.LuaLoadBuffer(chunk, name) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);
            return LuaDotnetHelper.Call(L, null, null);
        }

        /// <summary>
        /// Execute a Lua chunk from a buffer and returns all values in an array
        /// </summary>
        public static Object[] DoString(this ILuaState L, byte[] chunk, String name = "script")
        {
            var oldTop = L.LuaGetTop();
            if (L.LuaLoadBuffer(chunk, name) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);
            return LuaDotnetHelper.Call(L, null, null);
        }

        /// <summary>
        /// Execute a Lua chunk from a file and returns all values in an array
        /// </summary>
        public static Object[] DoFile(this ILuaState L, String filename)
        {
            var oldTop = L.LuaGetTop();
            if (L.LuaLoadFile(filename) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);
            return LuaDotnetHelper.Call(L, null, null);
        }

        #endregion

        #region .Net object specific methods

        /// <summary>
        /// Register the .Net objet metatable
        /// </summary>
        public static void RegisterDotnetMetatable(this ILuaState L)
        {
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
            {
                ldn.RegisterDotnetMetatable(LuaDotnetHelper.DotnetObjectMetatableName);
            }
            else
            {
                LuaDotnetHelper.CreateDotnetObjectMetatable(L);
            }
        }

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
            return LuaDotnetHelper.DefaultToTable(L, idx);
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
            return LuaDotnetHelper.DefaultToUserData(L, idx);
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
            return LuaDotnetHelper.DefaultToFunction(L, idx);
        }

        /// <summary>
        /// Convert the Lua value at the index to a .Net object corresponding
        /// </summary>
        public static Object ToValue(this ILuaState L, int idx)
        {
            if (L == null) return null;
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
                return ldn.ToValue(idx);
            return LuaDotnetHelper.DefaultToValue(L, idx);
        }

        /// <summary>
        /// Push a value as a .Net object
        /// </summary>
        public static void PushNetObject(this ILuaState L, Object value)
        {
            var ldn = L.GetService<ILuaDotnet>();
            if (ldn != null)
            {
                ldn.PushNetObject(value);
            }
            else
            {
                LuaDotnetHelper.DefaultPushNetObject(L, value);
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
            LuaDotnetHelper.DefaultPush(L, value);
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
