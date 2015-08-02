using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaNet.LuaLib.Tests
{
    partial class LuaStateTest
    {
        [Fact]
        public void TestDebugHook()
        {
            using (ILuaState L = new LuaState())
            {
                List<String> output = new List<string>();
                L.OnPrint += (s, e) => output.Add(e.Text);
                L.OpenLibs();

                Assert.Null(L.GetHook());
                Assert.Equal(0, L.GetHookCount());
                Assert.Equal(LuaHookMask.None, L.GetHookMask());

                LuaHook hook = (state, dg) =>
                 {
                     state.GetInfo(LuaGetInfoWhat.AllFills, dg);
                     output.Add(String.Format("Hook ({0}) Line {1} : {2} ({3}). {4}.", dg.Event, dg.CurrentLine, dg.What, dg.NameWhat, dg.Name));
                 };

                L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskRet, 0);

                Assert.Same(hook, L.GetHook());
                Assert.Equal(0, L.GetHookCount());
                Assert.Equal(LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskRet, L.GetHookMask());

                L.DoString(@"print('start')

function test(a,b)
 local c = a + b
 print('test ', a, b, c)
end

print(
  'Call1',
  test(1,2)
)

print(
  'Call2',
  test(23,17)
)

print('end')

");
                L.SetHook(null, LuaHookMask.None, 0);
                Assert.Null(L.GetHook());
                Assert.Equal(0, L.GetHookCount());
                Assert.Equal(LuaHookMask.None, L.GetHookMask());

                L.DoString(@"print('test after end')");

                int sk = 0;
                Assert.Equal(67, output.Count);
                Assert.Equal(new String[]
                {
                    // print('start')
                    "Hook (HookCall) Line 1 : main (). .",
                    "Hook (HookLine) Line 1 : main (). .",
                    "Hook (HookCall) Line -1 : C (global). print.",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "start",
                    "Hook (HookRet) Line -1 : C (global). print.",
                    // function
                    "Hook (HookLine) Line 6 : main (). .",
                    "Hook (HookLine) Line 3 : main (). .",
                    // first print
                    "Hook (HookLine) Line 8 : main (). .",
                    "Hook (HookLine) Line 9 : main (). .",
                    "Hook (HookLine) Line 10 : main (). .",
                    "Hook (HookCall) Line 4 : Lua (global). test.",
                    "Hook (HookLine) Line 4 : Lua (global). test.",
                    "Hook (HookLine) Line 5 : Lua (global). test.",
                    "Hook (HookCall) Line -1 : C (global). print.",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "test 	1	2	3",
                    "Hook (HookRet) Line -1 : C (global). print.",
                    "Hook (HookLine) Line 6 : Lua (global). test.",
                    "Hook (HookRet) Line 6 : Lua (global). test.",
                    "Hook (HookLine) Line 8 : main (). .",
                    "Hook (HookCall) Line -1 : C (global). print.",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Call1",
                    "Hook (HookRet) Line -1 : C (global). print.",
                    // Second print
                    "Hook (HookLine) Line 13 : main (). .",
                    "Hook (HookLine) Line 14 : main (). .",
                    "Hook (HookLine) Line 15 : main (). .",
                    "Hook (HookCall) Line 4 : Lua (global). test.",
                    "Hook (HookLine) Line 4 : Lua (global). test.",
                    "Hook (HookLine) Line 5 : Lua (global). test.",
                    "Hook (HookCall) Line -1 : C (global). print.",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "test 	23	17	40",
                    "Hook (HookRet) Line -1 : C (global). print.",
                    "Hook (HookLine) Line 6 : Lua (global). test.",
                    "Hook (HookRet) Line 6 : Lua (global). test.",
                    "Hook (HookLine) Line 13 : main (). .",
                    "Hook (HookCall) Line -1 : C (global). print.",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "Call2",
                    "Hook (HookRet) Line -1 : C (global). print.",
                    "Hook (HookLine) Line 18 : main (). .",
                    // print('end')
                    "Hook (HookCall) Line -1 : C (global). print.",
                    "Hook (HookCall) Line -1 : C (). .",
                    "Hook (HookRet) Line -1 : C (). .",
                    "end",
                    "Hook (HookRet) Line -1 : C (global). print.",
                    // end of chunk
                    "Hook (HookRet) Line 18 : main (). .",
                    // after end
                    "test after end"
                }.Skip(sk), output.Skip(sk));

            }
        }

        [Fact]
        public void TestGetDebugInfo()
        {
            using (ILuaState L = new LuaState())
            {
                List<String> output = new List<string>();
                L.OnPrint += (s, e) => output.Add(e.Text);
                L.OpenLibs();

                L.DoString(@"
function test(a,b)
 local c = a + b
 print('test ', a, b, c)
end
");
                L.DoString(@"print('start')");

                L.SetHook((state, ar) =>
                {
                    L.GetInfo(LuaGetInfoWhat.AllFills, ar);
                    if (ar.Name == "test")
                    {
                        // Locales
                        for (int i = -2; i < 5; i++)
                        {
                            if (i == 0) continue;
                            var s = L.GetLocal(ar, i);
                            output.Add(String.Format("Local #{0} = {1}", i, s));
                            if (s != null)
                                L.Pop(1);
                        }
                        // Change local 2 (b)
                        Assert.Equal("b", L.GetLocal(ar, 2));
                        L.PushNumber(7);
                        L.Arith(LuaArithOperator.Add);
                        Assert.Equal("b", L.SetLocal(ar, 2));
                        // Trace
                        int level = 0;
                        using (var tar = state.NewLuaDebug())
                        {
                            while (state.GetStack(level++, tar))
                            {
                                state.GetInfo(LuaGetInfoWhat.AllFills, tar);
                                output.Add(String.Format(">{0} : {1}:{2}", level-1, tar.ShortSource, tar.CurrentLine));
                            }
                        }
                    }

                }, LuaHookMask.MaskLine, 0);
                L.DoString(@"print('Call1',test(1,2))");
                L.SetHook(null, LuaHookMask.None, 0);

                using (var ar = L.NewLuaDebug())
                {
                    L.GetGlobal("test");
                    Assert.True(L.GetInfo(LuaGetInfoWhat.FromTopOfStack | LuaGetInfoWhat.AllFills, ar));

                    Assert.Equal(-1, ar.CurrentLine);
                    Assert.Equal(2, ar.LineDefined);
                    Assert.Equal(5, ar.LastLineDefined);
                    Assert.Equal(null, ar.Name);
                    Assert.Equal("", ar.NameWhat);
                    Assert.Equal(2, ar.NParams);
                    Assert.Equal(1, ar.NUps);
                    Assert.Equal("[string \"\r...\"]", ar.ShortSource);
                    Assert.Equal("Lua", ar.What);

                    // Local names
                    L.GetGlobal("test");
                    Assert.Equal(1, L.GetTop());
                    Assert.Equal(null, L.GetLocal(null, 0));
                    Assert.Equal("a", L.GetLocal(null, 1));
                    Assert.Equal("b", L.GetLocal(null, 2));
                    Assert.Equal(null, L.GetLocal(null, 3));
                    Assert.Equal(null, L.GetLocal(null, 4));
                    Assert.Equal(1, L.GetTop());

                }

                int sk = 20;
                Assert.Equal(27, output.Count);
                Assert.Equal(new String[]
                {
                    "start",
                    "Local #-2 = ",
                    "Local #-1 = ",
                    "Local #1 = a",
                    "Local #2 = b",
                    "Local #3 = (*temporary)",
                    "Local #4 = (*temporary)",
                    ">0 : [string \"\r...\"]:3",
                    ">1 : [string \"print('Call1',test(1,2))\"]:1",
                    "Local #-2 = ",
                    "Local #-1 = ",
                    "Local #1 = a",
                    "Local #2 = b",
                    "Local #3 = c",
                    "Local #4 = (*temporary)",
                    ">0 : [string \"\r...\"]:4",
                    ">1 : [string \"print('Call1',test(1,2))\"]:1",
                    "test 	1	2	3",
                    "Local #-2 = ",
                    "Local #-1 = ",
                    "Local #1 = a",
                    "Local #2 = b",
                    "Local #3 = c",
                    "Local #4 = (*temporary)",
                    ">0 : [string \"\r...\"]:5",
                    ">1 : [string \"print('Call1',test(1,2))\"]:1",
                    "Call1"
                }.Skip(sk), output.Skip(sk));

            }

        }

    }
}
