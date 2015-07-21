using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaNet.LuaLib
{
    using lua_State = IntPtr;

    /// <summary>
    /// $Id: lualib.h,v 1.44 2014/02/06 17:32:33 roberto Exp $
    /// Lua standard libraries
    /// See Copyright Notice in lua.h
    /// </summary>
    public static partial class Lua
    {
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_base(lua_State L);

        public const String LUA_COLIBNAME = "coroutine";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_coroutine(lua_State L);

        public const String LUA_TABLIBNAME = "table";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_table(lua_State L);

        public const String LUA_IOLIBNAME = "io";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_io(lua_State L);

        public const String LUA_OSLIBNAME = "os";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_os(lua_State L);

        public const String LUA_STRLIBNAME = "string";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_string(lua_State L);

        public const String LUA_UTF8LIBNAME = "utf8";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_utf8(lua_State L);

        public const String LUA_BITLIBNAME = "bit32";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_bit32(lua_State L);

        public const String LUA_MATHLIBNAME = "math";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_math(lua_State L);

        public const String LUA_DBLIBNAME = "debug";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_debug(lua_State L);

        public const String LUA_LOADLIBNAME = "package";
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int luaopen_package(lua_State L);

        /// <summary>
        /// open all previous libraries
        /// </summary>
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void luaL_openlibs(lua_State L);

        public static void lua_assert(bool x) { 
#if DEBUG
            System.Diagnostics.Debug.Assert(x);
#endif
        }

    }
}
