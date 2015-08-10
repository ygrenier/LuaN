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

            Lua l;
            using (l = new Lua(mEngine.Object))
            {
                Assert.Same(state, l.State);
            }
            Assert.Throws<ObjectDisposedException>(() => l.State != null);

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
            Lua l;
            using (l = new Lua(state))
            {
                Assert.Same(state, l.State);
            }
            Assert.Throws<ObjectDisposedException>(() => l.State != null);
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
            using (l = new Lua(state, false))
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

        [Fact]
        public void TestDisposeLuaValue()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            using (l = new Lua(state))
            {
                var tbl = new LuaTable(l, 123, true);
                tbl.Dispose();
                tbl = new LuaTable(l, 987, false);
                tbl.Dispose();
            }
            mState.Verify(s => s.LuaLUnref(state.RegistryIndex, 123), Times.Once());
            mState.Verify(s => s.LuaLUnref(state.RegistryIndex, 987), Times.Never());
        }

        [Fact]
        public void TestToTable()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Table);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.String);
            var state = mState.Object;
            Lua l;
            using (l = new Lua(state))
            {
                // Existing table
                using (var table=l.ToTable(1))
                {
                    Assert.IsType<LuaTable>(table);
                    var tb = table as LuaTable;
                    Assert.Same(l, tb.Lua);
                }
                // Not a table
                Assert.Null(l.ToTable(2));
            }

            // Invalid ref
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Table);
            mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            state = mState.Object;
            using (l = new Lua(state))
            {
                var ioex = Assert.Throws<InvalidOperationException>(() => l.ToTable(1));
                Assert.Equal("Can't create a reference for this value.", ioex.Message);
            }
        }

    }
}
