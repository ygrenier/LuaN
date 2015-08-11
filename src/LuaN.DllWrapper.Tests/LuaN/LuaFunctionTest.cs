using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaFunctionTest
    {
        [Fact]
        public void TestCreateRef()
        {
            var state = new LuaState();
            Lua l;
            LuaFunction v;
            using (l = new Lua(state))
            {
                v = new LuaFunction(l, 123, true);
                Assert.Same(l, v.Lua);
                Assert.Equal(123, v.Reference);
                Assert.Null(v.Function);
                v.Dispose();
            }
        }

        [Fact]
        public void TestCreateFunc()
        {
            var state = new LuaState();
            Lua l;
            LuaFunction v;
            using (l = new Lua(state))
            {
                LuaCFunction func = s => 0;
                v = new LuaFunction(l, func);
                Assert.Same(l, v.Lua);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                Assert.Same(func, v.Function);
                v.Dispose();
            }
        }

        [Fact]
        public void TestCall()
        {
            var state = new LuaState();
            Lua l;
            ILuaFunction fn;
            using (l = new Lua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d,e,f)
 return b, a, 123.45, d
end
");
                l.State.LuaGetGlobal("test");
                fn = l.ToFunction(-1);
                Assert.Equal(new Object[] { null, true, 123.45, "Test" }, fn.Call(true, null, 1234, "Test"));

                l.State.LuaPushCFunction(s =>
                {
                    Assert.Equal(4, s.LuaGetTop());
                    Assert.Equal(true, s.LuaToBoolean(1));
                    Assert.Equal(true, s.LuaIsNil(2));
                    Assert.Equal(1234, s.LuaToNumber(3));
                    Assert.Equal("Test", s.LuaToString(4));

                    s.LuaPushNil();
                    s.LuaPushBoolean(true);
                    s.LuaPushNumber(123.45);
                    s.LuaPushString("Test");
                    return 4;
                });
                fn = l.ToFunction(-1);
                Assert.Equal(new Object[] { null, true, 123.45, "Test" }, fn.Call(true, null, 1234, "Test"));
            }
        }

        [Fact]
        public void TestCallTyped()
        {
            var state = new LuaState();
            Lua l;
            ILuaFunction fn;
            using (l = new Lua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d,e,f)
 return b, a, 123.45, d
end
");
                l.State.LuaGetGlobal("test");
                fn = l.ToFunction(-1);
                Assert.Equal(
                    new Object[] { null, "True", 123 }, 
                    fn.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) })
                    );

                l.State.LuaPushCFunction(s =>
                {
                    Assert.Equal(4, s.LuaGetTop());
                    Assert.Equal(true, s.LuaToBoolean(1));
                    Assert.Equal(true, s.LuaIsNil(2));
                    Assert.Equal(1234, s.LuaToNumber(3));
                    Assert.Equal("Test", s.LuaToString(4));

                    s.LuaPushNil();
                    s.LuaPushBoolean(true);
                    s.LuaPushNumber(123.45);
                    s.LuaPushString("Test");
                    return 4;
                });
                fn = l.ToFunction(-1);
                Assert.Equal(
                    new Object[] { null, "True", 123 }, 
                    fn.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) })
                    );
            }
        }

    }
}
