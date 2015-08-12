using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{
    public class LuaValueTest
    {

        [Fact]
        public void TestDispose()
        {
            var mState = new Mock<ILuaState>();
            LuaValue v;
            using (var l = mState.Object)
            {
                v = new LuaTable(l, 123, true);
                v.Dispose();
                Assert.Null(v.State);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                v = new LuaTable(l, 321, false);
                v.Dispose();

                // Create a value for testing finalize
                v = new LuaTable(l, 555);
            }
            // Force the finalisation of the last value
            GC.Collect();
            //Assert.Null(v.Lua);
            //Assert.Equal(LuaRef.NoRef, v.Reference);

            mState.Verify(s => s.LuaLUnref(mState.Object.RegistryIndex, 123), Times.Once());
            mState.Verify(s => s.LuaLUnref(mState.Object.RegistryIndex, 321), Times.Never());
        }

        [Fact]
        public void TestPush()
        {
            var mState = new Mock<ILuaState>();
            LuaValue v;
            using (var state = mState.Object)
            {
                using (v = new LuaTable(state, 123))
                {
                    v.Push(state);

                    Assert.Throws<ArgumentNullException>(() => v.Push(null));
                    Assert.Throws<ArgumentException>(() => v.Push(new Mock<ILuaState>().Object));
                }
                Assert.Throws<ArgumentException>(() => v.Push(state));
            }
            mState.Verify(s => s.LuaRawGetI(mState.Object.RegistryIndex, 123), Times.Once());
        }

    }
}
