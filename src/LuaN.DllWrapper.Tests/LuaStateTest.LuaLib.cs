using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    partial class LuaStateTest
    {

        [Fact]
        public void TestLuaOpenBase()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenBase());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetGlobal("assert"));
            }
        }

        [Fact]
        public void TestLuaOpenCoroutine()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenCoroutine());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "create"));
            }
        }

        [Fact]
        public void TestLuaOpenTable()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenTable());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "concat"));
            }
        }

        [Fact]
        public void TestLuaOpenIo()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenIo());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "flush"));
            }
        }

        [Fact]
        public void TestLuaOpenOs()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenOs());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "clock"));
            }
        }

        [Fact]
        public void TestLuaOpenString()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenString());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "format"));
            }
        }

        [Fact]
        public void TestLuaOpenUtf8()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenUtf8());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "codepoint"));
            }
        }

        [Fact]
        public void TestLuaOpenBit32()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenBit32());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "band"));
            }
        }

        [Fact]
        public void TestLuaOpenMath()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenMath());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "acos"));
            }
        }

        [Fact]
        public void TestLuaOpenDebug()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenDebug());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "debug"));
            }
        }

        [Fact]
        public void TestLuaOpenPackage()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(1, L.LuaOpenPackage());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(LuaType.Function, L.LuaGetField(-1, "loadlib"));
            }
        }

        [Fact]
        public void TestLuaOpenLibs()
        {
            using (var L = new LuaState())
            {
                L.LuaOpenLibs();
                Assert.Equal(LuaType.Function, L.LuaGetGlobal("assert"));
                Assert.Equal(LuaType.Table, L.LuaGetGlobal("coroutine"));
                Assert.Equal(LuaType.Table, L.LuaGetGlobal("string"));
            }
        }

    }
}
