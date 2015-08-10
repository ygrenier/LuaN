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
            var state = mState.Object;
            Lua l;
            LuaValue v;
            using (l = new Lua(state))
            {
                v = new LuaTable(l, 123, true);
                v.Dispose();
                Assert.Null(v.Lua);
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

            mState.Verify(s => s.LuaLUnref(state.RegistryIndex, 123), Times.Once());
            mState.Verify(s => s.LuaLUnref(state.RegistryIndex, 321), Times.Never());
        }

        [Fact]
        public void TestPush()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaValue v;
            using (l = new Lua(state))
            {
                using (v = new LuaTable(l, 123))
                {
                    v.Push(l.State);

                    Assert.Throws<ArgumentNullException>(() => v.Push(null));
                    Assert.Throws<ArgumentException>(() => v.Push(new Mock<ILuaState>().Object));
                }
                Assert.Throws<ArgumentException>(() => v.Push(l.State));
            }
            mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
        }

    }
}
