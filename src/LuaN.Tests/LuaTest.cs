using Moq;
using Moq.Protected;
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
        class PublicLua : Lua
        {
            public PublicLua(ILuaState state) : base(state) { }
        }

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
        public void TestThrowError()
        {
            var mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => 6);
            mState.Setup(s => s.LuaPCall(0, -1, 0)).Returns(() => { return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.Nil);
            var state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var ex = Assert.Throws<LuaException>(() => l.State.CallValue(123, new object[] { }));
                Assert.Equal("Unknown Lua error.", ex.Message);
                mState.Verify(s => s.LuaSetTop(5));
            }

            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => 6);
            mState.Setup(s => s.LuaPCall(0, -1, 0)).Returns(() => { return LuaStatus.ErrorRun; });
            mState.Setup(s => s.LuaType(-1)).Returns(LuaType.String);
            mState.Setup(s => s.LuaToString(-1)).Returns("Custom error");
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var ex = Assert.Throws<LuaException>(() => l.State.CallValue(123, new object[] { }));
                Assert.Equal("Custom error", ex.Message);
                mState.Verify(s => s.LuaSetTop(5));
            }

            LuaException myEx = new LuaException("My error");
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => 6);
            mState.Setup(s => s.LuaPCall(0, -1, 0)).Returns(() => { return LuaStatus.ErrorRun; });
            mState.Setup(s => s.LuaType(-1)).Returns(LuaType.LightUserData);
            mState.Setup(s => s.LuaToUserData(-1)).Returns(myEx);
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var ex = Assert.Throws<LuaException>(() => l.State.CallValue(123, new object[] { }));
                Assert.Equal("My error", ex.Message);
                Assert.Same(myEx, ex);
                mState.Verify(s => s.LuaSetTop(5));
            }

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
                    Assert.Same(l.State, tb.State);
                }
                // Not a table
                Assert.Null(l.ToTable(2));
            }

            // Invalid ref
            //mState = new Mock<ILuaState>();
            //mState.Setup(s => s.LuaType(1)).Returns(LuaType.Table);
            //mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            //state = mState.Object;
            //using (l = new Lua(state))
            //{
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToTable(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }

        [Fact]
        public void TestToUserData()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.UserData);
            mState.Setup(s => s.LuaIsUserData(1)).Returns(true);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.String);
            mState.Setup(s => s.LuaIsUserData(2)).Returns(false);
            var state = mState.Object;
            Lua l;
            using (l = new Lua(state))
            {
                // Existing userdata
                using (var userdata = l.ToUserData(1))
                {
                    Assert.IsType<LuaUserData>(userdata);
                    var ud = userdata as LuaUserData;
                    Assert.Same(l.State, ud.State);
                }
                // Not an userdata
                Assert.Null(l.ToUserData(2));
            }

            // Invalid ref
            //mState = new Mock<ILuaState>();
            //mState.Setup(s => s.LuaType(1)).Returns(LuaType.UserData);
            //mState.Setup(s => s.LuaIsUserData(1)).Returns(true);
            //mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            //state = mState.Object;
            //using (l = new Lua(state))
            //{
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToUserData(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }

        [Fact]
        public void TestToFunction()
        {
            LuaCFunction func = s => 0;

            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaIsFunction(1)).Returns(true);
            mState.Setup(s => s.LuaIsCFunction(1)).Returns(true);
            mState.Setup(s => s.LuaToCFunction(1)).Returns(func);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaIsFunction(2)).Returns(true);
            mState.Setup(s => s.LuaIsCFunction(2)).Returns(false);
            mState.Setup(s => s.LuaType(3)).Returns(LuaType.String);
            mState.Setup(s => s.LuaIsFunction(3)).Returns(false);
            mState.Setup(s => s.LuaIsCFunction(3)).Returns(false);
            var state = mState.Object;
            Lua l;
            using (l = new Lua(state))
            {
                // Existing c function
                using (var function = l.ToFunction(1))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(l.State, fn.State);
                    Assert.Same(func, fn.Function);
                }
                // Existing lua function
                using (var function = l.ToFunction(2))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(l.State, fn.State);
                }
                // Not an LuaFunction
                Assert.Null(l.ToFunction(3));
            }

            // Invalid ref
            //mState = new Mock<ILuaState>();
            //mState.Setup(s => s.LuaType(1)).Returns(LuaType.Function);
            //mState.Setup(s => s.LuaIsFunction(1)).Returns(true);
            //mState.Setup(s => s.LuaIsCFunction(1)).Returns(false);
            //mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            //state = mState.Object;
            //using (l = new Lua(state))
            //{
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToFunction(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }

        [Fact]
        public void TestPush()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            using (l = new Lua(state))
            {
                mState.Verify(s => s.LuaPushBoolean(true), Times.Exactly(1));   // First call do when Lua is created

                l.Push(null);
                mState.Verify(s => s.LuaPushNil(), Times.Once());

                l.Push(true);
                mState.Verify(s => s.LuaPushBoolean(true), Times.Exactly(2));   // First call do when Lua is created

                l.Push(false);
                mState.Verify(s => s.LuaPushBoolean(false), Times.Once());

                l.Push(12f);
                mState.Verify(s => s.LuaPushNumber(12), Times.Once());

                l.Push(34.56d);
                mState.Verify(s => s.LuaPushNumber(34.56), Times.Once());

                l.Push(78.9m);
                mState.Verify(s => s.LuaPushNumber(78.9), Times.Once());

                l.Push(98);
                mState.Verify(s => s.LuaPushInteger(98), Times.Once());

                l.Push("Test");
                mState.Verify(s => s.LuaPushString("Test"), Times.Once());

                var ioex = Assert.Throws<InvalidOperationException>(() => l.Push(new Mock<ILuaNativeUserData>().Object));
                Assert.Equal("Can't push a userdata", ioex.Message);

                LuaCFunction func = st => 0;
                l.Push(func);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());

                l.Push(l.State);
                mState.Verify(s => s.LuaPushThread(), Times.Once());
                ioex = Assert.Throws<InvalidOperationException>(() => l.Push(new Mock<ILuaState>().Object));
                Assert.Equal("Can't push a different thread", ioex.Message);

                var tbl = new LuaTable(state, 9876);
                l.Push(tbl);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 9876), Times.Once());

                var ud = new LuaUserData(state, 8765);
                l.Push(ud);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 8765), Times.Once());

                l.Push(this);
                mState.Verify(s => s.LuaPushLightUserData(this), Times.Once());
            }
        }

        [Fact]
        public void TestToValue()
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
            mState.Setup(_ => _.LuaIsUserData(6)).Returns(true);
            mState.Setup(_ => _.LuaToUserData(6)).Returns(this);
            mState.Setup(_ => _.LuaType(7)).Returns(LuaType.UserData);
            mState.Setup(_ => _.LuaIsUserData(7)).Returns(true);
            mState.Setup(_ => _.LuaToUserData(7)).Returns(this);
            mState.Setup(_ => _.LuaType(8)).Returns(LuaType.Table);
            mState.Setup(_ => _.LuaType(9)).Returns(LuaType.Function);
            mState.Setup(_ => _.LuaIsFunction(9)).Returns(true);
            mState.Setup(_ => _.LuaType(10)).Returns(LuaType.Thread);
            mState.Setup(_ => _.LuaToThread(10)).Returns(() => l.State);

            var mUserData = new Mock<ILuaNativeUserData>();
            var ud = mUserData.Object;
            mState.Setup(_ => _.LuaToUserData(7)).Returns(ud);
            var state = mState.Object;
            using (l = new Lua(state))
            {
                Assert.Equal(null, l.ToValue(1));
                Assert.Equal(true, l.ToValue(2));
                Assert.Equal(false, l.ToValue(3));
                Assert.Equal(123.45, l.ToValue(4));
                Assert.Equal("Test", l.ToValue(5));
                Assert.Same(this, l.ToValue(6));
                Assert.IsAssignableFrom<ILuaUserData>(l.ToValue(7));
                var tbl = l.ToValue(8);
                Assert.IsAssignableFrom<ILuaTable>(tbl);
                Assert.IsAssignableFrom<ILuaFunction>(l.ToValue(9));
                Assert.Same(state, l.ToValue(10));
            }
        }

        [Fact]
        public void TestDoFile()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaGetTop()).Returns(4);
            mState.Setup(s => s.LuaLLoadFile("file")).Returns(LuaStatus.Ok);
            mState.Setup(s => s.LuaLLoadFile("fileErr")).Returns(LuaStatus.ErrorRun);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                var result = l.DoFile("file");
                Assert.Equal(new object[] { }, result);

                var ex = Assert.Throws<LuaException>(() => l.DoFile("fileErr"));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
        }

        [Fact]
        public void TestDoStringText()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaGetTop()).Returns(4);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 6, "myScript")).Returns(LuaStatus.Ok);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 9, "myScript")).Returns(LuaStatus.ErrorRun);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                var result = l.DoString("script", "myScript");
                Assert.Equal(new object[] { }, result);

                var ex = Assert.Throws<LuaException>(() => l.DoString("scriptErr", "myScript"));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
        }

        [Fact]
        public void TestDoStringBinary()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 6, "myScript")).Returns(LuaStatus.Ok);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 9, "myScript")).Returns(LuaStatus.ErrorRun);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                byte[] buffer = new byte[6];
                var result = l.DoString(buffer, "myScript");
                Assert.Equal(new object[] { }, result);

                buffer = new byte[9];
                var ex = Assert.Throws<LuaException>(() => l.DoString(buffer, "myScript"));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
        }

        [Fact]
        public void TestLoadFile()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaLLoadFile("file")).Returns(LuaStatus.Ok);
            mState.Setup(s => s.LuaIsFunction(-1)).Returns(true);
            mState.Setup(s => s.LuaLLoadFile("fileErr")).Returns(LuaStatus.ErrorRun);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                var function = l.LoadFile("file");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] {  }, result);

                var ex = Assert.Throws<LuaException>(() => l.LoadFile("fileErr"));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
        }

        [Fact]
        public void TestLoadStringText()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaIsFunction(-1)).Returns(true);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 6, "myScript")).Returns(LuaStatus.Ok);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 9, "myScript")).Returns(LuaStatus.ErrorRun);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                var function = l.LoadString("script", "myScript");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { }, result);

                var ex = Assert.Throws<LuaException>(() => l.LoadString("scriptErr", "myScript"));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
        }

        [Fact]
        public void TestLoadStringBinary()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaIsFunction(-1)).Returns(true);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 6, "myScript")).Returns(LuaStatus.Ok);
            mState.Setup(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 9, "myScript")).Returns(LuaStatus.ErrorRun);
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                byte[] buffer = new byte[6];
                var function = l.LoadString(buffer, "myScript");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { }, result);

                buffer = new byte[9];
                var ex = Assert.Throws<LuaException>(() => l.LoadString(buffer, "myScript"));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
        }

        [Fact]
        public void TestGlobalsAccess()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                Assert.Null(l["var1"]);
                l["Var1"] = "One";
                l["var1"] = 123.45;

                mState.Verify(s => s.LuaGetGlobal("var1"), Times.Once());
                mState.Verify(s => s.LuaSetGlobal("var1"), Times.Once());
                mState.Verify(s => s.LuaSetGlobal("Var1"), Times.Once());

            }
        }

        [Fact]
        public void TestPop()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                Assert.Null(l.Pop());
                mState.Verify(s => s.LuaPop(1), Times.Never());

                mState.Setup(s => s.LuaGetTop()).Returns(5);
                Assert.Null(l.Pop());
                mState.Verify(s => s.LuaPop(1), Times.Once());
            }
        }

        [Fact]
        public void TestPopGeneric()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                Assert.Null(l.Pop<String>());
                mState.Verify(s => s.LuaPop(1), Times.Never());

                mState.Setup(s => s.LuaGetTop()).Returns(5);
                Assert.Equal(0, l.Pop<int>());
                mState.Verify(s => s.LuaPop(1), Times.Once());
            }
        }

        [Fact]
        public void TestPopValues()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            using (var l = new Lua(state))
            {
                var res = l.PopValues(0);
                Assert.Equal(new object[] { }, res);

                res = l.PopValues(4);
                Assert.Equal(new object[] { null, null, null, null }, res);

            }
        }

    }
}
