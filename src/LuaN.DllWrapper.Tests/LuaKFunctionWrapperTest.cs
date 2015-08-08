using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaKFunctionWrapperTest
    {

        [Fact]
        public void TestFromKFunction()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaKFunction kfunction = (state,status,ctx) =>
                {
                    Assert.Same(state, L);
                    return 1;
                };
                var wrapper = new LuaKFunctionWrapper(kfunction);
                Assert.Same(kfunction, wrapper.KFunction);

                Assert.Equal(1, wrapper.NativeFunction(L.NativeState, 0, 0));
                Assert.Equal(0, wrapper.NativeFunction(IntPtr.Zero, 0, 0));

                Assert.Throws<ArgumentNullException>(() => new LuaKFunctionWrapper((LuaKFunction)null));
            }
        }

    }
}
