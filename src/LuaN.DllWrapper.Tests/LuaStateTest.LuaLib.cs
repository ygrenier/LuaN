using System;
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public void TestLuaPrint()
        {
            var oldStdout = Console.Out;
            StringBuilder stdout = new StringBuilder();
            Console.SetOut(new StringWriter(stdout));
            try
            {
                using (var L = new LuaState())
                {
                    List<String> output = new List<string>();

                    Assert.Equal(LuaStatus.ErrorRun, L.DoString("print('First line', 123.45, false)"));
                    L.LuaOpenLibs();

                    Assert.Equal(LuaStatus.Ok, L.DoString("print('First line', 123.45, false)"));

                    bool doHandled = false;
                    L.OnPrint += (s, e) =>
                    {
                        output.Add("P:" + e.Text);
                        e.Handled = doHandled;
                    };
                    L.OnWriteString += (s, e) =>
                    {
                        output.Add("W:" + e.Text);
                        e.Handled = doHandled;
                    };
                    L.OnWriteLine += (s, e) =>
                    {
                        output.Add("WL:" + e.Text);
                        e.Handled = doHandled;
                    };

                    Assert.Equal(LuaStatus.Ok, L.DoString("print('Second line', 987.65, true)"));

                    doHandled = true;

                    Assert.Equal(LuaStatus.Ok, L.DoString("print('Third line', 555)"));

                    Assert.Equal(
                        "First line\t123.45\tfalse" + Environment.NewLine
                        + "Second line\t987.65\ttrue" + Environment.NewLine
                        , stdout.ToString());
                    Assert.Equal(new String[]
                    {
                        "P:Second line\t987.65\ttrue",
                        "W:Second line\t987.65\ttrue",
                        "WL:"+Environment.NewLine,
                        "P:Third line\t555",
                    }, output);

                    // Test with an error while a print
                    L.LuaPushLightUserData(this);
                    L.LuaPushValue(-1);
                    L.LuaSetGlobal("ud");
                    L.LuaNewTable();
                    L.LuaPushCFunction(state =>
                    {
                        state.LuaPushString(null);
                        return 1;
                    });
                    L.LuaSetField(-2, "__tostring");
                    L.LuaSetMetatable(-2);
                    Assert.Equal(LuaStatus.ErrorRun, L.DoString("print(ud)"));
                    Assert.Equal("[string \"print(ud)\"]:1: 'tostring' must return a string to 'print'", L.LuaToString(-1));

                }
            }
            finally
            {
                Console.SetOut(oldStdout);
            }
        }

    }
}
