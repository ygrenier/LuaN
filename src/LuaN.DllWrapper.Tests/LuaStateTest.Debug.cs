using Moq;
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
        public void TestNewLuaDebug()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                using (var db = L.NewLuaDebug())
                {
                    Assert.NotNull(db);
                    Assert.Null(db.Name);
                    Assert.Null(db.What);
                    Assert.Null(db.NameWhat);
                    Assert.Null(db.ShortSource);
                    Assert.Null(db.Source);
                    Assert.Equal(-1, db.CurrentLine);
                    Assert.Equal(LuaHookEvent.HookCall, db.Event);
                    Assert.Equal(-1, db.LineDefined);
                    Assert.Equal(-1, db.LastLineDefined);
                    Assert.Equal(false, db.IsTailCall);
                    Assert.Equal(false, db.IsVarArg);
                    Assert.Equal(0, db.NParams);
                    Assert.Equal(0, db.NUps);
                }
            }
        }

        [Fact]
        public void TestLuaGetStack()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        Assert.True(l.LuaGetStack(0, ar));

                        Assert.Null(ar.Name);
                        Assert.Null(ar.What);
                        Assert.Null(ar.NameWhat);
                        Assert.Null(ar.ShortSource);
                        Assert.Null(ar.Source);
                        Assert.Equal(-1, ar.CurrentLine);
                        //Assert.NotEqual(LuaHookEvent.HookCall, ar.Event);
                        Assert.Equal(-1, ar.LineDefined);
                        Assert.Equal(-1, ar.LastLineDefined);
                        Assert.Equal(false, ar.IsTailCall);
                        Assert.Equal(false, ar.IsVarArg);
                        Assert.Equal(0, ar.NParams);
                        Assert.Equal(0, ar.NUps);

                        Assert.False(l.LuaGetStack(1, ar));

                        // TODO Add test from lua code
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                Assert.Throws<ArgumentNullException>(() => L.LuaGetStack(0, null));

                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaGetStack(0, mDebug.Object));
            }
        }

        [Fact]
        public void TestLuaGetInfo()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        Assert.True(l.LuaGetStack(0, ar));
                        Assert.True(l.LuaGetInfo(LuaGetInfoWhat.AllFills, ar));

                        Assert.Null(ar.Name);
                        Assert.Equal("C", ar.What);
                        Assert.Equal("", ar.NameWhat);
                        Assert.Equal("[C]", ar.ShortSource);
                        Assert.Equal("=[C]", ar.Source);
                        Assert.Equal(-1, ar.CurrentLine);
                        //Assert.NotEqual(LuaHookEvent.HookCall, ar.Event);
                        Assert.Equal(-1, ar.LineDefined);
                        Assert.Equal(-1, ar.LastLineDefined);
                        Assert.Equal(false, ar.IsTailCall);
                        Assert.Equal(true, ar.IsVarArg);
                        Assert.Equal(0, ar.NParams);
                        Assert.Equal(0, ar.NUps);

                        Assert.Equal(0, l.LuaGetTop());
                        Assert.True(l.LuaGetInfo(LuaGetInfoWhat.AllFills | LuaGetInfoWhat.FromTopOfStack | LuaGetInfoWhat.PushFunction | LuaGetInfoWhat.PushLines, ar));
                        Assert.Equal(1, l.LuaGetTop());
                        Assert.Equal(LuaType.Nil, l.LuaType(-1));

                        // TODO Add test from Lua code
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                Assert.Throws<ArgumentNullException>(() => L.LuaGetInfo(LuaGetInfoWhat.AllFills, null));

                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaGetInfo(LuaGetInfoWhat.AllFills, mDebug.Object));
            }
        }

        [Fact]
        public void TestLuaGetLocal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        L.LuaGetStack(0, ar);
                        Assert.Equal(null, l.LuaGetLocal(ar, 1));
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                // TODO Add test from Lua code

                L.LuaLDoString(@"
function test(a,b)
    local c = a + b
    return a, b, c
end
                ");
                // Local names
                L.LuaGetGlobal("test");
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(null, L.LuaGetLocal(null, 0));
                Assert.Equal("a", L.LuaGetLocal(null, 1));
                Assert.Equal("b", L.LuaGetLocal(null, 2));
                Assert.Equal(null, L.LuaGetLocal(null, 3));
                Assert.Equal(null, L.LuaGetLocal(null, 4));
                Assert.Equal(1, L.LuaGetTop());

                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaGetLocal(mDebug.Object, 0));
            }
        }

        [Fact]
        public void TestLuaSetLocal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        L.LuaGetStack(0, ar);
                        L.LuaPushNumber(123);
                        Assert.Equal("(*temporary)", l.LuaSetLocal(ar, 1));
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                // TODO Add test from lua code


                Assert.Throws<ArgumentNullException>(() => L.LuaSetLocal(null, 0));
                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaSetLocal(mDebug.Object, 0));

            }
        }

        [Fact]
        public void TestLuaGetUpvalue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(23);
                L.LuaPushCFunction(l =>
                {
                    Assert.Equal(null, l.LuaGetUpvalue(0, 0));
                    return 0;
                });
                L.LuaPCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestSetUpvalue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(23);
                L.LuaPushCFunction(l =>
                {
                    Assert.Equal(null, l.LuaSetUpvalue(0, 0));
                    return 0;
                });
                L.LuaPCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestLuaUpvalueId()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(23);
                L.LuaPushCFunction(l =>
                {
                    Assert.Equal(0, l.LuaUpvalueId(0, 0));
                    return 0;
                });
                L.LuaPCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestLuaUpvalueJoin()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(23);
                L.LuaPushCFunction(l =>
                {
                    // TODO To test
                    //l.LuaUpvalueJoin(0, 0, 0, 0);
                    return 0;
                });
                L.LuaPCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestLuaGetSetHook()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaHook hook = (ILuaState l, ILuaDebug ar) =>
                {
                };

                L.LuaSetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, 1);
                Assert.Same(hook, L.LuaGetHook());
                Assert.Equal(LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, L.LuaGetHookMask());
                Assert.Equal(1, L.LuaGetHookCount());

                L.LuaSetHook(hook, LuaHookMask.None, 1);
                Assert.Same(null, L.LuaGetHook());
                Assert.Equal(LuaHookMask.None, L.LuaGetHookMask());
                Assert.Equal(1, L.LuaGetHookCount());

            }
        }

