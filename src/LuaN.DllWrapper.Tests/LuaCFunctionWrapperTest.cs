using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaCFunctionWrapperTest
    {

        [Fact]
        public void TestFromCFunction()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaCFunction cfunction = state =>
                {
                    Assert.Same(state, L);
                    return 1;
                };
                var wrapper = new LuaCFunctionWrapper(cfunction);
                Assert.Same(cfunction, wrapper.CFunction);

                Assert.Equal(1, wrapper.NativeFunction(L.NativeState));
                Assert.Equal(0, wrapper.NativeFunction(IntPtr.Zero));

                Assert.Throws<ArgumentNullException>(() => new LuaCFunctionWrapper((LuaCFunction)null));
            }
        }

        [Fact]
        public void TestFromNativeFunction()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaDll.lua_CFunction nativeFunction = state =>
                {
                    Assert.Equal(state, L.NativeState);
                    return 1;
                };
                var wrapper = new LuaCFunctionWrapper(nativeFunction);
                Assert.Same(nativeFunction, wrapper.NativeFunction);

                Assert.Equal(1, wrapper.CFunction(L));
                Assert.Equal(0, wrapper.CFunction(null));

                Assert.Throws<ArgumentNullException>(() => new LuaCFunctionWrapper((LuaDll.lua_CFunction)null));
            }
        }

    }
}
