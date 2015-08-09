using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaHookWrapperTest
    {

        [Fact]
        public void TestFromHook()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaHook hook = (state, ar) =>
                {
                    Assert.Same(state, L);
                };
                var wrapper = new LuaHookWrapper(hook);
                Assert.Same(hook, wrapper.Hook);

                wrapper.NativeHook(L.NativeState, IntPtr.Zero);
                wrapper.NativeHook(IntPtr.Zero, IntPtr.Zero);

                Assert.Throws<ArgumentNullException>(() => new LuaHookWrapper((LuaHook)null));
            }
        }

        [Fact]
        public void TestFromNativeHook()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaDll.lua_Hook nativeHook = (state, pr) =>
                {
                    Assert.Equal(state, L.NativeState);
                };
                var wrapper = new LuaHookWrapper(nativeHook);
                Assert.Same(nativeHook, wrapper.NativeHook);

                var d = L.NewLuaDebug();
                wrapper.Hook(L, d);
                wrapper.Hook(null, d);

                Assert.Throws<ArgumentNullException>(() => new LuaHookWrapper((LuaDll.lua_Hook)null));
            }
        }

    }
}
