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
        public void TestLuaLLoadFileX()
        {
            String file = System.IO.Path.GetTempFileName();
            using (var L = new LuaState())
            {
                System.IO.File.WriteAllText(file, "return 2*5");
                Assert.Equal(LuaStatus.Ok, L.LuaLLoadFileX(file, "t"));
                Assert.Equal(LuaType.Function, L.LuaType(-1));
                Assert.Equal(null, L.LuaToCFunction(-1));
                L.LuaCall(0, 1);
                Assert.Equal(10d, L.LuaToNumber(-1));

                System.IO.File.WriteAllText(file, "return 2*");
                Assert.Equal(LuaStatus.ErrorSyntax, L.LuaLLoadFileX(file, "t"));
                Assert.Equal(file + ":1: unexpected symbol near <eof>", L.LuaToString(-1));
            }
            System.IO.File.Delete(file);
        }

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
                Assert.Equal(LuaStatus.ErrorSyntax, L.LuaLoadFile(file));
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

                Assert.Equal(LuaStatus.ErrorSyntax, L.LuaLoadString("return 2*"));
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
                Assert.Equal(LuaStatus.ErrorSyntax, L.LuaDoFile(file));
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

                Assert.Equal(LuaStatus.ErrorSyntax, L.LuaDoString("return 2*"));
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

        [Fact]
        public void TestLuaLCheckVersion()
        {
            using (var L = new LuaState())
            {
                L.LuaLCheckVersion();
            }
        }

        [Fact]
        public void TestLuaLGetMetaField()
        {
            using (var L = new LuaState())
            {
                // Create a table
                L.LuaNewTable();
                L.LuaPushString("Field Value");
                L.LuaSetField(1, "TableField");
                L.LuaPushString("Another Value");
                L.LuaSetField(1, "NoMetaField");

                // Create the metatable
                L.LuaNewTable();
                L.LuaPushString("Field MetaValue");
                L.LuaSetField(2, "TableField");
                L.LuaSetMetatable(1);

                Assert.Equal(1, L.LuaGetTop());

                // Get field by metatable
                Assert.Equal(LuaType.String, L.LuaLGetMetaField(1, "TableField"));
                Assert.Equal("Field MetaValue", L.LuaToString(-1));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(LuaType.Nil, L.LuaLGetMetaField(1, "NoMetaField"));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(LuaType.Nil, L.LuaLGetMetaField(1, "AnotherMetaField"));
                Assert.Equal(2, L.LuaGetTop());

            }
        }

        [Fact]
        public void TestLuaLGetCallMeta()
        {
            using (var L = new LuaState())
            {
                // Create a table
                L.LuaNewTable();
                L.LuaPushCFunction(state =>
                {
                    state.LuaPushString("Result call");
                    return 1;
                });
                L.LuaSetField(1, "TableCall");
                L.LuaPushCFunction(state =>
                {
                    state.LuaPushString("Another Result call");
                    return 1;
                });
                L.LuaSetField(1, "NoMetaCall");
                L.LuaPushNumber(123);
                L.LuaSetField(1, "NoCallableField");
                L.LuaPushNumber(123);
                L.LuaSetField(1, "NoMetaCallableField");

                // Create the metatable
                L.LuaNewTable();
                L.LuaPushCFunction(state =>
                {
                    state.LuaPushString("Result meta call");
                    return 1;
                });
                L.LuaSetField(2, "TableCall");
                L.LuaSetMetatable(1);
                L.LuaPushNumber(987);
                L.LuaSetField(1, "NoCallableField");

                Assert.Equal(1, L.LuaGetTop());

                // Call by metatable
                Assert.Equal(true, L.LuaLCallMeta(1, "TableCall"));
                Assert.Equal("Result meta call", L.LuaToString(-1));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(false, L.LuaLCallMeta(1, "NoMetaCall"));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(false, L.LuaLCallMeta(1, "NoCallableField"));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(false, L.LuaLCallMeta(1, "NoMetaCallableField"));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(false, L.LuaLCallMeta(1, "AnotherCall"));
                Assert.Equal(2, L.LuaGetTop());

            }
        }

        [Fact]
        public void TestLuaLToLString()
        {
            using (var L = new LuaState())
            {
                L.LuaLOpenLibs();

                uint len;
                L.LuaPushNumber(1234);
                Assert.Equal("1234.0", L.LuaLToLString(-1, out len));
                Assert.Equal("1234.0", L.LuaToLString(-1, out len));
                L.LuaGetGlobal("_G");
                Assert.StartsWith("table: ", L.LuaLToLString(-1, out len));
                Assert.StartsWith("table: ", L.LuaToLString(-1, out len));
            }
        }

        [Fact]
        public void TestLuaLCheckStack()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                L.LuaLCheckStack(2, "Failed 1");
                Assert.Equal(0, L.LuaGetTop());
                L.LuaLCheckStack(35, "Failed 2");
                var lex = Assert.Throws<LuaException>(() => L.LuaLCheckStack(1000000, "Failed 3"));
                Assert.Equal("stack overflow (Failed 3)", lex.Message);
                lex = Assert.Throws<LuaException>(() => L.LuaLCheckStack(1000000, null));
                Assert.Equal("stack overflow", lex.Message);
            }
        }

        [Fact]
        public void TestLuaLNewMetatable()
        {
            using (var L = new LuaState())
            {
                Assert.True(L.LuaLNewMetatable("meta1"));
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.String, L.LuaGetField(1, "__name"));
                Assert.Equal("meta1", L.LuaToString(-1));
                L.LuaPop(1);

                Assert.Equal(LuaType.Table, L.LuaGetField(L.RegistryIndex, "meta1"));
                Assert.Equal(true, L.LuaRawEqual(1, 2));
                L.LuaPop(1);

                // Fail to create a new metatable with the same name, but push the existing metatable on the stack
                Assert.False(L.LuaLNewMetatable("meta1"));
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaRawEqual(1, 2));

                Assert.Throws<ArgumentNullException>(() => L.LuaLNewMetatable("  "));
            }
        }

        [Fact]
        public void TestLuaLSetMetatable()
        {
            using (var L = new LuaState())
            {
                Assert.True(L.LuaLNewMetatable("meta1"));

                L.LuaNewTable();
                L.LuaLSetMetatable("meta1");
                Assert.True(L.LuaGetMetatable(-1));
                Assert.Equal(true, L.LuaRawEqual(1, -1));

                L.LuaNewTable();
                L.LuaLSetMetatable("meta2");
                Assert.False(L.LuaGetMetatable(-1));

                Assert.Throws<ArgumentNullException>(() => L.LuaLSetMetatable("  "));
            }
        }

        [Fact]
        public void TestLuaLGetMetatable()
        {
            using (var L = new LuaState())
            {
                Assert.True(L.LuaLNewMetatable("meta1"));

                Assert.Equal(LuaType.Table, L.LuaLGetMetatable("meta1"));
                Assert.Equal(true, L.LuaRawEqual(1, -1));

                Assert.Equal(LuaType.Nil, L.LuaLGetMetatable("meta2"));

                Assert.Throws<ArgumentNullException>(() => L.LuaLGetMetatable("  "));
            }
        }

        [Fact]
        public void TestLuaLError()
        {
            using (var L = new LuaState())
            {
                var ex = Assert.Throws<LuaException>(() => L.LuaLError("Error message"));
                Assert.Equal("Error message", ex.Message);

                ex = Assert.Throws<LuaException>(() => L.LuaLError("Error message : %s", "That's an error"));
                Assert.Equal("Error message : That's an error", ex.Message);

                ex = Assert.Throws<LuaException>(() => L.LuaLError("Error message (%s): %s", "test", "That's an error"));
                Assert.Equal("Error message (test): That's an error", ex.Message);

            }
        }

        [Fact]
        public void TestLuaWriteString_LuaWriteLine()
        {
            var oldStdout = Console.Out;
            StringBuilder stdout = new StringBuilder();
            Console.SetOut(new StringWriter(stdout));
            try
            {
                using (var L = new LuaState())
                {
                    List<String> output = new List<string>();
                    L.LuaWriteString("First string");
                    L.LuaWriteLine();

                    bool doHandled = false;
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

                    L.LuaWriteString("Second string");
                    L.LuaWriteLine();

                    doHandled = true;

                    L.LuaWriteString("Third string");
                    L.LuaWriteLine();

                    Assert.Equal("First string" + Environment.NewLine + "Second string" + Environment.NewLine, stdout.ToString());
                    Assert.Equal(new String[]
                    {
                        "W:Second string",
                        "WL:"+Environment.NewLine,
                        "W:Third string",
                        "WL:"+Environment.NewLine
                    }, output);
                }
            }
            finally
            {
                Console.SetOut(oldStdout);
            }
        }

        [Fact]
        public void TestLuaWriteStringError()
        {
            var oldStderr = Console.Error;
            StringBuilder stderr = new StringBuilder();
            Console.SetError(new StringWriter(stderr));
            try
            {
                using (var L = new LuaState())
                {
                    List<String> output = new List<string>();
                    L.LuaWriteStringError("%s error", "First");

                    bool doHandled = false;
                    L.OnWriteStringError += (s, e) =>
                    {
                        output.Add("E:" + e.Text);
                        e.Handled = doHandled;
                    };

                    L.LuaWriteStringError("%s error", "Second");

                    doHandled = true;

                    L.LuaWriteStringError("%s error", "Third");

                    Assert.Equal("First errorSecond error", stderr.ToString());
                    Assert.Equal(new String[]
                    {
                        "E:Second error",
                        "E:Third error",
                    }, output);
                }
            }
            finally
            {
                Console.SetError(oldStderr);
            }
        }

        [Fact]
        public void TestLuaAsset()
        {
            using (var L = new LuaState())
            {
                L.LuaAssert(true);

                //Assert.Throws<Exception>(() => L.LuaAssert(false));
            }
        }

        [Fact]
        public void TestLuaLLen()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Exception ex = Assert.Throws<LuaException>(() => L.LuaLLen(1));
                Assert.Equal("attempt to get length of a nil value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(2));
                Assert.Equal("attempt to get length of a number value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(3));
                Assert.Equal("attempt to get length of a number value", ex.Message);
                Assert.Equal(4u, L.LuaLLen(4));
                Assert.Equal(3u, L.LuaLLen(5));
                Assert.Equal(2u, L.LuaLLen(6));
                Assert.Equal(2u, L.LuaLLen(7));
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(8));
                Assert.Equal("attempt to get length of a boolean value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(9));
                Assert.Equal("attempt to get length of a function value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(10));
                Assert.Equal("attempt to get length of a function value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(11));
                Assert.Equal("attempt to get length of a userdata value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(12));
                Assert.Equal("attempt to get length of a userdata value", ex.Message);
                L.LuaLLen(13);
                Assert.Equal(0u, L.LuaToNumber(-1));
                ex = Assert.Throws<LuaException>(() => L.LuaLLen(14));
                Assert.Equal("attempt to get length of a thread value", ex.Message);

                // Test with metamethod __len
                L.LuaNewTable();
                L.LuaPushCFunction(state =>
                {
                    state.LuaPushNumber(1234);
                    return 1;
                });
                L.LuaSetField(-2, "__len");
                L.LuaSetMetatable(11);
                Assert.Equal(1234u, L.LuaLLen(11));

            }

        }

        [Fact]
        public void TestLuaLTraceback()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaLOpenLibs();
                L.LuaRegister("TestTrace", state =>
                {
                    state.LuaLTraceback(state, "The Call Trace", 0);
                    Assert.Equal(String.Join("\n", new String[] {
                        "The Call Trace",
                        "stack traceback:",
                        "\t[C]: in function 'TestTrace'",
                        "\t[string \"\r...\"]:3: in function 'f1'",
                        "\t[string \"\r...\"]:6: in function 'f2'",
                        "\t[string \"\r...\"]:8: in main chunk",
                    }), state.LuaToString(-1));

                    state.LuaLTraceback(state, null, 1);
                    Assert.Equal(String.Join("\n", new String[] {
                        "stack traceback:",
                        "\t[string \"\r...\"]:3: in function 'f1'",
                        "\t[string \"\r...\"]:6: in function 'f2'",
                        "\t[string \"\r...\"]:8: in main chunk",
                    }), state.LuaToString(-1));

                    state.LuaLTraceback(state, null, 3);
                    Assert.Equal(String.Join("\n", new String[] {
                        "stack traceback:",
                        "\t[string \"\r...\"]:8: in main chunk",
                    }), state.LuaToString(-1));

                    return 0;
                });
                L.LuaDoString(@"
function f1()
 TestTrace()
end
function f2()
 f1()
end
f2()
");
            }

        }

        [Fact]
        public void TestLuaLTypeName()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal("nil", L.LuaLTypeName(1));
                Assert.Equal("number", L.LuaLTypeName(2));
                Assert.Equal("number", L.LuaLTypeName(3));
                Assert.Equal("string", L.LuaLTypeName(4));
                Assert.Equal("string", L.LuaLTypeName(5));
                Assert.Equal("string", L.LuaLTypeName(6));
                Assert.Equal("string", L.LuaLTypeName(7));
                Assert.Equal("boolean", L.LuaLTypeName(8));
                Assert.Equal("function", L.LuaLTypeName(9));
                Assert.Equal("function", L.LuaLTypeName(10));
                Assert.Equal("userdata", L.LuaLTypeName(11));
                Assert.Equal("userdata", L.LuaLTypeName(12));
                Assert.Equal("table", L.LuaLTypeName(13));
                Assert.Equal("thread", L.LuaLTypeName(14));
            }
        }

        [Fact]
        public void TestLuaLLoadBuffer()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(LuaStatus.Ok, L.LuaLoadBuffer(Encoding.ASCII.GetBytes("return 2*7"), "name", "t"));
                Assert.Equal(LuaType.Function, L.LuaType(-1));
                L.LuaPushValue(-1);
                L.LuaCall(0, 1);
                Assert.Equal(14, L.LuaToNumber(-1));
                L.LuaPop(1);

                byte[] buffBytes;
                using (var ms = new MemoryStream())
                {
                    LuaWriter wrt = (state, p, ud) =>
                     {
                         ms.Write(p, 0, p.Length);
                         return 0;
                     };
                    L.LuaDump(wrt, null, true);
                    buffBytes = ms.ToArray();
                }

                Assert.Equal(LuaStatus.Ok, L.LuaLoadBuffer(buffBytes, null));
                Assert.Equal(LuaType.Function, L.LuaType(-1));
                L.LuaCall(0, 1);
                Assert.Equal(14, L.LuaToNumber(-1));

            }
        }

        [Fact]
        public void TestLuaLRequireF()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaLOpenLibs();

                int count = 0;
                LuaCFunction openf = s =>
                {
                    s.LuaNewTable();
                    count++;
                    return 1;
                };
                L.LuaLRequireF("module", openf, true);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(1, count);

                L.LuaLRequireF("module", openf, true);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
                Assert.Equal(1, count);

            }
        }

    }
}
