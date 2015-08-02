using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{
    using System.Runtime.InteropServices;

    using size_t = UInt32;

    /* unsigned integer type */
    using LUA_UNSIGNED = UInt64;
    using lua_Unsigned = UInt64;
    /* type for continuation-function contexts */
    using LUA_KCONTEXT = Int64;
    using lua_KContext = Int64;
    /* type of numbers in Lua */
    using LUA_NUMBER = Double;
    using lua_Number = Double;
    /* type for integer functions */
    using LUA_INTEGER = Int64;
    using lua_Integer = Int64;

    using lua_State = IntPtr;

    /// <summary>
    /// $Id: lua.h,v 1.325 2014/12/26 17:24:27 roberto Exp $
    /// Lua - A Scripting Language
    /// Lua.org, PUC-Rio, Brazil (http://www.lua.org)
    /// See Copyright Notice at the end of this file
    /// </summary>
    public static partial class Lua
    {
        public const String LUA_VERSION_MAJOR = "5";
        public const String LUA_VERSION_MINOR = "3";
        public const Double LUA_VERSION_NUM = 503;
        public const String LUA_VERSION_RELEASE = "0";

        public const String LUA_VERSION = "Lua " + LUA_VERSION_MAJOR + "." + LUA_VERSION_MINOR;
        public const String LUA_RELEASE = LUA_VERSION + "." + LUA_VERSION_RELEASE;
        public const String LUA_COPYRIGHT = LUA_RELEASE + "  Copyright (C) 1994-2015 Lua.org, PUC-Rio";
        public const String LUA_AUTHORS = "R. Ierusalimschy, L. H. de Figueiredo, W. Celes";

        /// <summary>
        /// mark for precompiled code ('<esc>Lua') 
        /// </summary>
        public const String LUA_SIGNATURE = "\x1bLua";

        /// <summary>
        /// option for multiple returns in 'lua_pcall' and 'lua_call' 
        /// </summary>
        public const int LUA_MULTRET = (-1);

        /*
        ** Pseudo-indices
        */
        public const int LUA_REGISTRYINDEX = LUAI_FIRSTPSEUDOIDX;
        public static int lua_upvalueindex(int i) { return (LUA_REGISTRYINDEX - (i)); }

        /* thread status */
        public const int LUA_OK = 0;
        public const int LUA_YIELD = 1;
        public const int LUA_ERRRUN = 2;
        public const int LUA_ERRSYNTAX = 3;
        public const int LUA_ERRMEM = 4;
        public const int LUA_ERRGCMM = 5;
        public const int LUA_ERRERR = 6;

        /*
        ** basic types
        */
        public const int LUA_TNONE = (-1);

        public const int LUA_TNIL = 0;
        public const int LUA_TBOOLEAN = 1;
        public const int LUA_TLIGHTUSERDATA = 2;
        public const int LUA_TNUMBER = 3;
        public const int LUA_TSTRING = 4;
        public const int LUA_TTABLE = 5;
        public const int LUA_TFUNCTION = 6;
        public const int LUA_TUSERDATA = 7;
        public const int LUA_TTHREAD = 8;

        public const int LUA_NUMTAGS = 9;

        /// <summary>
        /// minimum Lua stack available to a C function
        /// </summary>
        public const int LUA_MINSTACK = 20;

        /* predefined values in the registry */
        public const int LUA_RIDX_MAINTHREAD = 1;
        public const int LUA_RIDX_GLOBALS = 2;
        public const int LUA_RIDX_LAST = LUA_RIDX_GLOBALS;

        /// <summary>
        /// Type for C functions registered with Lua
        /// </summary>
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public delegate int lua_CFunction(lua_State L);

        /// <summary>
        /// Type for continuation functions
        /// </summary>
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public delegate int lua_KFunction(lua_State L, int status, lua_KContext ctx);

        /// <summary>
        /// Type for functions that read/write blocks when loading/dumping Lua chunks
        /// </summary>
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public delegate IntPtr lua_Reader(lua_State L, IntPtr ud, ref size_t sz);
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public delegate int lua_Writer(lua_State L, IntPtr p, size_t sz, IntPtr ud);

        /// <summary>
        /// Type for memory-allocation functions
        /// </summary>
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public delegate IntPtr lua_Alloc(IntPtr ud, IntPtr ptr, size_t osize, size_t nsize);

        /// <summary>
        /// RCS ident string
        /// </summary>
        /// <remarks>
        /// Can't access from exported data, so rebuild from code source
        /// </remarks>
        public static readonly String lua_ident = "$LuaVersion: " + LUA_COPYRIGHT + " $" + "$LuaAuthors: " + LUA_AUTHORS + " $";
        //public static readonly String lua_ident = LoadExportedData("lua_ident");

        /*
        ** state manipulation
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_State lua_newstate(lua_Alloc f, IntPtr ud);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_close(lua_State L);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_State lua_newthread(lua_State L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_CFunction lua_atpanic(lua_State L, lua_CFunction panicf);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_version")]
        extern static IntPtr _lua_version(lua_State L);
        public static lua_Number lua_version(lua_State L)
        {
            var ptr = _lua_version(L);
            return (Double)Marshal.PtrToStructure(ptr, typeof(Double));
        }

        /*
        ** basic stack manipulation
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_absindex(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_gettop(lua_State L);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_settop(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushvalue(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_rotate(lua_State L, int idx, int n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_copy(lua_State L, int fromidx, int toidx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_checkstack(lua_State L, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_xmove(lua_State from, lua_State to, int n);

        /*
        ** access functions (stack -> C)
        */

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_isnumber(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_isstring(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_iscfunction(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_isinteger(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_isuserdata(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_type(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_typename")]
        extern static IntPtr _lua_typename(lua_State L, int tp);
        public static String lua_typename(lua_State L, int tp) { return Marshal.PtrToStringAnsi(_lua_typename(L, tp)); }

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Number lua_tonumberx(lua_State L, int idx, out int isnum);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Integer lua_tointegerx(lua_State L, int idx, out int isnum);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_toboolean(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_tolstring")]
        extern static IntPtr _lua_tolstring(lua_State L, int idx, out size_t len);
        public static String lua_tolstring(lua_State L, int idx, out size_t len) { return Marshal.PtrToStringAnsi(_lua_tolstring(L, idx, out len)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static size_t lua_rawlen(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_CFunction lua_tocfunction(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr lua_touserdata(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_State lua_tothread(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr lua_topointer(lua_State L, int idx);

        /*
        ** Comparison and arithmetic functions
        */

        public const int LUA_OPADD = 0;	/* ORDER TM, ORDER OP */
        public const int LUA_OPSUB = 1;
        public const int LUA_OPMUL = 2;
        public const int LUA_OPMOD = 3;
        public const int LUA_OPPOW = 4;
        public const int LUA_OPDIV = 5;
        public const int LUA_OPIDIV = 6;
        public const int LUA_OPBAND = 7;
        public const int LUA_OPBOR = 8;
        public const int LUA_OPBXOR = 9;
        public const int LUA_OPSHL = 10;
        public const int LUA_OPSHR = 11;
        public const int LUA_OPUNM = 12;
        public const int LUA_OPBNOT = 13;

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_arith(lua_State L, int op);

        public const int LUA_OPEQ = 0;
        public const int LUA_OPLT = 1;
        public const int LUA_OPLE = 2;

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_rawequal(lua_State L, int idx1, int idx2);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_compare(lua_State L, int idx1, int idx2, int op);

        /*
        ** push functions (C -> stack)
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushnil(lua_State L);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushnumber(lua_State L, lua_Number n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushinteger(lua_State L, lua_Integer n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushlstring")]
        extern static IntPtr _lua_pushlstring(lua_State L, String s, size_t len);
        public static String lua_pushlstring(lua_State L, String s, size_t len) { return Marshal.PtrToStringAnsi(_lua_pushlstring(L, s, len)); }
        //public extern static String lua_pushlstring(lua_State L, String s, size_t len);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushstring")]
        extern static IntPtr _lua_pushstring(lua_State L, String s);
        public static String lua_pushstring(lua_State L, String s) { return Marshal.PtrToStringAnsi(_lua_pushstring(L, s)); }
        //public extern static String lua_pushstring(lua_State L, String s);
        #region push(v)fstring
        //[DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushvfstring")]
        //extern static IntPtr _lua_pushvfstring(lua_State L, String fmt, __arglist);
        // Not compatible with .Net
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt);
        public static String lua_pushfstring(lua_State L, String fmt)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, String arg0);
        public static String lua_pushfstring(lua_State L, String fmt, String arg0)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, lua_Number arg0);
        public static String lua_pushfstring(lua_State L, String fmt, lua_Number arg0)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, lua_Integer arg0);
        public static String lua_pushfstring(lua_State L, String fmt, lua_Integer arg0)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, String arg0, String arg1);
        public static String lua_pushfstring(lua_State L, String fmt, String arg0, String arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, String arg0, lua_Number arg1);
        public static String lua_pushfstring(lua_State L, String fmt, String arg0, lua_Number arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, String arg0, Int32 arg1);
        public static String lua_pushfstring(lua_State L, String fmt, String arg0, Int32 arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, lua_Number arg0, String arg1);
        public static String lua_pushfstring(lua_State L, String fmt, lua_Number arg0, String arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, lua_Number arg0, lua_Number arg1);
        public static String lua_pushfstring(lua_State L, String fmt, lua_Number arg0, lua_Number arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, lua_Number arg0, Int32 arg1);
        public static String lua_pushfstring(lua_State L, String fmt, lua_Number arg0, Int32 arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, Int32 arg0, String arg1);
        public static String lua_pushfstring(lua_State L, String fmt, Int32 arg0, String arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, Int32 arg0, lua_Number arg1);
        public static String lua_pushfstring(lua_State L, String fmt, Int32 arg0, lua_Number arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushfstring")]
        extern static IntPtr _lua_pushfstring(lua_State L, String fmt, Int32 arg0, Int32 arg1);
        public static String lua_pushfstring(lua_State L, String fmt, Int32 arg0, Int32 arg1)
        { return Marshal.PtrToStringAnsi(_lua_pushfstring(L, fmt, arg0, arg1)); }
        #endregion
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushcclosure(lua_State L, lua_CFunction fn, int n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushboolean(lua_State L, int b);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_pushlightuserdata(lua_State L, IntPtr p);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_pushthread(lua_State L);

        /*
        ** get functions (Lua -> stack)
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_getglobal(lua_State L, String name);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_gettable(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_getfield(lua_State L, int idx, String k);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_geti(lua_State L, int idx, lua_Integer n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_rawget(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_rawgeti(lua_State L, int idx, lua_Integer n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_rawgetp(lua_State L, int idx, IntPtr p);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_createtable(lua_State L, int narr, int nrec);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr lua_newuserdata(lua_State L, size_t sz);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_getmetatable(lua_State L, int objindex);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_getuservalue(lua_State L, int idx);

        /*
        ** set functions (stack -> Lua)
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_setglobal(lua_State L, String name);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_settable(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_setfield(lua_State L, int idx, String k);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_seti(lua_State L, int idx, lua_Integer n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_rawset(lua_State L, int idx);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_rawseti(lua_State L, int idx, lua_Integer n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_rawsetp(lua_State L, int idx, IntPtr p);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_setmetatable(lua_State L, int objindex);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_setuservalue(lua_State L, int idx);

        /*
        ** 'load' and 'call' functions (load and run Lua code)
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_callk(lua_State L, int nargs, int nresults, lua_KContext ctx, lua_KFunction k);
        public static void lua_call(lua_State L, int nargs, int nresults)
        { lua_callk(L, (nargs), (nresults), 0, null); }

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_pcallk(lua_State L, int nargs, int nresults, int errfunc, lua_KContext ctx, lua_KFunction k);
        public static int lua_pcall(lua_State L, int nargs, int nresults, int errfunc)
        { return lua_pcallk(L, (nargs), (nresults), (errfunc), 0, null); }

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_load(lua_State L, lua_Reader reader, IntPtr dt, String chunkname, String mode);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_dump(lua_State L, lua_Writer writer, IntPtr data, int strip);

        /*
        ** coroutine functions
        */
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_yieldk(lua_State L, int nresults, lua_KContext ctx, lua_KFunction k);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_resume(lua_State L, lua_State from, int narg);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_status(lua_State L);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_isyieldable(lua_State L);

        public static int lua_yield(lua_State L, int nresults)
        { return lua_yieldk(L, (nresults), 0, null); }


        /*
        ** garbage-collection function and options
        */

        public const int LUA_GCSTOP = 0;
        public const int LUA_GCRESTART = 1;
        public const int LUA_GCCOLLECT = 2;
        public const int LUA_GCCOUNT = 3;
        public const int LUA_GCCOUNTB = 4;
        public const int LUA_GCSTEP = 5;
        public const int LUA_GCSETPAUSE = 6;
        public const int LUA_GCSETSTEPMUL = 7;
        public const int LUA_GCISRUNNING = 9;

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_gc(lua_State L, int what, int data);

        /*
        ** miscellaneous functions
        */

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_error(lua_State L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_next(lua_State L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_concat(lua_State L, int n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_len(lua_State L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_stringtonumber(lua_State L, String s);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Alloc lua_getallocf(lua_State L, IntPtr ud);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_setallocf(lua_State L, lua_Alloc f, IntPtr ud);

        /*
        ** {==============================================================
        ** some useful macros
        ** ===============================================================
        */

        //#define lua_getextraspace(L)	((void *)((char *)(L) - LUA_EXTRASPACE))

        public static lua_Number lua_tonumber(lua_State L, int idx) { int dummy; return lua_tonumberx(L, idx, out dummy); }
        public static lua_Integer lua_tointeger(lua_State L, int idx) { int dummy; return lua_tointegerx(L, idx, out dummy); }

        public static void lua_pop(lua_State L, int n) { lua_settop(L, -(n) - 1); }

        public static void lua_newtable(lua_State L) { lua_createtable(L, 0, 0); }

        public static void lua_register(lua_State L, String n, lua_CFunction f) { lua_pushcfunction(L, (f)); lua_setglobal(L, (n)); }

        public static void lua_pushcfunction(lua_State L, lua_CFunction f) { lua_pushcclosure(L, (f), 0); }

        public static bool lua_isfunction(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TFUNCTION); }
        public static bool lua_istable(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TTABLE); }
        public static bool lua_islightuserdata(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TLIGHTUSERDATA); }
        public static bool lua_isnil(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TNIL); }
        public static bool lua_isboolean(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TBOOLEAN); }
        public static bool lua_isthread(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TTHREAD); }
        public static bool lua_isnone(lua_State L, int n) { return (lua_type(L, (n)) == LUA_TNONE); }
        public static bool lua_isnoneornil(lua_State L, int n) { return (lua_type(L, (n)) <= 0); }

        public static String lua_pushliteral(lua_State L, String s) { return lua_pushstring(L, s ?? String.Empty); }

        public static int lua_pushglobaltable(lua_State L) { return lua_rawgeti(L, LUA_REGISTRYINDEX, LUA_RIDX_GLOBALS); }

        public static String lua_tostring(lua_State L, int i) { UInt32 dummy; return lua_tolstring(L, (i), out dummy); }

        public static void lua_insert(lua_State L, int idx) { lua_rotate(L, (idx), 1); }

        public static void lua_remove(lua_State L, int idx) { lua_rotate(L, (idx), -1); lua_pop(L, 1); }

        public static void lua_replace(lua_State L, int idx) { lua_copy(L, -1, (idx)); lua_pop(L, 1); }

        /* }============================================================== */

        /*
        ** {==============================================================
        ** compatibility macros for unsigned conversions
        ** ===============================================================
        */
        //#if defined(LUA_COMPAT_APIINTCASTS)

        public static void lua_pushunsigned(lua_State L, UInt32 n) { lua_pushinteger(L, (lua_Integer)(n)); }
        public static lua_Unsigned lua_tounsignedx(lua_State L, int i, out int isn) { return ((lua_Unsigned)lua_tointegerx(L, i, out isn)); }
        public static lua_Unsigned lua_tounsigned(lua_State L, int i) { int dummy; return lua_tounsignedx(L, (i), out dummy); }

        //#endif
        /* }============================================================== */

        /*
        ** {======================================================================
        ** Debug API
        ** =======================================================================
        */

        /*
        ** Event codes
        */
        public const int LUA_HOOKCALL = 0;
        public const int LUA_HOOKRET = 1;
        public const int LUA_HOOKLINE = 2;
        public const int LUA_HOOKCOUNT = 3;
        public const int LUA_HOOKTAILCALL = 4;

        /*
        ** Event masks
        */
        public const int LUA_MASKCALL = (1 << LUA_HOOKCALL);
        public const int LUA_MASKRET = (1 << LUA_HOOKRET);
        public const int LUA_MASKLINE = (1 << LUA_HOOKLINE);
        public const int LUA_MASKCOUNT = (1 << LUA_HOOKCOUNT);

        /* Functions to be called by the debugger in specific events */
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        //public delegate void lua_Hook(lua_State L, lua_Debug ar);
        public delegate void lua_Hook(lua_State L, IntPtr ar);
        //typedef void (*lua_Hook) (lua_State *L, lua_Debug *ar);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_getstack(lua_State L, int level, IntPtr ar);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static int lua_getinfo(lua_State L, String what, lua_Debug ar);
        public extern static int lua_getinfo(lua_State L, String what, IntPtr ar);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_getlocal")]
        extern static IntPtr _lua_getlocal(lua_State L, IntPtr ar, int n);
        public static String lua_getlocal(lua_State L, IntPtr ar, int n)
        { return Marshal.PtrToStringAnsi(_lua_getlocal(L, ar, n)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_setlocal")]
        extern static IntPtr _lua_setlocal(lua_State L, IntPtr ar, int n);
        public static String lua_setlocal(lua_State L, IntPtr ar, int n)
        { return Marshal.PtrToStringAnsi(_lua_setlocal(L, ar, n)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_getupvalue")]
        extern static IntPtr _lua_getupvalue(lua_State L, int funcindex, int n);
        public static String lua_getupvalue(lua_State L, int funcindex, int n)
        { return Marshal.PtrToStringAnsi(_lua_getupvalue(L, funcindex, n)); }
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_setupvalue")]
        extern static IntPtr _lua_setupvalue(lua_State L, int funcindex, int n);
        public static String lua_setupvalue(lua_State L, int funcindex, int n)
        { return Marshal.PtrToStringAnsi(_lua_setupvalue(L, funcindex, n)); }

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr lua_upvalueid(lua_State L, int fidx, int n);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_upvaluejoin(lua_State L, int fidx1, int n1, int fidx2, int n2);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void lua_sethook(lua_State L, lua_Hook func, int mask, int count);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Hook lua_gethook(lua_State L);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_gethookmask(lua_State L);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int lua_gethookcount(lua_State L);

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public class lua_Debug
        //{
        //    public int evnt;
        //    //[MarshalAs(UnmanagedType.LPStr)]
        //    //public String name;	/* (n) */
        //    //[MarshalAs(UnmanagedType.LPStr)]
        //    //public String namewhat;	/* (n) 'global', 'local', 'field', 'method' */
        //    //[MarshalAs(UnmanagedType.LPStr)]
        //    //public String what;	/* (S) 'Lua', 'C', 'main', 'tail' */
        //    //[MarshalAs(UnmanagedType.LPStr)]
        //    //public String source;	/* (S) */

        //    IntPtr pname;
        //    IntPtr pnamewhat;
        //    IntPtr pwhat;
        //    IntPtr psource;

        //    public int currentline;	/* (l) */
        //    public int linedefined;	/* (S) */
        //    public int lastlinedefined;	/* (S) */
        //    public byte nups;	/* (u) number of upvalues */
        //    public byte nparams;/* (u) number of parameters */
        //    public sbyte isvararg;        /* (u) */
        //    public sbyte istailcall; /* (t) */
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Lua.LUA_IDSIZE)] 
        //    public string short_src; /* (S) */
        //    /* private part */
        //    //public struct CallInfo *i_ci;  /* active function */
        //    public IntPtr i_ci;  /* active function */

        //    public String name { get { return Marshal.PtrToStringAnsi(pname); } }	/* (n) */
        //    public String namewhat { get { return Marshal.PtrToStringAnsi(pnamewhat); } }	/* (n) 'global', 'local', 'field', 'method' */
        //    public String what { get { return Marshal.PtrToStringAnsi(pwhat); } }	/* (S) 'Lua', 'C', 'main', 'tail' */
        //    public String source { get { return Marshal.PtrToStringAnsi(psource); } }	/* (S) */
        //};

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class lua_Debug
        {
            public int evnt;
            public IntPtr name; /* (n) */
            public IntPtr namewhat;   /* (n) 'global', 'local', 'field', 'method' */
            public IntPtr what;   /* (S) 'Lua', 'C', 'main', 'tail' */
            public IntPtr source; /* (S) */
            public int currentline;    /* (l) */
            public int linedefined;    /* (S) */
            public int lastlinedefined;    /* (S) */
            public byte nups; /* (u) number of upvalues */
            public byte nparams;/* (u) number of parameters */
            public sbyte isvararg;        /* (u) */
            public sbyte istailcall;    /* (t) */
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Lua.LUA_IDSIZE)]
            public string short_src; /* (S) */
            /* private part */
            public IntPtr i_ci;  /* active function */
        };

        /* }====================================================================== */


        ///******************************************************************************
        //* Copyright (C) 1994-2015 Lua.org, PUC-Rio.
        //*
        //* Permission is hereby granted, free of charge, to any person obtaining
        //* a copy of this software and associated documentation files (the
        //* "Software"), to deal in the Software without restriction, including
        //* without limitation the rights to use, copy, modify, merge, publish,
        //* distribute, sublicense, and/or sell copies of the Software, and to
        //* permit persons to whom the Software is furnished to do so, subject to
        //* the following conditions:
        //*
        //* The above copyright notice and this permission notice shall be
        //* included in all copies or substantial portions of the Software.
        //*
        //* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        //* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        //* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
        //* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
        //* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
        //* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
        //* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        //******************************************************************************/

    }

}