//        [Fact]
//        public void TestDebugHook()
//        {
//            using (ILuaState L = new LuaState())
//            {
//                List<String> output = new List<string>();
//                L.OnPrint += (s, e) => output.Add(e.Text);
//                L.OpenLibs();

//                Assert.Null(L.GetHook());
//                Assert.Equal(0, L.GetHookCount());
//                Assert.Equal(LuaHookMask.None, L.GetHookMask());

//                LuaHook hook = (state, dg) =>
//                {
//                    state.GetInfo(LuaGetInfoWhat.AllFills, dg);
//                    output.Add(String.Format("Hook ({0}) Line {1} : {2} ({3}). {4}.", dg.Event, dg.CurrentLine, dg.What, dg.NameWhat, dg.Name));
//                };

//                L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskRet, 0);

//                Assert.Same(hook, L.GetHook());
//                Assert.Equal(0, L.GetHookCount());
//                Assert.Equal(LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskRet, L.GetHookMask());

//                L.DoString(@"print('start')

//function test(a,b)
// local c = a + b
// print('test ', a, b, c)
//end

//print(
//  'Call1',
//  test(1,2)
//)

//print(
//  'Call2',
//  test(23,17)
//)

//print('end')

//");
//                L.SetHook(null, LuaHookMask.None, 0);
//                Assert.Null(L.GetHook());
//                Assert.Equal(0, L.GetHookCount());
//                Assert.Equal(LuaHookMask.None, L.GetHookMask());

//                L.DoString(@"print('test after end')");

//                int sk = 0;
//                Assert.Equal(67, output.Count);
//                Assert.Equal(new String[]
//                {
//                    // print('start')
//                    "Hook (HookCall) Line 1 : main (). .",
//                    "Hook (HookLine) Line 1 : main (). .",
//                    "Hook (HookCall) Line -1 : C (global). print.",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "start",
//                    "Hook (HookRet) Line -1 : C (global). print.",
//                    // function
//                    "Hook (HookLine) Line 6 : main (). .",
//                    "Hook (HookLine) Line 3 : main (). .",
//                    // first print
//                    "Hook (HookLine) Line 8 : main (). .",
//                    "Hook (HookLine) Line 9 : main (). .",
//                    "Hook (HookLine) Line 10 : main (). .",
//                    "Hook (HookCall) Line 4 : Lua (global). test.",
//                    "Hook (HookLine) Line 4 : Lua (global). test.",
//                    "Hook (HookLine) Line 5 : Lua (global). test.",
//                    "Hook (HookCall) Line -1 : C (global). print.",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "test 	1	2	3",
//                    "Hook (HookRet) Line -1 : C (global). print.",
//                    "Hook (HookLine) Line 6 : Lua (global). test.",
//                    "Hook (HookRet) Line 6 : Lua (global). test.",
//                    "Hook (HookLine) Line 8 : main (). .",
//                    "Hook (HookCall) Line -1 : C (global). print.",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Call1",
//                    "Hook (HookRet) Line -1 : C (global). print.",
//                    // Second print
//                    "Hook (HookLine) Line 13 : main (). .",
//                    "Hook (HookLine) Line 14 : main (). .",
//                    "Hook (HookLine) Line 15 : main (). .",
//                    "Hook (HookCall) Line 4 : Lua (global). test.",
//                    "Hook (HookLine) Line 4 : Lua (global). test.",
//                    "Hook (HookLine) Line 5 : Lua (global). test.",
//                    "Hook (HookCall) Line -1 : C (global). print.",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "test 	23	17	40",
//                    "Hook (HookRet) Line -1 : C (global). print.",
//                    "Hook (HookLine) Line 6 : Lua (global). test.",
//                    "Hook (HookRet) Line 6 : Lua (global). test.",
//                    "Hook (HookLine) Line 13 : main (). .",
//                    "Hook (HookCall) Line -1 : C (global). print.",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "Call2",
//                    "Hook (HookRet) Line -1 : C (global). print.",
//                    "Hook (HookLine) Line 18 : main (). .",
//                    // print('end')
//                    "Hook (HookCall) Line -1 : C (global). print.",
//                    "Hook (HookCall) Line -1 : C (). .",
//                    "Hook (HookRet) Line -1 : C (). .",
//                    "end",
//                    "Hook (HookRet) Line -1 : C (global). print.",
//                    // end of chunk
//                    "Hook (HookRet) Line 18 : main (). .",
//                    // after end
//                    "test after end"
//                }.Skip(sk), output.Skip(sk));

