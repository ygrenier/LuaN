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
        public void TestNewThread()
        {
            List<String> output = new List<string>();

            using (var L = new LuaState())
            {
                L.OnPrint += (s, e) => { output.Add(e.Text); e.Handled = true; };
                L.OpenLibs();

                Assert.Equal(LuaType.Thread, L.GetI(L.RegistryIndex, Lua.LUA_RIDX_MAINTHREAD));
                ILuaState mThread = L.ToThread(-1);
                L.Pop(1);

                L.PushString("Content of the var a");
                L.SetGlobal("a");

                L.GetGlobal("print");
                L.PushString("From L");
                L.GetGlobal("a");
                L.Call(2, 0);

                using (var L2 = L.NewThread())
                {
                    L2.GetGlobal("a");
                    Assert.Equal("Content of the var a", L2.ToString(-1));

                    Assert.Equal(LuaType.Thread, L2.GetI(L.RegistryIndex, Lua.LUA_RIDX_MAINTHREAD));
                    ILuaState mThread2 = L2.ToThread(-1);
                    L2.Pop(1);
                    Assert.Same(mThread, mThread2);

                    L2.GetGlobal("print");
                    L2.PushString("From L2");
                    L2.GetGlobal("a");
                    L2.Call(2, 0);
                }

            }

            Assert.Equal(new String[]{
                "From L\tContent of the var a",
                "From L2\tContent of the var a"
            }, output);

        }

        [Fact]
        public void TestCoroutine()
        {
            List<String> output = new List<string>();

            using (var L = new LuaState())
            {
                L.OnPrint += (s, e) => { output.Add(e.Text); e.Handled = true; };
                L.OpenLibs();

                L.DoString(@"
function foo (a)
  print('foo', a)
  return coroutine.yield(2 * a)
end

co = coroutine.create(function(a, b)
      print('co-body1', a, b)
      local r = foo(a + 1)
      print('co-body2', r)
      local r, s = coroutine.yield(a + b, a - b)
      print('co-body3', r, s)
      return b, 'end'
end)

print('main', coroutine.resume(co, 1, 10))
print('main', coroutine.resume(co, 'r'))
print('main', coroutine.resume(co, 'x', 'y'))
print('main', coroutine.resume(co, 'x', 'y'))

");

            }

            Assert.Equal(new String[]{
                "co-body1\t1\t10",
                "foo\t2",
                "main\ttrue\t4",
                "co-body2\tr",
                "main\ttrue\t11\t-9",
                "co-body3\tx\ty",
                "main\ttrue\t10\tend",
                "main\tfalse\tcannot resume dead coroutine"
            }, output);
        }

        /*
        
        
        
        
        */

    }
}
