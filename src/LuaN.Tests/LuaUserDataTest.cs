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
            LuaValue v;
            using (var state = mState.Object)
            {
                v = new LuaUserData(state, 123, true);
                Assert.Same(state, v.State);
                Assert.Equal(123, v.Reference);
                v.Dispose();
            }
        }

        [Fact]
        public void TestCall()
        {
            int top = 0;
            var mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(It.IsAny<int>(), -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            LuaUserData ud;
            using (var state = mState.Object)
            {
                ud = new LuaUserData(state, 123, true);

                Assert.Equal(new Object[] { null, true, 123.45, "Test" }, ud.Call(true, null, 1234, "Test"));
            }
        }

        [Fact]
        public void TestCallTyped()
        {
            int top = 0;
            var mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(It.IsAny<int>(), -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            LuaUserData ud;
            using (var state = mState.Object)
            {
                ud = new LuaUserData(state, 123, true);

                Assert.Equal(new Object[] { null, "True", 123 }, ud.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) }));
            }
        }

        [Fact]
        public void TestAccesByField()
        {
            var mState = new Mock<ILuaState>();
            LuaUserData ud;
            using (var state = mState.Object)
            {
                ud = new LuaUserData(state, 123, true);
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
            LuaUserData ud;
            using (var state = mState.Object)
            {
                ud = new LuaUserData(state, 123, true);
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
