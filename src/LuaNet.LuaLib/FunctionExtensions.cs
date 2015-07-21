using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{
    /// <summary>
    /// Some extensions
    /// </summary>
    public static class FunctionExtensions
    {
        /// <summary>
        /// Convert a LuaFunction to a lua_CFunction
        /// </summary>
        public static Lua.lua_CFunction ToCFunction(this LuaFunction function)
        {
            if (function == null) return null;
            var proxy = LuaCFunctionProxy.GetProxy(function);
            return proxy != null ? proxy.UnmanagedFunction : null;
        }

        /// <summary>
        /// Convert a lua_CFunction to LuaFunction
        /// </summary>
        public static LuaFunction ToFunction(this Lua.lua_CFunction function)
        {
            if (function == null) return null;
            var proxy = LuaCFunctionProxy.GetProxy(function);
            return proxy != null ? proxy.ManagedFunction : null;
        }

        /// <summary>
        /// Convert a reader to lua reader
        /// </summary>
        public static Lua.lua_Reader ToLuaReader(this LuaReader reader)
        {
            if (reader == null) return null;
            var proxy = LuaReaderProxy.GetProxy(reader);
            return proxy != null ? proxy.UnmanagedReader : null;
        }

        /// <summary>
        /// Convert a lua reader to reader
        /// </summary>
        public static LuaReader ToReader(this Lua.lua_Reader reader)
        {
            if (reader == null) return null;
            var proxy = LuaReaderProxy.GetProxy(reader);
            return proxy != null ? proxy.ManagedReader : null;
        }

        /// <summary>
        /// Convert a writer to lua writer
        /// </summary>
        public static Lua.lua_Writer ToLuaWriter(this LuaWriter writer)
        {
            if (writer == null) return null;
            var proxy = LuaWriterProxy.GetProxy(writer);
            return proxy != null ? proxy.UnmanagedWriter : null;
        }

        /// <summary>
        /// Convert a lua writer to writer
        /// </summary>
        public static LuaWriter ToWriter(this Lua.lua_Writer writer)
        {
            if (writer == null) return null;
            var proxy = LuaWriterProxy.GetProxy(writer);
            return proxy != null ? proxy.ManagedWriter : null;
        }
    }
}
