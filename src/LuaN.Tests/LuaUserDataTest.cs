using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{

    public class LuaUserDataTest
    {

        [Fact]
        public void TestCreate()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaValue v;
            using (l = new Lua(state))
            {
                v = new LuaUserData(l, 123, true);
                Assert.Same(l, v.Lua);
                Assert.Equal(123, v.Reference);
                v.Dispose();
            }
        }

        [Fact]
        public void TestAccesByField()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaUserData ud;
            using (l = new Lua(state))
            {
                ud = new LuaUserData(l, 123, true);
                ud["field1"] = 1234;
                
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaSetField(It.IsAny<int>(), "field1"), Times.Once());

                Assert.Equal(null, ud["field2"]);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(2));
                mState.Verify(s => s.LuaGetField(It.IsAny<int>(), "field2"), Times.Once());

                ud.Dispose();
            }
        }

        [Fact]
        public void TestAccesByObject()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaUserData ud;
            using (l = new Lua(state))
            {
                ud = new LuaUserData(l, 123, true);
                ud[777.77] = 1234;

                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushNumber(777.77), Times.Once());
                //mState.Verify(s => s.LuaSetTable(It.IsAny<int>()), Times.Once());

                Assert.Equal(null, ud[888.88]);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(2));
                mState.Verify(s => s.LuaPushNumber(888.88), Times.Once());
                //mState.Verify(s => s.LuaGetTable(It.IsAny<int>()), Times.Once());

                ud.Dispose();
            }
        }

    }

}
