using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaDllTest
    {
        [Fact]
        public void TestConstantsAndMacro()
        {
            Assert.Equal("Lua 5.3.0  Copyright (C) 1994-2015 Lua.org, PUC-Rio", LuaDll.LUA_COPYRIGHT);
            Assert.Equal("$LuaVersion: Lua 5.3.0  Copyright (C) 1994-2015 Lua.org, PUC-Rio $$LuaAuthors: R. Ierusalimschy, L. H. de Figueiredo, W. Celes $", LuaDll.lua_ident);
            Assert.Equal("'%s'", LuaDll.LUA_QS);
            Assert.Equal("'test'", LuaDll.LUA_QL("test"));
        }

        [Fact]
        public void Test_l_floor()
        {
            Assert.Equal(234, LuaDll.l_floor(234.56));
            Assert.Equal(-235, LuaDll.l_floor(-234.56));
        }

        [Fact]
        public void Test_lua_integer2str()
        {
            Assert.Equal("2345", LuaDll.lua_integer2str(2345));
            Assert.Equal("-5432", LuaDll.lua_integer2str(-5432));

            String s;
            Assert.Equal(4, LuaDll.lua_integer2str(out s, 2345));
            Assert.Equal("2345", s);
            Assert.Equal(5, LuaDll.lua_integer2str(out s, -5432));
            Assert.Equal("-5432", s);
        }

        [Fact]
        public void Test_lua_number2str()
        {
            Assert.Equal("2345", LuaDll.lua_number2str(2345));
            Assert.Equal("2345.67", LuaDll.lua_number2str(2345.67));
            Assert.Equal("-5432", LuaDll.lua_number2str(-5432));
            Assert.Equal("-5432.12", LuaDll.lua_number2str(-5432.12));

            String s;
            Assert.Equal(4, LuaDll.lua_number2str(out s, 2345));
            Assert.Equal("2345", s);
            Assert.Equal(7, LuaDll.lua_number2str(out s, 2345.67));
            Assert.Equal("2345.67", s);
            Assert.Equal(5, LuaDll.lua_number2str(out s, -5432));
            Assert.Equal("-5432", s);
            Assert.Equal(8, LuaDll.lua_number2str(out s, -5432.12));
            Assert.Equal("-5432.12", s);
        }

        [Fact]
        public void Test_lua_numbertointeger()
        {
            long n;
            Assert.Equal(true, LuaDll.lua_numbertointeger(2345,out n));
            Assert.Equal(2345, n);

            Assert.Equal(true, LuaDll.lua_numbertointeger(2345.67, out n));
            Assert.Equal(2345, n);

            Assert.Equal(true, LuaDll.lua_numbertointeger(-2345, out n));
            Assert.Equal(-2345, n);

            Assert.Equal(true, LuaDll.lua_numbertointeger(-2345.67, out n));
            Assert.Equal(-2345, n);

            Assert.Equal(false, LuaDll.lua_numbertointeger(Double.MaxValue, out n));
            Assert.Equal(0, n);

            Assert.Equal(false, LuaDll.lua_numbertointeger(Double.MinValue, out n));
            Assert.Equal(0, n);

            Assert.Equal(false, LuaDll.lua_numbertointeger(Double.NaN, out n));
            Assert.Equal(0, n);

            Assert.Equal(false, LuaDll.lua_numbertointeger(Double.NegativeInfinity, out n));
            Assert.Equal(0, n);

            Assert.Equal(false, LuaDll.lua_numbertointeger(Double.PositiveInfinity, out n));
            Assert.Equal(0, n);
        }

        [Fact]
        public void Test_lua_str2number()
        {
            int p;

            Assert.Equal(2345, LuaDll.lua_str2number("2345", out p));
            Assert.Equal(4, p);

            Assert.Equal(2345, LuaDll.lua_str2number("+2345", out p));
            Assert.Equal(5, p);

            Assert.Equal(-2345, LuaDll.lua_str2number("-2345", out p));
            Assert.Equal(5, p);

            Assert.Equal(2345.67, LuaDll.lua_str2number("2345.67", out p));
            Assert.Equal(7, p);

            Assert.Equal(2345.67, LuaDll.lua_str2number("+2345.67", out p));
            Assert.Equal(8, p);

            Assert.Equal(-2345.67, LuaDll.lua_str2number("-2345.67", out p));
            Assert.Equal(8, p);

            Assert.Equal(2345, LuaDll.lua_str2number("2345test", out p));
            Assert.Equal(4, p);

            Assert.Equal(0, LuaDll.lua_str2number("test2345", out p));
            Assert.Equal(0, p);

            Assert.Equal(0, LuaDll.lua_str2number(null, out p));
            Assert.Equal(0, p);

            Assert.Equal(0, LuaDll.lua_str2number("   ", out p));
            Assert.Equal(0, p);

            Assert.Equal(2345, LuaDll.lua_str2number("  2345  ", out p));
            Assert.Equal(8, p);

            Assert.Equal(-2345, LuaDll.lua_str2number("  -2345  ", out p));
            Assert.Equal(9, p);

            Assert.Equal(Double.PositiveInfinity, LuaDll.lua_str2number("  INFINITY", out p));
            Assert.Equal(10, p);

            Assert.Equal(Double.NegativeInfinity, LuaDll.lua_str2number("  -INfinity", out p));
            Assert.Equal(11, p);

            Assert.Equal(Double.PositiveInfinity, LuaDll.lua_str2number("  INF ", out p));
            Assert.Equal(5, p);

            Assert.Equal(Double.NegativeInfinity, LuaDll.lua_str2number("  -INF ", out p));
            Assert.Equal(6, p);

            Assert.Equal(Double.NaN, LuaDll.lua_str2number("  nan ", out p));
            Assert.Equal(5, p);

            Assert.Equal(Double.NaN, LuaDll.lua_str2number("  -nan ", out p));
            Assert.Equal(6, p);

            Assert.Equal(0, LuaDll.lua_str2number("  0x ", out p));
            Assert.Equal(4, p);

            Assert.Equal(0x12ABC, LuaDll.lua_str2number("  0x12abC ", out p));
            Assert.Equal(9, p);

            Assert.Equal(0x12ABC, LuaDll.lua_str2number("  0x12abCGz ", out p));
            Assert.Equal(9, p);

            Assert.Equal(-0x12ABC, LuaDll.lua_str2number("  -0x12abC", out p));
            Assert.Equal(10, p);

            Assert.Equal(-0x12ABC, LuaDll.lua_str2number("  -0x12abCGz", out p));
            Assert.Equal(10, p);
        }

        [Fact]
        public void Test_lua_upvalueindex()
        {
            Assert.Equal(LuaDll.LUAI_FIRSTPSEUDOIDX, LuaDll.lua_upvalueindex(0));
            Assert.Equal(LuaDll.LUAI_FIRSTPSEUDOIDX - 10, LuaDll.lua_upvalueindex(10));
            Assert.Equal(LuaDll.LUAI_FIRSTPSEUDOIDX + 10, LuaDll.lua_upvalueindex(-10));
        }

        [Fact]
        public void Test_lua_version()
        {
            var L = LuaDll.luaL_newstate();
            try {
                Assert.Equal(503d, LuaDll.lua_version(L));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
            Assert.Equal(503d, LuaDll.lua_version(IntPtr.Zero));
        }

        [Fact]
        public void Test_lua_typename()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                Assert.Equal("no value", LuaDll.lua_typename(L, LuaDll.LUA_TNONE));
                Assert.Equal("nil", LuaDll.lua_typename(L, LuaDll.LUA_TNIL));
                Assert.Equal("boolean", LuaDll.lua_typename(L, LuaDll.LUA_TBOOLEAN));
                Assert.Equal("userdata", LuaDll.lua_typename(L, LuaDll.LUA_TLIGHTUSERDATA));
                Assert.Equal("number", LuaDll.lua_typename(L, LuaDll.LUA_TNUMBER));
                Assert.Equal("string", LuaDll.lua_typename(L, LuaDll.LUA_TSTRING));
                Assert.Equal("table", LuaDll.lua_typename(L, LuaDll.LUA_TTABLE));
                Assert.Equal("function", LuaDll.lua_typename(L, LuaDll.LUA_TFUNCTION));
                Assert.Equal("userdata", LuaDll.lua_typename(L, LuaDll.LUA_TUSERDATA));
                Assert.Equal("thread", LuaDll.lua_typename(L, LuaDll.LUA_TTHREAD));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_tolstring()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                uint len;
                Assert.Equal(null, LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(0u, len);

                LuaDll.lua_pushnil(L);
                Assert.Equal(null, LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(0u, len);

                LuaDll.lua_pushnumber(L, 123.45);
                Assert.Equal("123.45", LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(6u, len);

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_tostring()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                Assert.Equal(null, LuaDll.lua_tostring(L, -1));

                LuaDll.lua_pushnil(L);
                Assert.Equal(null, LuaDll.lua_tostring(L, -1));

                LuaDll.lua_pushnumber(L, 123.45);
                Assert.Equal("123.45", LuaDll.lua_tostring(L, -1));

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pushstring_pushlstring()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                uint len;

                LuaDll.lua_pushstring(L, "String");
                Assert.Equal("String", LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(6u, len);

                LuaDll.lua_pushlstring(L, "String", 3);
                Assert.Equal("Str", LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(3u, len);

                LuaDll.lua_pushlstring(L, "String", 0);
                Assert.Equal("", LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(0u, len);

                LuaDll.lua_pushstring(L, null);
                Assert.Equal(null, LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(0u, len);

                LuaDll.lua_pushstring(L, String.Empty);
                Assert.Equal(String.Empty, LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(0u, len);

                LuaDll.lua_pushlstring(L, null, 0);
                Assert.Equal("", LuaDll.lua_tolstring(L, -1, out len));
                Assert.Equal(0u, len);

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_call()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushcclosure(L, state =>
                {
                    Assert.Equal(2, LuaDll.lua_gettop(state));
                    LuaDll.lua_pushvalue(state, 1);
                    LuaDll.lua_pushvalue(state, 2);
                    LuaDll.lua_arith(state, LuaDll.LUA_OPADD);
                    LuaDll.lua_pushvalue(state, 1);
                    LuaDll.lua_pushvalue(state, 2);
                    LuaDll.lua_arith(state, LuaDll.LUA_OPMUL);
                    return 2;
                }, 0);
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushnumber(L, 5);
                LuaDll.lua_call(L, 2, LuaDll.LUA_MULTRET);
                Assert.Equal(2, LuaDll.lua_gettop(L));
                int isn;
                Assert.Equal(8, LuaDll.lua_tonumberx(L, -2, out isn));
                Assert.Equal(15, LuaDll.lua_tonumberx(L, -1, out isn));

                LuaDll.lua_atpanic(L, state =>
                {
                    uint len;
                    throw new ApplicationException(LuaDll.lua_tolstring(state, -1, out len));
                });

                LuaDll.lua_pushcclosure(L, state =>
                {
                    LuaDll.lua_pushstring(state, "Raise an error");
                    LuaDll.lua_error(state);
                    return 0;
                }, 0);
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushnumber(L, 5);
                var ex=Assert.Throws<ApplicationException>(() => LuaDll.lua_call(L, 2, LuaDll.LUA_MULTRET));
                Assert.Equal("Raise an error", ex.Message);

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pcall()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushcclosure(L, state =>
                {
                    Assert.Equal(2, LuaDll.lua_gettop(state));
                    LuaDll.lua_pushvalue(state, 1);
                    LuaDll.lua_pushvalue(state, 2);
                    LuaDll.lua_arith(state, LuaDll.LUA_OPADD);
                    LuaDll.lua_pushvalue(state, 1);
                    LuaDll.lua_pushvalue(state, 2);
                    LuaDll.lua_arith(state, LuaDll.LUA_OPMUL);
                    return 2;
                }, 0);
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushnumber(L, 5);
                Assert.Equal(LuaDll.LUA_OK, LuaDll.lua_pcall(L, 2, LuaDll.LUA_MULTRET, 0));
                Assert.Equal(2, LuaDll.lua_gettop(L));
                int isn;
                Assert.Equal(8, LuaDll.lua_tonumberx(L, -2, out isn));
                Assert.Equal(15, LuaDll.lua_tonumberx(L, -1, out isn));

                uint len;
                LuaDll.lua_atpanic(L, state =>
                {
                    throw new ApplicationException(LuaDll.lua_tolstring(state, -1, out len));
                });

                LuaDll.lua_pushcclosure(L, state =>
                {
                    LuaDll.lua_pushstring(state, "Raise an error");
                    LuaDll.lua_error(state);
                    return 0;
                }, 0);
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushnumber(L, 5);
                Assert.Equal(LuaDll.LUA_ERRRUN, LuaDll.lua_pcall(L, 2, LuaDll.LUA_MULTRET, 0));
                Assert.Equal("Raise an error", LuaDll.lua_tolstring(L, -1, out len));

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_yield()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                uint len;
                LuaDll.lua_atpanic(L, state =>
                {
                    throw new ApplicationException(LuaDll.lua_tolstring(state, -1, out len));
                });
                var ex = Assert.Throws<ApplicationException>(() => LuaDll.lua_yield(L, 0));
                Assert.Equal("attempt to yield from outside a coroutine", ex.Message);
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_tonumber()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnil(L);
                Assert.Equal(0, LuaDll.lua_tonumber(L, -1));

                LuaDll.lua_pushnumber(L, 123.45);
                Assert.Equal(123.45, LuaDll.lua_tonumber(L, -1));

                LuaDll.lua_pushinteger(L, 123);
                Assert.Equal(123, LuaDll.lua_tonumber(L, -1));

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_tointeger()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnil(L);
                Assert.Equal(0, LuaDll.lua_tointeger(L, -1));

                LuaDll.lua_pushnumber(L, 123.45);
                Assert.Equal(0, LuaDll.lua_tointeger(L, -1));

                LuaDll.lua_pushinteger(L, 123);
                Assert.Equal(123, LuaDll.lua_tointeger(L, -1));

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pop()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnil(L);
                LuaDll.lua_pushnumber(L, 123.45);
                LuaDll.lua_pushinteger(L, 123);

                Assert.Equal(3, LuaDll.lua_gettop(L));
                LuaDll.lua_pop(L, 2);
                Assert.Equal(1, LuaDll.lua_gettop(L));

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_newtable()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_newtable(L);
                Assert.Equal(LuaDll.LUA_TTABLE, LuaDll.lua_type(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pushcfunction()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_CFunction func = state => 0;
                LuaDll.lua_pushcfunction(L, func);
                Assert.Equal(LuaDll.LUA_TFUNCTION, LuaDll.lua_type(L, -1));
                Assert.Same(func, LuaDll.lua_tocfunction(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_register()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_CFunction func = state => 0;
                LuaDll.lua_register(L, "MyFunc", func);
                LuaDll.lua_getglobal(L, "MyFunc");
                Assert.Equal(LuaDll.LUA_TFUNCTION, LuaDll.lua_type(L, -1));
                Assert.Same(func, LuaDll.lua_tocfunction(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_isXXX()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnil(L);
                LuaDll.lua_pushnumber(L, 1234);
                LuaDll.lua_pushstring(L, "Test");
                LuaDll.lua_pushthread(L);
                LuaDll.lua_newtable(L);
                int n = 0;
                Assert.False(LuaDll.lua_isfunction(L, n));
                Assert.False(LuaDll.lua_istable(L, n));
                Assert.False(LuaDll.lua_islightuserdata(L, n));
                Assert.True(LuaDll.lua_isnil(L, n));
                Assert.False(LuaDll.lua_isboolean(L, n));
                Assert.False(LuaDll.lua_isthread(L, n));
                Assert.False(LuaDll.lua_isnone(L, n));
                Assert.True(LuaDll.lua_isnoneornil(L, n));
                // nil
                n = 1;
                Assert.False(LuaDll.lua_isfunction(L, n));
                Assert.False(LuaDll.lua_istable(L, n));
                Assert.False(LuaDll.lua_islightuserdata(L, n));
                Assert.True(LuaDll.lua_isnil(L, n));
                Assert.False(LuaDll.lua_isboolean(L, n));
                Assert.False(LuaDll.lua_isthread(L, n));
                Assert.False(LuaDll.lua_isnone(L, n));
                Assert.True(LuaDll.lua_isnoneornil(L, n));
                // number
                n = 2;
                Assert.False(LuaDll.lua_isfunction(L, n));
                Assert.False(LuaDll.lua_istable(L, n));
                Assert.False(LuaDll.lua_islightuserdata(L, n));
                Assert.False(LuaDll.lua_isnil(L, n));
                Assert.False(LuaDll.lua_isboolean(L, n));
                Assert.False(LuaDll.lua_isthread(L, n));
                Assert.False(LuaDll.lua_isnone(L, n));
                Assert.False(LuaDll.lua_isnoneornil(L, n));
                // string
                n = 3;
                Assert.False(LuaDll.lua_isfunction(L, n));
                Assert.False(LuaDll.lua_istable(L, n));
                Assert.False(LuaDll.lua_islightuserdata(L, n));
                Assert.False(LuaDll.lua_isnil(L, n));
                Assert.False(LuaDll.lua_isboolean(L, n));
                Assert.False(LuaDll.lua_isthread(L, n));
                Assert.False(LuaDll.lua_isnone(L, n));
                Assert.False(LuaDll.lua_isnoneornil(L, n));
                // thread
                n = 4;
                Assert.False(LuaDll.lua_isfunction(L, n));
                Assert.False(LuaDll.lua_istable(L, n));
                Assert.False(LuaDll.lua_islightuserdata(L, n));
                Assert.False(LuaDll.lua_isnil(L, n));
                Assert.False(LuaDll.lua_isboolean(L, n));
                Assert.True(LuaDll.lua_isthread(L, n));
                Assert.False(LuaDll.lua_isnone(L, n));
                Assert.False(LuaDll.lua_isnoneornil(L, n));
                // table
                n = 5;
                Assert.False(LuaDll.lua_isfunction(L, n));
                Assert.True(LuaDll.lua_istable(L, n));
                Assert.False(LuaDll.lua_islightuserdata(L, n));
                Assert.False(LuaDll.lua_isnil(L, n));
                Assert.False(LuaDll.lua_isboolean(L, n));
                Assert.False(LuaDll.lua_isthread(L, n));
                Assert.False(LuaDll.lua_isnone(L, n));
                Assert.False(LuaDll.lua_isnoneornil(L, n));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pushliteral()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushliteral(L, null);
                LuaDll.lua_pushliteral(L, "Test");
                uint len;
                Assert.Equal(String.Empty, LuaDll.lua_tolstring(L, 1, out len));
                Assert.Equal("Test", LuaDll.lua_tolstring(L, 2, out len));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pushglobaltable()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushstring(L, "Global content");
                LuaDll.lua_setglobal(L, "GlobalVar");

                LuaDll.lua_pushglobaltable(L);
                Assert.Equal(LuaDll.LUA_TTABLE, LuaDll.lua_type(L, -1));

                uint len;
                LuaDll.lua_getfield(L, -1, "GlobalVar");
                Assert.Equal("Global content", LuaDll.lua_tolstring(L, -1, out len));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_rotate()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnumber(L, 1);
                LuaDll.lua_pushstring(L, "Test");
                LuaDll.lua_pushnumber(L, 2);
                LuaDll.lua_pushstring(L, "Text");
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushstring(L, "Toto");
                LuaDll.lua_pushnumber(L, 4);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                LuaDll.lua_rotate(L, 2, 1);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                Assert.Equal(1, LuaDll.lua_tonumber(L, 1));
                Assert.Equal(4, LuaDll.lua_tonumber(L, 2));
                Assert.Equal("Test", LuaDll.lua_tostring(L, 3));
                Assert.Equal(2, LuaDll.lua_tonumber(L, 4));
                Assert.Equal("Text", LuaDll.lua_tostring(L, 5));
                Assert.Equal(3, LuaDll.lua_tonumber(L, 6));
                Assert.Equal("Toto", LuaDll.lua_tostring(L, 7));

                LuaDll.lua_rotate(L, 2, 2);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                Assert.Equal(1, LuaDll.lua_tonumber(L, 1));
                Assert.Equal(3, LuaDll.lua_tonumber(L, 2));
                Assert.Equal("Toto", LuaDll.lua_tostring(L, 3));
                Assert.Equal(4, LuaDll.lua_tonumber(L, 4));
                Assert.Equal("Test", LuaDll.lua_tostring(L, 5));
                Assert.Equal(2, LuaDll.lua_tonumber(L, 6));
                Assert.Equal("Text", LuaDll.lua_tostring(L, 7));

                LuaDll.lua_rotate(L, 2, -3);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                Assert.Equal(1, LuaDll.lua_tonumber(L, 1));
                Assert.Equal("Test", LuaDll.lua_tostring(L, 2));
                Assert.Equal(2, LuaDll.lua_tonumber(L, 3));
                Assert.Equal("Text", LuaDll.lua_tostring(L, 4));
                Assert.Equal(3, LuaDll.lua_tonumber(L, 5));
                Assert.Equal("Toto", LuaDll.lua_tostring(L, 6));
                Assert.Equal(4, LuaDll.lua_tonumber(L, 7));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_insert()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnumber(L, 1);
                LuaDll.lua_pushstring(L, "Test");
                LuaDll.lua_pushnumber(L, 2);
                LuaDll.lua_pushstring(L, "Text");
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushstring(L, "Toto");
                LuaDll.lua_pushnumber(L, 4);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                LuaDll.lua_insert(L, 2);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                Assert.Equal(1, LuaDll.lua_tonumber(L, 1));
                Assert.Equal(4, LuaDll.lua_tonumber(L, 2));
                Assert.Equal("Test", LuaDll.lua_tostring(L, 3));
                Assert.Equal(2, LuaDll.lua_tonumber(L, 4));
                Assert.Equal("Text", LuaDll.lua_tostring(L, 5));
                Assert.Equal(3, LuaDll.lua_tonumber(L, 6));
                Assert.Equal("Toto", LuaDll.lua_tostring(L, 7));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_remove()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnumber(L, 1);
                LuaDll.lua_pushstring(L, "Test");
                LuaDll.lua_pushnumber(L, 2);
                LuaDll.lua_pushstring(L, "Text");
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushstring(L, "Toto");
                LuaDll.lua_pushnumber(L, 4);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                LuaDll.lua_remove(L, 2);
                Assert.Equal(6, LuaDll.lua_gettop(L));

                Assert.Equal(1, LuaDll.lua_tonumber(L, 1));
                Assert.Equal(2, LuaDll.lua_tonumber(L, 2));
                Assert.Equal("Text", LuaDll.lua_tostring(L, 3));
                Assert.Equal(3, LuaDll.lua_tonumber(L, 4));
                Assert.Equal("Toto", LuaDll.lua_tostring(L, 5));
                Assert.Equal(4, LuaDll.lua_tonumber(L, 6));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_replace()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnumber(L, 1);
                LuaDll.lua_pushstring(L, "Test");
                LuaDll.lua_pushnumber(L, 2);
                LuaDll.lua_pushstring(L, "Text");
                LuaDll.lua_pushnumber(L, 3);
                LuaDll.lua_pushstring(L, "Toto");
                LuaDll.lua_pushnumber(L, 4);
                Assert.Equal(7, LuaDll.lua_gettop(L));

                LuaDll.lua_replace(L, 2);
                Assert.Equal(6, LuaDll.lua_gettop(L));

                Assert.Equal(1, LuaDll.lua_tonumber(L, 1));
                Assert.Equal(4, LuaDll.lua_tonumber(L, 2));
                Assert.Equal(2, LuaDll.lua_tonumber(L, 3));
                Assert.Equal("Text", LuaDll.lua_tostring(L, 4));
                Assert.Equal(3, LuaDll.lua_tonumber(L, 5));
                Assert.Equal("Toto", LuaDll.lua_tostring(L, 6));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_unsigned()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushunsigned(L, 1234);
                LuaDll.lua_pushnumber(L, -1234);
                Assert.Equal(1234u, LuaDll.lua_tounsigned(L, 1));
                Assert.Equal(18446744073709550382u, LuaDll.lua_tounsigned(L, 2));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_luaL_tolstring()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                uint len;
                Assert.Equal("nil", LuaDll.luaL_tolstring(L, -1, out len));
                Assert.Equal(3u, len);

                LuaDll.lua_pushnil(L);
                Assert.Equal("nil", LuaDll.luaL_tolstring(L, -1, out len));
                Assert.Equal(3u, len);

                LuaDll.lua_pushnumber(L, 123.45);
                Assert.Equal("123.45", LuaDll.luaL_tolstring(L, -1, out len));
                Assert.Equal(6u, len);

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_luaL_checkversion()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_atpanic(L, state =>
                {
                    throw new ApplicationException(LuaDll.lua_tostring(state, -1));
                });

                LuaDll.luaL_checkversion(L);

                LuaDll.luaL_checkversion_(L, 503, LuaDll.LUAL_NUMSIZES);

                var ex = Assert.Throws<ApplicationException>(() => LuaDll.luaL_checkversion_(L, 502, LuaDll.LUAL_NUMSIZES));
                Assert.Equal("version mismatch: app. needs 502.0, Lua core provides 503.0", ex.Message);

                ex = Assert.Throws<ApplicationException>(() => LuaDll.luaL_checkversion_(L, 503, LuaDll.LUAL_NUMSIZES - 2));
                Assert.Equal("core and library have incompatible numeric types", ex.Message);

            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_luaL_loadfile()
        {
            String fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, @"
a=3
b=5
return a * b
");
            var L = LuaDll.luaL_newstate();
            try
            {
                Assert.Equal(LuaDll.LUA_OK, LuaDll.luaL_loadfile(L, fileName));
                Assert.Equal(1, LuaDll.lua_gettop(L));
                Assert.Equal(LuaDll.LUA_TFUNCTION, LuaDll.lua_type(L, -1));
                Assert.Equal(LuaDll.LUA_OK, LuaDll.lua_pcall(L, 0, LuaDll.LUA_MULTRET, 0));
                Assert.Equal(15, LuaDll.lua_tonumber(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [Fact]
        public void Test_luaL_newlibtable()
        {
            var regs = new LuaDll.luaL_Reg[]
            {
                    new LuaDll.luaL_Reg { func=state=>
                    {
                        LuaDll.lua_pushstring(state, "Hello");
                        return 1;
                    }, name= "hello" },
                    new LuaDll.luaL_Reg { func=state=>
                    {
                        LuaDll.lua_pushstring(state, DateTime.Now.ToString());
                        return 1;
                    }, name= "today" },
                    new LuaDll.luaL_Reg {func=null, name=null }
            };

            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.luaL_newlibtable(L, regs);
                Assert.Equal(LuaDll.LUA_TTABLE, LuaDll.lua_type(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_luaL_newlib()
        {
            var regs = new LuaDll.luaL_Reg[]
            {
                    new LuaDll.luaL_Reg { func=state=>
                    {
                        LuaDll.lua_pushstring(state, "Hello");
                        return 1;
                    }, name= "hello" },
                    new LuaDll.luaL_Reg { func=state=>
                    {
                        LuaDll.lua_pushstring(state, DateTime.Now.ToString());
                        return 1;
                    }, name= "today" },
                    new LuaDll.luaL_Reg {func=null, name=null }
            };

            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_atpanic(L, state =>
                {
                    throw new ApplicationException(LuaDll.lua_tostring(state, -1));
                });

                LuaDll.luaL_newlib(L, regs);
                Assert.Equal(LuaDll.LUA_TTABLE, LuaDll.lua_type(L, -1));
                Assert.Equal(LuaDll.LUA_TFUNCTION, LuaDll.lua_getfield(L, 1, "hello"));
                LuaDll.lua_call(L, 0, 1);
                Assert.Equal("Hello", LuaDll.lua_tostring(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_luaL_typename()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushnil(L);
                Assert.Equal("nil", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_pushboolean(L, 1);
                Assert.Equal("boolean", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_pushlightuserdata(L, new IntPtr(12345));
                Assert.Equal("userdata", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_pushnumber(L, 1);
                Assert.Equal("number", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_pushstring(L, "text");
                Assert.Equal("string", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_pushglobaltable(L);
                Assert.Equal("table", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_pushcfunction(L, st => 0);
                Assert.Equal("function", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_newuserdata(L, 100);
                Assert.Equal("userdata", LuaDll.luaL_typename(L, -1));
                LuaDll.lua_newthread(L);
                Assert.Equal("thread", LuaDll.luaL_typename(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }

        [Fact]
        public void Test_lua_pushfstring()
        {
            var L = LuaDll.luaL_newstate();
            try
            {
                LuaDll.lua_pushfstring(L, "Fmt:");
                Assert.Equal("Fmt:", LuaDll.lua_tostring(L, -1));

                LuaDll.lua_pushfstring(L, "Fmt: %f", 123.45);
                Assert.Equal("Fmt: 123.45", LuaDll.lua_tostring(L, -1));

                LuaDll.lua_pushfstring(L, "Fmt: %d", 123);
                Assert.Equal("Fmt: 123", LuaDll.lua_tostring(L, -1));

                LuaDll.lua_pushfstring(L, "Fmt: %s - %f", "Value", 123.45);
                Assert.Equal("Fmt: Value - 123.45", LuaDll.lua_tostring(L, -1));
            }
            finally
            {
                LuaDll.lua_close(L);
            }
        }
    }

}
