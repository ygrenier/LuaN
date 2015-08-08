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
        public void TestLuaLLoadFile()
        {
            String file = System.IO.Path.GetTempFileName();
            using (var L = new LuaState())
            {
                System.IO.File.WriteAllText(file, "return 2*5");
                Assert.Equal(LuaStatus.Ok, L.LuaLLoadFile(file));
                Assert.Equal(LuaType.Function, L.LuaType(-1));
                Assert.Equal(null, L.LuaToCFunction(-1));
                L.LuaCall(0, 1);
                Assert.Equal(10d, L.LuaToNumber(-1));

                System.IO.File.WriteAllText(file, "return 2*");
                Assert.Equal(LuaStatus.ErrorSyntax, L.LoadFile(file));
                Assert.Equal(file + ":1: unexpected symbol near <eof>", L.LuaToString(-1));
            }
            System.IO.File.Delete(file);
        }

        [Fact]
        public void TestLuaLLoadString()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(LuaStatus.Ok, L.LuaLLoadString("return 2*5"));
                Assert.Equal(LuaType.Function, L.LuaType(-1));
                Assert.Equal(null, L.LuaToCFunction(-1));
                L.LuaCall(0, 1);
                Assert.Equal(10d, L.LuaToNumber(-1));

                Assert.Equal(LuaStatus.ErrorSyntax, L.LoadString("return 2*"));
                Assert.Equal("[string \"return 2*\"]:1: unexpected symbol near <eof>", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaLDoFile()
        {
            String file = System.IO.Path.GetTempFileName();
            using (var L = new LuaState())
            {
                System.IO.File.WriteAllText(file, "return 2*5");
                Assert.Equal(LuaStatus.Ok, L.LuaLDoFile(file));
                Assert.Equal(10d, L.LuaToNumber(-1));

                System.IO.File.WriteAllText(file, "return 2*");
                Assert.Equal(LuaStatus.ErrorSyntax, L.DoFile(file));
                Assert.Equal(file + ":1: unexpected symbol near <eof>", L.LuaToString(-1));
            }
            System.IO.File.Delete(file);
        }

        [Fact]
        public void TestLuaLDoString()
        {
            using (var L = new LuaState())
            {
                Assert.Equal(LuaStatus.Ok, L.LuaLDoString("return 2*5"));
                Assert.Equal(10d, L.LuaToNumber(-1));

                Assert.Equal(LuaStatus.ErrorSyntax, L.DoString("return 2*"));
                Assert.Equal("[string \"return 2*\"]:1: unexpected symbol near <eof>", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaRef()
        {
            using (var L = new LuaState())
            {
                // Table to store
                L.LuaNewTable();

                // Table to reference
                L.LuaNewTable();
                L.LuaPushValue(2);  // Duplicate for two references
                var tref1 = L.LuaLRef(1);
                Assert.Equal(1, tref1);
                var tref2 = L.LuaLRef(1);
                Assert.Equal(2, tref2);
                Assert.NotEqual(tref1, tref2);

                Assert.Equal(1, L.LuaGetTop());

                L.LuaRawGetI(1, tref1);
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                L.LuaRawGetI(1, tref2);
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(true, L.LuaRawEqual(2, 3));
                L.LuaPop(2);

                L.LuaLUnref(1, tref2);
                L.LuaRawGetI(1, tref1);
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                L.LuaRawGetI(1, tref2);
                Assert.Equal(LuaType.Nil, L.LuaType(-1));
                Assert.Equal(false, L.LuaRawEqual(2, 3));
                L.LuaPop(2);

                L.LuaPushNil();
                Assert.Equal(LuaRef.RefNil, L.LuaLRef(1));
            }
        }

    }
}
