using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static LuaNet.LuaLib.Lua;

namespace LuaNet.LuaLib.Tests
{
    public class LuaFunctionTest
    {

        [Fact]
        public void TestToCFunction()
        {
            LuaFunction fnc = null;
            lua_CFunction cFnc = null;
            Assert.Null(fnc.ToCFunction());
            Assert.Null(cFnc.ToFunction());

            using (LuaState state = new LuaState())
            {
                fnc = (L) =>
                {
                    Assert.Same(state, L);
                    return 123;
                };

                cFnc = fnc.ToCFunction();
                Assert.NotNull(cFnc);
                Assert.Same(cFnc, fnc.ToCFunction());
                Assert.Same(fnc, cFnc.ToFunction());

                var a = cFnc(state.NativeState);
                Assert.Equal(123, a);
            }
        }

    }
}
