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

        [Fact]
        public void TestPushNetObject()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            using (l = new Lua(state))
            {
                mState.Verify(s => s.LuaPushBoolean(true), Times.Exactly(1));   // First call do when Lua is created

                l.PushNetObject(null);
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                l.PushNetObject(true);
                mState.Verify(s => s.LuaPushBoolean(true), Times.Exactly(2));   // First call do when Lua is created
                l.PushNetObject(false);
                mState.Verify(s => s.LuaPushBoolean(false), Times.Once());
                l.PushNetObject(12f);
                mState.Verify(s => s.LuaPushNumber(12), Times.Once());
                l.PushNetObject(34.56d);
                mState.Verify(s => s.LuaPushNumber(34.56), Times.Once());
                l.PushNetObject(78.9m);
                mState.Verify(s => s.LuaPushNumber(78.9), Times.Once());
                l.PushNetObject(98);
                mState.Verify(s => s.LuaPushInteger(98), Times.Once());
                l.PushNetObject("Test");
                mState.Verify(s => s.LuaPushString("Test"), Times.Once());
                var ioex = Assert.Throws<InvalidOperationException>(() => l.PushNetObject(new Mock<ILuaUserData>().Object));
                Assert.Equal("Can't push a userdata", ioex.Message);
                LuaCFunction func = st => 0;
                l.PushNetObject(func);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());
                l.PushNetObject(l.State);
                mState.Verify(s => s.LuaPushThread(), Times.Once());
                ioex = Assert.Throws<InvalidOperationException>(() => l.PushNetObject(new Mock<ILuaState>().Object));
                Assert.Equal("Can't push a different thread", ioex.Message);
                var tbl = new LuaTable(l, 9876);
                l.PushNetObject(tbl);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 9876), Times.Once());
                l.PushNetObject(this);
                mState.Verify(s => s.LuaPushLightUserData(this), Times.Once());
            }
        }

        [Fact]
        public void TestToNetObject()
        {
            Lua l = null;

            var mState = new Mock<ILuaState>();
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(3)).Returns(false);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(4)).Returns(123.45);
            mState.Setup(_ => _.LuaType(5)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(5)).Returns("Test");
            mState.Setup(_ => _.LuaType(6)).Returns(LuaType.LightUserData);
            mState.Setup(_ => _.LuaToUserData(6)).Returns(this);
            mState.Setup(_ => _.LuaType(7)).Returns(LuaType.UserData);
            mState.Setup(_ => _.LuaType(8)).Returns(LuaType.Table);
            mState.Setup(_ => _.LuaType(9)).Returns(LuaType.Function);
            mState.Setup(_ => _.LuaType(10)).Returns(LuaType.Thread);
            mState.Setup(_ => _.LuaToThread(10)).Returns(() => l.State);

            var mUserData = new Mock<ILuaUserData>();
            var ud = mUserData.Object;
            mState.Setup(_ => _.LuaToUserData(7)).Returns(ud);
            var state = mState.Object;
            using (l = new Lua(state))
            {
                Assert.Equal(null, l.ToNetObject(1));
                Assert.Equal(true, l.ToNetObject(2));
                Assert.Equal(false, l.ToNetObject(3));
                Assert.Equal(123.45, l.ToNetObject(4));
                Assert.Equal("Test", l.ToNetObject(5));
                Assert.Same(this, l.ToNetObject(6));
                Assert.Same(ud, l.ToNetObject(7));
                var tbl = l.ToNetObject(8);
                Assert.IsAssignableFrom<ILuaTable>(tbl);
                Assert.Throws<NotImplementedException>(() => l.ToNetObject(9));
                Assert.Same(state, l.ToNetObject(10));
            }
        }

    }
}
