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
            public Object[] PublicCallValue(int reference, Object[] args, Type[] typedResult = null) { return CallValue(reference, args, typedResult); }
            public Object PublicGetFieldValue(int reference, String field) { return GetFieldValue(reference, field); }
            public void PublicSetFieldValue(int reference, String field, object value) { SetFieldValue(reference, field, value); }
            public Object PublicGetFieldValue(int reference, int index) { return GetFieldValue(reference, index); }
            public void PublicSetFieldValue(int reference, int index, object value) { SetFieldValue(reference, index, value); }
            public Object PublicGetFieldValue(int reference, object index) { return GetFieldValue(reference, index); }
            public void PublicSetFieldValue(int reference, object index, object value) { SetFieldValue(reference, index, value); }
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
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(123, new object[] { }));
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
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(123, new object[] { }));
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
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(123, new object[] { }));
                Assert.Equal("My error", ex.Message);
                Assert.Same(myEx, ex);
                mState.Verify(s => s.LuaSetTop(5));
            }

        }

        [Fact]
        public void TestCallValue()
        {
            // No results
            int top = 0;
            var mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(()=> { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 0; });
            var state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var result = l.PublicCallValue(123, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Call failed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Returns(() => { top = 1; return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.Nil);
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(123, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Returns(() => { top = 1; return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(-1)).Returns("Error in the call");
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(123, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Error in the call", ex.Message);
            }

            // Multiple results
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var result = l.PublicCallValue(123, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var result = l.PublicCallValue(123, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var result = l.PublicCallValue(123, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            state = mState.Object;
            using (var l = new PublicLua(state))
            {
                var result = l.PublicCallValue(123, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }
        }

        [Fact]
        public void TestSetFieldValueByField()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.PublicSetFieldValue(123, "field1", 1234);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaSetField(It.IsAny<int>(), "field1"), Times.Once());
            }
        }

        [Fact]
        public void TestGetFieldValueByField()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                Assert.Equal(null, l.PublicGetFieldValue(123, "field2"));
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(1));
                mState.Verify(s => s.LuaGetField(It.IsAny<int>(), "field2"), Times.Once());
            }
        }

        [Fact]
        public void TestSetFieldValueByInteger()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.PublicSetFieldValue(123, 777, 1234);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaSetI(It.IsAny<int>(), 777), Times.Once());
            }
        }

        [Fact]
        public void TestGetFieldValueByInteger()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                Assert.Equal(null, l.PublicGetFieldValue(123,888));
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(1));
                mState.Verify(s => s.LuaGetI(It.IsAny<int>(), 888), Times.Once());
            }
        }

        [Fact]
        public void TestSetFieldValueByObject()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.PublicSetFieldValue(123, 777.77, 1234);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushNumber(777.77), Times.Once());
                //mState.Verify(s => s.LuaSetTable(It.IsAny<int>()), Times.Once());
            }
        }

        [Fact]
        public void TestGetFieldValueByObject()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                Assert.Equal(null, l.PublicGetFieldValue(123, 888.88));
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(1));
                mState.Verify(s => s.LuaPushNumber(888.88), Times.Once());
                //mState.Verify(s => s.LuaGetTable(It.IsAny<int>()), Times.Once());
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
                    Assert.Same(l, ud.Lua);
                }
                // Not an userdata
                Assert.Null(l.ToUserData(2));
            }

            // Invalid ref
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.UserData);
            mState.Setup(s => s.LuaIsUserData(1)).Returns(true);
            mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            state = mState.Object;
            using (l = new Lua(state))
            {
                var ioex = Assert.Throws<InvalidOperationException>(() => l.ToUserData(1));
                Assert.Equal("Can't create a reference for this value.", ioex.Message);
            }
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

                var tbl = new LuaTable(l, 9876);
                l.Push(tbl);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 9876), Times.Once());

                var ud = new LuaUserData(l, 8765);
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
                Assert.Throws<NotImplementedException>(() => l.ToValue(9));
                Assert.Same(state, l.ToValue(10));
            }
        }

    }
}
