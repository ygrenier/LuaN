using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaNet.LuaLib.Tests
{
    public class LuaTest
    {
        [Fact]
        public void TestVersion()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Assert.Equal(503d, Lua.lua_version(L));
                Lua.luaL_checkversion(L);

                Lua.lua_pushcfunction(L, l => {
                    Lua.luaL_checkversion_(L, Lua.LUA_VERSION_NUM, 1);
                    return 0;
                });
                var status = Lua.lua_pcall(L, 0, 0, 0);
                Assert.NotEqual(Lua.LUA_OK, status);
                String msg = Lua.lua_tostring(L, -1);
                Assert.Equal("core and library have incompatible numeric types", msg);
                Lua.lua_pop(L, 1);

                Lua.lua_pushcfunction(L, l => {
                    Lua.luaL_checkversion_(L, 1, Lua.LUAL_NUMSIZES);
                    return Lua.LUA_OK;
                });
                status = Lua.lua_pcall(L, 0, 0, 0);
                Assert.NotEqual(Lua.LUA_OK, status);
                msg = Lua.lua_tostring(L, -1);
                Assert.Equal("version mismatch: app. needs 1.0, Lua core provides 503.0", msg);
                Lua.lua_pop(L, 1);
            }
            finally
            {
                Lua.lua_close(L);
            }
        }

        [Fact]
        public void TestWriteString()
        {
            StringBuilder serr = new StringBuilder();
            var oldErr = Console.Error;
            Console.SetError(new System.IO.StringWriter(serr));
            try
            {
                StringBuilder sb = new StringBuilder();
                Lua.OnWriteString += (s, e) => {
                    sb.Append(e.Text);
                    e.Handled = true;
                };
                Lua.OnWriteLine += (s, e) => {
                    sb.Append(e.Text);
                    e.Handled = true;
                };
                Lua.OnWriteStringError += (s, e) => {
                    sb.AppendFormat("ERR: {0}", e.Text).AppendLine();
                    e.Handled = false;
                };
                Lua.lua_writestring("First");
                Lua.lua_writestring(" test");
                Lua.lua_writeline();
                Lua.lua_writestring("Second");
                Lua.lua_writestring(" test");
                Lua.lua_writeline();
                Lua.lua_writestringerror("-%s-", "One error");

                Assert.Equal("First test\r\nSecond test\r\nERR: -One error-\r\n", sb.ToString());

                Assert.Equal("-One error-", serr.ToString());
            }
            finally
            {
                Console.SetError(oldErr);
            }
        }

        [Fact]
        public void TestLuaPushFString()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Assert.Equal("-s-d-", Lua.lua_pushfstring(L, "-s-d-"));
                Assert.Equal("-Text-", Lua.lua_pushfstring(L, "-%s-", "Text"));
                Assert.Equal("-123.45-", Lua.lua_pushfstring(L, "-%f-", 123.45));
                Assert.Equal("-987-", Lua.lua_pushfstring(L, "-%d-", 987));
                Assert.Equal("-Str-Text-", Lua.lua_pushfstring(L, "-%s-%s-", "Str", "Text"));
                Assert.Equal("-Str-123.45-", Lua.lua_pushfstring(L, "-%s-%f-", "Str", 123.45));
                Assert.Equal("-Str-987-", Lua.lua_pushfstring(L, "-%s-%d-", "Str", 987));
                Assert.Equal("-11.22-Text-", Lua.lua_pushfstring(L, "-%f-%s-", 11.22, "Text"));
                Assert.Equal("-11.22-123.45-", Lua.lua_pushfstring(L, "-%f-%f-", 11.22, 123.45));
                Assert.Equal("-11.22-987-", Lua.lua_pushfstring(L, "-%f-%d-", 11.22, 987));
                Assert.Equal("-9988-Text-", Lua.lua_pushfstring(L, "-%d-%s-", 9988, "Text"));
                Assert.Equal("-9988-123.45-", Lua.lua_pushfstring(L, "-%d-%f-", 9988, 123.45));
                Assert.Equal("-9988-987-", Lua.lua_pushfstring(L, "-%d-%d-", 9988, 987));
                
                Assert.Equal("-s-d-", Lua.lua_tostring(L, 1));
                Assert.Equal("-Text-", Lua.lua_tostring(L, 2));
                Assert.Equal("-123.45-", Lua.lua_tostring(L, 3));
                Assert.Equal("-987-", Lua.lua_tostring(L, 4));
                Assert.Equal("-Str-Text-", Lua.lua_tostring(L, 5));
                Assert.Equal("-Str-123.45-", Lua.lua_tostring(L, 6));
                Assert.Equal("-Str-987-", Lua.lua_tostring(L, 7));
                Assert.Equal("-11.22-Text-", Lua.lua_tostring(L, 8));
                Assert.Equal("-11.22-123.45-", Lua.lua_tostring(L, 9));
                Assert.Equal("-11.22-987-", Lua.lua_tostring(L, 10));
                Assert.Equal("-9988-Text-", Lua.lua_tostring(L, 11));
                Assert.Equal("-9988-123.45-", Lua.lua_tostring(L, 12));
                Assert.Equal("-9988-987-", Lua.lua_tostring(L, 13));
            }
            finally
            {
                Lua.lua_close(L);
            }
        }

        [Fact]
        public void TestNumber()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Lua.lua_pushnumber(L, 123.45);
                Assert.Equal(123.45, Lua.lua_tonumber(L, -1));
                Assert.Equal(0, Lua.lua_tointeger(L, -1));

                Lua.lua_pushinteger(L, 987);
                Assert.Equal(987, Lua.lua_tointeger(L, -1));
                int b;
                Assert.Equal(987, Lua.lua_tonumberx(L, -1, out b));
                Assert.Equal(1, b);

            }
            finally
            {
                Lua.lua_close(L);
            }
        }

        [Fact]
        public void TestLString()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Lua.lua_pushnumber(L, 123.45);
                UInt32 l;
                Assert.Equal("123.45", Lua.luaL_tolstring(L, -1, out l));
                Assert.Equal(6ul, l);

            }
            finally
            {
                Lua.lua_close(L);
            }
        }

        [Fact]
        public void TestCall()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Lua.lua_pushcfunction(L, l => {
                    double a = Lua.lua_tonumber(l, 1);
                    double b = Lua.lua_tonumber(l, 2);
                    Assert.Equal(3, a);
                    Assert.Equal(7, b);
                    Lua.lua_pushnumber(l, a - b);
                    return 1;
                });
                Lua.lua_pushnumber(L, 3);
                Lua.lua_pushnumber(L, 7);
                Assert.Equal(3, Lua.lua_gettop(L));
                Lua.lua_call(L, 2, 1);
                Assert.Equal(1, Lua.lua_gettop(L));
                Assert.Equal(-4, Lua.lua_tonumber(L, -1));
            }
            finally
            {
                Lua.lua_close(L);
            }
        }

        [Fact]
        public void TestLoad()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Lua.luaL_loadstring(L, "return 2+2");
                Lua.lua_pcall(L, 0, Lua.LUA_MULTRET, 0);
                Assert.Equal(1, Lua.lua_gettop(L));
                Assert.Equal(4d, Lua.lua_tonumber(L, -1));

                String content = "return 7*3";
                IntPtr resContent = IntPtr.Zero;
                var r = Lua.lua_load(L, (IntPtr l, IntPtr ud, ref UInt32 sz) => {
                    if (resContent != IntPtr.Zero) Marshal.FreeHGlobal(resContent);
                    resContent = IntPtr.Zero;
                    String res;
                    if (content.Length == 0)
                    {
                        sz = 0;
                    }
                    else if (content.Length < 3)
                    {
                        sz = (UInt32)content.Length;
                        res = content;
                        resContent = Marshal.AllocHGlobal((int)sz);
                        Marshal.Copy(Encoding.ASCII.GetBytes(res), 0, resContent, (int)sz);
                        content = String.Empty;
                    }
                    else
                    {
                        res = content.Substring(0, 3);
                        content = content.Substring(3);
                        sz = (UInt32)res.Length;
                        resContent = Marshal.AllocHGlobal((int)sz);
                        Marshal.Copy(Encoding.ASCII.GetBytes(res), 0, resContent, (int)sz);
                    }
                    return resContent;
                }, IntPtr.Zero, "main", null);
                Assert.Equal(Lua.LUA_OK, r);
                Lua.lua_pcall(L, 0, Lua.LUA_MULTRET, 0);
                Assert.Equal(2, Lua.lua_gettop(L));
                Assert.Equal(21d, Lua.lua_tonumber(L, -1));
            }
            finally
            {
                Lua.lua_close(L);
            }
        }

        [Fact]
        public void TestDump()
        {
            var L = Lua.luaL_newstate();
            try
            {
                Lua.luaL_loadstring(L, "return 2+2");

                StringBuilder sb = new StringBuilder();
                var r = Lua.lua_dump(L, (IntPtr l, IntPtr p, UInt32 sz, IntPtr ud) => {
                    Byte[] b = new Byte[sz];
                    Marshal.Copy(p, b, 0, (int)sz);
                    String s = Encoding.ASCII.GetString(b);
                    sb.Append(s);
                    return 0;
                }, IntPtr.Zero, 0);
                Assert.Equal(Lua.LUA_OK, r);
            }
            finally
            {
                Lua.lua_close(L);
            }
        }

    }
}
