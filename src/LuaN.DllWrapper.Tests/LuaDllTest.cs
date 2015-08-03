using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaDllTest
    {
        [Fact]
        public void TestConstants()
        {
            Assert.Equal("Lua 5.3.0  Copyright (C) 1994-2015 Lua.org, PUC-Rio", LuaDll.LUA_COPYRIGHT);
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

    }
}
