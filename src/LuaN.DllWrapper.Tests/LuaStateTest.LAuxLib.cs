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
        public void TestLuaLNewMetatable()
        {
            using (var L=new LuaState())
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
        public void TestLuaLError()
        {
            using (var L=new LuaState())
            {
                var ex = Assert.Throws<LuaException>(() => L.LuaLError("Error message"));
                Assert.Equal("Error message", ex.Message);
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

    }
}
