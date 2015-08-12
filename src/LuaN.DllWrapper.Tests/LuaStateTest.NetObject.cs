using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    partial class LuaStateTest
    {

		[Fact]
		public void TestToObject()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal(null, L.ToObject(1));
                Assert.Equal(123.45d, L.ToObject(2));
                Assert.Equal(987d, L.ToObject(3));
                Assert.Equal("Test", L.ToObject(4));
                Assert.Equal("5.6", L.ToObject(5));
                Assert.Equal("5D", L.ToObject(6));
                Assert.Equal("5z", L.ToObject(7));
                Assert.Equal(true, L.ToObject(8));
                Assert.IsAssignableFrom<ILuaFunction>(L.ToObject(9));
                Assert.IsAssignableFrom<ILuaFunction>(L.ToObject(10));
                Assert.IsAssignableFrom<ILuaUserData>(L.ToObject(11));
                Assert.Same(this, L.ToObject(12));
                Assert.IsAssignableFrom<ILuaTable>(L.ToObject(13));
                Assert.Same(L, L.ToObject(14));
            }
        }

		[Fact]
		public void TestPush()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                var ud = L.LuaNewUserData(20);
                Assert.Equal(1, L.LuaGetTop());
                L.LuaSetGlobal("UserData");
                Assert.Equal(0, L.LuaGetTop());
                var th = L.LuaNewThread();
                Assert.Equal(1, L.LuaGetTop());
                L.LuaSetGlobal("Thread");
                Assert.Equal(0, L.LuaGetTop());

                L.Push(null);
                L.Push(123.45);
                L.Push(987);
                L.Push("Test");
                Assert.Throws<InvalidOperationException>(() => L.Push(ud));
                L.Push(true);
                L.Push(false);
                LuaCFunction fn = s => 0;
                L.Push(fn);
                L.Push(L);
                Assert.Throws<InvalidOperationException>(() => L.Push(th));
                var dt = DateTime.Now;
                L.Push(dt);


                Assert.Equal(LuaType.Nil, L.LuaType(1));
                Assert.Equal(LuaType.Number, L.LuaType(2));
                Assert.Equal(123.45d, L.LuaToNumber(2));
                Assert.Equal(LuaType.Number, L.LuaType(3));
                Assert.Equal(987d, L.LuaToNumber(3));
                Assert.Equal(LuaType.String, L.LuaType(4));
                Assert.Equal("Test", L.LuaToString(4));
                Assert.Equal(LuaType.Boolean, L.LuaType(5));
                Assert.Equal(true, L.LuaToBoolean(5));
                Assert.Equal(LuaType.Boolean, L.LuaType(6));
                Assert.Equal(false, L.LuaToBoolean(6));
                Assert.Equal(LuaType.Function, L.LuaType(7));
                Assert.Same(fn, L.LuaToCFunction(7));
                Assert.Equal(LuaType.Thread, L.LuaType(8));
                Assert.Same(L, L.LuaToThread(8));
                Assert.Equal(LuaType.LightUserData, L.LuaType(9));
                Assert.Equal(dt, L.LuaToUserData(9));

            }
        }

    }
}
