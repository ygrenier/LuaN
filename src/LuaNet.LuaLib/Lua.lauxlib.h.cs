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
    /// $Id: lauxlib.h,v 1.128 2014/10/29 16:11:17 roberto Exp $
    /// Auxiliary functions for building Lua libraries
    /// See Copyright Notice in lua.h
    /// </summary>
    public static partial class Lua
    {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class luaL_Reg
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public String name;
            public lua_CFunction func;
        }

        //#define LUAL_NUMSIZES	(sizeof(lua_Integer)*16 + sizeof(lua_Number))
        //public const int LUAL_NUMSIZES = (8 * 16) + 8;
        public static readonly int LUAL_NUMSIZES = (sizeof(lua_Integer) * 16 + sizeof(lua_Number));

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_checkversion_(lua_State L, lua_Number ver, int sz);
        public static void luaL_checkversion(lua_State L) { luaL_checkversion_(L, LUA_VERSION_NUM, LUAL_NUMSIZES); }

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_getmetafield(lua_State L, int obj, String e);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_callmeta(lua_State L, int obj, String e);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_tolstring")]
        extern static IntPtr _luaL_tolstring(lua_State L, int idx, out size_t len);
        public static String luaL_tolstring(lua_State L, int idx, out size_t len)
        { return Marshal.PtrToStringAnsi(_luaL_tolstring(L, idx, out len)); }
        //public extern static String luaL_tolstring(lua_State L, int idx, out size_t len);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_argerror(lua_State L, int arg, String extramsg);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_checklstring")]
        extern static IntPtr _luaL_checklstring(lua_State L, int arg, out size_t l);
        public static String luaL_checklstring(lua_State L, int arg, out size_t l)
        { return Marshal.PtrToStringAnsi(_luaL_checklstring(L, arg, out l)); }
        //public extern static String luaL_checklstring(lua_State L, int arg, out size_t l);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_optlstring")]
        extern static IntPtr _luaL_optlstring(lua_State L, int arg, String def, out size_t l);
        public static String luaL_optlstring(lua_State L, int arg, String def, out size_t l)
        { return Marshal.PtrToStringAnsi(_luaL_optlstring(L, arg, def, out l)); }
        //public extern static String luaL_optlstring(lua_State L, int arg, String def, out size_t l);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Number luaL_checknumber(lua_State L, int arg);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Number luaL_optnumber(lua_State L, int arg, lua_Number def);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Integer luaL_checkinteger(lua_State L, int arg);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Integer luaL_optinteger(lua_State L, int arg, lua_Integer def);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_checkstack(lua_State L, int sz, String msg);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_checktype(lua_State L, int arg, int t);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_checkany(lua_State L, int arg);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_newmetatable(lua_State L, String tname);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_setmetatable(lua_State L, String tname);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr luaL_testudata(lua_State L, int ud, String tname);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr luaL_checkudata(lua_State L, int ud, String tname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_where(lua_State L, int lvl);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_error(lua_State L, String fmt, __arglist);
        //LUALIB_API int (luaL_error) (lua_State *L, const char *fmt, ...);

        //[DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static int luaL_checkoption(lua_State L, int arg, String def, String[] lst);
        // TODO Check the String[] transmission

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_fileresult(lua_State L, int stat, String fname);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_execresult(lua_State L, int stat);

        /* pre-defined references */
        public const int LUA_NOREF = (-2);
        public const int LUA_REFNIL = (-1);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_ref(lua_State L, int t);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_unref(lua_State L, int t, int r);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_loadfilex(lua_State L, String filename, String mode);

        public static int luaL_loadfile(lua_State L, String filename) { return luaL_loadfilex(L, filename, null); }

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_loadbufferx(lua_State L, String buff, int sz, String name, String mode);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_loadstring(lua_State L, String s);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_State luaL_newstate();

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static lua_Integer luaL_len(lua_State L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_gsub")]
        //extern static IntPtr _luaL_gsub(lua_State L, String s, String p, String r);
        //public static String luaL_gsub(lua_State L, String s, String p, String r)
        //{ return Marshal.PtrToStringAnsi(_luaL_gsub(L, s, p, r)); }
        public extern static String luaL_gsub(lua_State L, String s, String p, String r);


        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_setfuncs(lua_State L, luaL_Reg[] l, int nup);
        // TODO Check the luaL_Reg[] transmission

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int luaL_getsubtable(lua_State L, int idx, String fname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_traceback(lua_State L, lua_State L1, String msg, int level);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void luaL_requiref(lua_State L, String modname, lua_CFunction openf, int glb);


        /*
        ** ===============================================================
        ** some useful macros
        ** ===============================================================
        */

        public static void luaL_newlibtable(lua_State L, luaL_Reg[] l)
        { lua_createtable(L, 0, l.Length); }

        public static void luaL_newlib(lua_State L, luaL_Reg[] l)
        { luaL_checkversion(L); luaL_newlibtable(L, l); luaL_setfuncs(L, l, 0); }

        public static void luaL_argcheck(lua_State L, bool cond, int arg, String extramsg)
        { if (!cond) luaL_argerror(L, arg, extramsg); }
        public static String luaL_checkstring(lua_State L, int n)
        { UInt32 dummy; return luaL_checklstring(L, n, out dummy); }
        public static String luaL_optstring(lua_State L, int n, String def)
        { UInt32 dummy; return luaL_optlstring(L, n, def, out dummy); }

        public static String luaL_typename(lua_State L, int idx) { return lua_typename(L, lua_type(L, (idx))); }

        public static int luaL_dofile(lua_State L, String fn)
        {
            var res = luaL_loadfile(L, fn);
            if (res == LUA_OK)
                return lua_pcall(L, 0, LUA_MULTRET, 0);
            return res;
        }

        public static int luaL_dostring(lua_State L, String s)
        {
            var res = luaL_loadstring(L, s);
            if (res == LUA_OK)
                return lua_pcall(L, 0, LUA_MULTRET, 0);
            return res;
        }

        public static int luaL_getmetatable(lua_State L, String n) { return lua_getfield(L, LUA_REGISTRYINDEX, (n)); }

        //public static int luaL_opt(lua_State L, lua_CFunction f, int n, int d) { return (lua_isnoneornil(L, (n)) ? (d) : f(L, (n))); }
        //#define luaL_opt(L,f,n,d)	(lua_isnoneornil(L,(n)) ? (d) : f(L,(n)))

        public static int luaL_loadbuffer(lua_State L, String s, int sz, String n) { return luaL_loadbufferx(L, s, sz, n, null); }

        ///*
        //** {======================================================
        //** Generic Buffer manipulation
        //** =======================================================
        //*/

        //typedef struct luaL_Buffer {
        //  char *b;  /* buffer address */
        //  size_t size;  /* buffer size */
        //  size_t n;  /* number of characters in buffer */
        //  lua_State *L;
        //  char initb[LUAL_BUFFERSIZE];  /* initial buffer */
        //} luaL_Buffer;


        //#define luaL_addchar(B,c) \
        //  ((void)((B)->n < (B)->size || luaL_prepbuffsize((B), 1)), \
        //   ((B)->b[(B)->n++] = (c)))

        //#define luaL_addsize(B,s)	((B)->n += (s))

        //LUALIB_API void (luaL_buffinit) (lua_State *L, luaL_Buffer *B);
        //LUALIB_API char *(luaL_prepbuffsize) (luaL_Buffer *B, size_t sz);
        //LUALIB_API void (luaL_addlstring) (luaL_Buffer *B, const char *s, size_t l);
        //LUALIB_API void (luaL_addstring) (luaL_Buffer *B, const char *s);
        //LUALIB_API void (luaL_addvalue) (luaL_Buffer *B);
        //LUALIB_API void (luaL_pushresult) (luaL_Buffer *B);
        //LUALIB_API void (luaL_pushresultsize) (luaL_Buffer *B, size_t sz);
        //LUALIB_API char *(luaL_buffinitsize) (lua_State *L, luaL_Buffer *B, size_t sz);

        //#define luaL_prepbuffer(B)	luaL_prepbuffsize(B, LUAL_BUFFERSIZE)

        ///* }====================================================== */



        ///*
        //** {======================================================
        //** File handles for IO library
        //** =======================================================
        //*/

        ///*
        //** A file handle is a userdata with metatable 'LUA_FILEHANDLE' and
        //** initial structure 'luaL_Stream' (it may contain other fields
        //** after that initial structure).
        //*/

        //#define LUA_FILEHANDLE          "FILE*"


        //typedef struct luaL_Stream {
        //  FILE *f;  /* stream (NULL for incompletely created streams) */
        //  lua_CFunction closef;  /* to close stream (NULL for closed streams) */
        //} luaL_Stream;

        ///* }====================================================== */



        ///* compatibility with old module system */
        //#if defined(LUA_COMPAT_MODULE)

        //LUALIB_API void (luaL_pushmodule) (lua_State *L, const char *modname,
        //                                   int sizehint);
        //LUALIB_API void (luaL_openlib) (lua_State *L, const char *libname,
        //                                const luaL_Reg *l, int nup);

        //#define luaL_register(L,n,l)	(luaL_openlib(L,(n),(l),0))

        //#endif

        /*
        ** {==================================================================
        ** "Abstraction Layer" for basic report of messages and errors
        ** ===================================================================
        */

        /// <summary>
        /// Process write
        /// </summary>
        static void ProcessWrite(String s, EventHandler<LuaWriteEventArgs> h, System.IO.TextWriter w)
        {
            LuaWriteEventArgs e = new LuaWriteEventArgs(s);
            if (h != null) h(null, e);
            if (!e.Handled)
            {
                w.Write(e.Text);
                w.Flush();
            }
        }

        /// <summary>
        /// print a string
        /// </summary>
        public static void lua_writestring(String s, int l)
        {
            ProcessWrite(s.Substring(0, l), OnWriteString, Console.Out);
        }

        /// <summary>
        /// print a string
        /// </summary>
        public static void lua_writestring(String s)
        { lua_writestring(s, s.Length); }

        /// <summary>
        /// print a newline and flush the output
        /// </summary>
        public static void lua_writeline()
        {
            ProcessWrite(Environment.NewLine, OnWriteLine, Console.Out);
        }

        /// <summary>
        /// print an error message
        /// </summary>
        public static void lua_writestringerror(String s, String p)
        {
            ProcessWrite(String.Format(s.Replace("%s", "{0}"), p), OnWriteStringError, Console.Error);
        }

        /// <summary>
        /// Event raised when lua_writestring is called
        /// </summary>
        public static event EventHandler<LuaWriteEventArgs> OnWriteString;
        /// <summary>
        /// Event raised when lua_writeline is called
        /// </summary>
        public static event EventHandler<LuaWriteEventArgs> OnWriteLine;
        /// <summary>
        /// Event raised when lua_writestringerror is called
        /// </summary>
        public static event EventHandler<LuaWriteEventArgs> OnWriteStringError;

        public class LuaWriteEventArgs : EventArgs
        {
            public LuaWriteEventArgs(String text)
            {
                this.Text = text;
                this.Handled = false;
            }
            public String Text { get; private set; }
            public bool Handled { get; set; }
        }

        /* }================================================================== */

        /*
        ** {============================================================
        ** Compatibility with deprecated conversions
        ** =============================================================
        */

        public static lua_Unsigned luaL_checkunsigned(lua_State L, int a) { return (lua_Unsigned)luaL_checkinteger(L, a); }
        public static lua_Unsigned luaL_optunsigned(lua_State L, int a, int d)
        { return (lua_Unsigned)luaL_optinteger(L, a, d); }

        public static int luaL_checkint(lua_State L, int n) { return (int)luaL_checkinteger(L, (n)); }
        public static int luaL_optint(lua_State L, int n, int d) { return (int)luaL_optinteger(L, (n), d); }

        public static long luaL_checklong(lua_State L, int n) { return (long)luaL_checkinteger(L, (n)); }
        public static long luaL_optlong(lua_State L, int n, int d) { return (long)luaL_optinteger(L, (n), d); }

        /* }============================================================ */

    }

}
