using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{

    public class LuaFunctionTest
    {

        [Fact]
        public void TestCreateRef()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaFunction v;
            using (l = new Lua(state))
            {
                v = new LuaFunction(l, 123, true);
                Assert.Same(l, v.Lua);
                Assert.Equal(123, v.Reference);
                Assert.Null(v.Function);
                v.Dispose();
            }
        }

        [Fact]
        public void TestCreateFunc()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaFunction v;
            using (l = new Lua(state))
            {
                LuaCFunction func = s => 0;
                v = new LuaFunction(l, func);
                Assert.Same(l, v.Lua);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                Assert.Same(func, v.Function);
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
            var state = mState.Object;
            Lua l;
            LuaFunction fn;
            using (l = new Lua(state))
            {
                fn = new LuaFunction(l, 123, true);
                Assert.Equal(new Object[] { null, true, 123.45, "Test" }, fn.Call(true, null, 1234, "Test"));

                top = 1;
                fn = new LuaFunction(l, s => 0);
                Assert.Equal(new Object[] { null, true, 123.45, "Test" }, fn.Call(true, null, 1234, "Test"));
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
            var state = mState.Object;
            Lua l;
            LuaFunction fn;
            using (l = new Lua(state))
            {
                fn = new LuaFunction(l, 123, true);
                Assert.Equal(new Object[] { null, "True", 123 }, fn.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) }));

                top = 1;
                fn = new LuaFunction(l, s => 0);
                Assert.Equal(new Object[] { null, "True", 123 }, fn.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) }));
            }
        }

    }

}