//            }
//        }

//        [Fact]
//        public void TestGetDebugInfo()
//        {
//            using (ILuaState L = new LuaState())
//            {
//                List<String> output = new List<string>();
//                L.OnPrint += (s, e) => output.Add(e.Text);
//                L.OpenLibs();

//                L.DoString(@"
//function test(a,b)
// local c = a + b
// print('test ', a, b, c)
//end
//");
//                L.DoString(@"print('start')");

//                L.SetHook((state, ar) =>
//                {
//                    L.GetInfo(LuaGetInfoWhat.AllFills, ar);
//                    if (ar.Name == "test")
//                    {
//                        // Locales
//                        for (int i = -2; i < 5; i++)
//                        {
//                            if (i == 0) continue;
//                            var s = L.GetLocal(ar, i);
//                            output.Add(String.Format("Local #{0} = {1}", i, s));
//                            if (s != null)
//                                L.Pop(1);
//                        }
//                        // Change local 2 (b)
//                        Assert.Equal("b", L.GetLocal(ar, 2));
//                        L.PushNumber(7);
//                        L.Arith(LuaArithOperator.Add);
//                        Assert.Equal("b", L.SetLocal(ar, 2));
//                        // Trace
//                        int level = 0;
//                        using (var tar = state.NewLuaDebug())
//                        {
//                            while (state.GetStack(level++, tar))
//                            {
//                                state.GetInfo(LuaGetInfoWhat.AllFills, tar);
//                                output.Add(String.Format(">{0} : {1}:{2}", level - 1, tar.ShortSource, tar.CurrentLine));
//                            }
//                        }
//                    }

//                }, LuaHookMask.MaskLine, 0);
//                L.DoString(@"print('Call1',test(1,2))");
//                L.SetHook(null, LuaHookMask.None, 0);

//                using (var ar = L.NewLuaDebug())
//                {
//                    L.GetGlobal("test");
//                    Assert.True(L.GetInfo(LuaGetInfoWhat.FromTopOfStack | LuaGetInfoWhat.AllFills, ar));

//                    Assert.Equal(-1, ar.CurrentLine);
//                    Assert.Equal(2, ar.LineDefined);
//                    Assert.Equal(5, ar.LastLineDefined);
//                    Assert.Equal(null, ar.Name);
//                    Assert.Equal("", ar.NameWhat);
//                    Assert.Equal(2, ar.NParams);
//                    Assert.Equal(1, ar.NUps);
//                    Assert.Equal("[string \"\r...\"]", ar.ShortSource);
//                    Assert.Equal("Lua", ar.What);

//                    // Local names
//                    L.GetGlobal("test");
//                    Assert.Equal(1, L.GetTop());
//                    Assert.Equal(null, L.GetLocal(null, 0));
//                    Assert.Equal("a", L.GetLocal(null, 1));
//                    Assert.Equal("b", L.GetLocal(null, 2));
//                    Assert.Equal(null, L.GetLocal(null, 3));
//                    Assert.Equal(null, L.GetLocal(null, 4));
//                    Assert.Equal(1, L.GetTop());

//                }

//                int sk = 20;
//                Assert.Equal(27, output.Count);
//                Assert.Equal(new String[]
//                {
//                    "start",
//                    "Local #-2 = ",
//                    "Local #-1 = ",
//                    "Local #1 = a",
//                    "Local #2 = b",
//                    "Local #3 = (*temporary)",
//                    "Local #4 = (*temporary)",
//                    ">0 : [string \"\r...\"]:3",
//                    ">1 : [string \"print('Call1',test(1,2))\"]:1",
//                    "Local #-2 = ",
//                    "Local #-1 = ",
//                    "Local #1 = a",
//                    "Local #2 = b",
//                    "Local #3 = c",
//                    "Local #4 = (*temporary)",
//                    ">0 : [string \"\r...\"]:4",
//                    ">1 : [string \"print('Call1',test(1,2))\"]:1",
//                    "test 	1	2	3",
//                    "Local #-2 = ",
//                    "Local #-1 = ",
//                    "Local #1 = a",
//                    "Local #2 = b",
//                    "Local #3 = c",
//                    "Local #4 = (*temporary)",
//                    ">0 : [string \"\r...\"]:5",
//                    ">1 : [string \"print('Call1',test(1,2))\"]:1",
//                    "Call1"
//                }.Skip(sk), output.Skip(sk));

//            }

//        }

    }
}
