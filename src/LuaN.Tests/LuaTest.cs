using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{
    public class LuaTest
    {

        [Fact]
        public void TestCreateFromEngine()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            var mEngine = new Mock<ILuaEngine>();
            mEngine.Setup(e => e.NewState()).Returns(state);

            using(var l=new Lua(mEngine.Object))
            {
                Assert.Same(state, l.State);
            }

            mState.Verify(s => s.LuaPushString("LUAN HOSTED"), Times.Once());
            mState.Verify(s => s.LuaPushBoolean(true), Times.Once());
            mState.Verify(s => s.LuaSetTable(state.RegistryIndex), Times.Once());
            mState.Verify(s => s.Dispose(), Times.Once());

            Assert.Throws<ArgumentNullException>(() => new Lua((ILuaEngine)null));
        }

        [Fact]
        public void TestCreateFromState()
        {
            // Normal
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaToBoolean(-1)).Returns(false);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                Assert.Same(state, l.State);
            }
            mState.Verify(s => s.LuaPushString("LUAN HOSTED"), Times.Exactly(2));   // Once to check, Once to defined
            mState.Verify(s => s.LuaGetTable(state.RegistryIndex), Times.Once());
            mState.Verify(s => s.LuaToBoolean(-1), Times.Once());
            mState.Verify(s => s.LuaPushBoolean(true), Times.Once());
            mState.Verify(s => s.LuaSetTable(state.RegistryIndex), Times.Once());
            mState.Verify(s => s.Dispose(), Times.Once());

            // State not owned
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaToBoolean(-1)).Returns(false);
            state = mState.Object;
            using (var l = new Lua(state, false))
            {
                Assert.Same(state, l.State);
            }
            mState.Verify(s => s.LuaPushString("LUAN HOSTED"), Times.Exactly(2));   // Once to check, Once to defined
            mState.Verify(s => s.LuaGetTable(state.RegistryIndex), Times.Once());
            mState.Verify(s => s.LuaToBoolean(-1), Times.Once());
            mState.Verify(s => s.LuaPushBoolean(true), Times.Once());
            mState.Verify(s => s.LuaSetTable(state.RegistryIndex), Times.Once());
            mState.Verify(s => s.Dispose(), Times.Never()); // Because the state is not owned, there is not disposed

            // State already hosted
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaToBoolean(-1)).Returns(true);
            state = mState.Object;
            var ioex = Assert.Throws<InvalidOperationException>(() => new Lua(state));
            Assert.Equal("This state is already hosted.", ioex.Message);
            mState.Verify(s => s.Dispose(), Times.Never());

            Assert.Throws<ArgumentNullException>(() => new Lua((ILuaState)null));
        }

    }
}
