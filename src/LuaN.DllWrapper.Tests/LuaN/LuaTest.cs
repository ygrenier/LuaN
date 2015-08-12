using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaTest
    {

        class PublicLua : Lua
        {
            public PublicLua(ILuaState state) : base(state) { }
            public Object[] PublicCallValue(int reference, Object[] args, Type[] typedResult = null) { return CallValue(reference, args, typedResult); }
            public Object[] PublicCallFunction(object function, Object[] args, Type[] typedResult = null) { return CallFunction(function, args, typedResult); }
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
            var engine = new LuaEngine();
            Lua l;
            using (l = new Lua(engine))
            {
                Assert.Same(engine, l.State.Engine);

                l.State.LuaPushString("LUAN HOSTED");
                Assert.Equal(LuaType.Boolean, l.State.LuaGetTable(l.State.RegistryIndex));
                Assert.Equal(true, l.ToValue(-1));
            }
            Assert.Throws<ObjectDisposedException>(() => l.State != null);

            Assert.Throws<ArgumentNullException>(() => new Lua((ILuaEngine)null));
        }

        [Fact]
        public void TestCreateFromState()
        {
            // Normal
            var state = new LuaState();
            Lua l;
            using (l = new Lua(state))
            {
                Assert.Same(state, l.State);

                l.State.LuaPushString("LUAN HOSTED");
                Assert.Equal(LuaType.Boolean, l.State.LuaGetTable(l.State.RegistryIndex));
                Assert.Equal(true, l.ToValue(-1));
            }
            Assert.Throws<ObjectDisposedException>(() => l.State != null);
            Assert.Throws<ObjectDisposedException>(() => state.LuaVersion());

            // State not owned
            state = new LuaState();
            using (l = new Lua(state, false))
            {
                Assert.Same(state, l.State);

                l.State.LuaPushString("LUAN HOSTED");
                Assert.Equal(LuaType.Boolean, l.State.LuaGetTable(l.State.RegistryIndex));
                Assert.Equal(true, l.ToValue(-1));
            }
            // Because the state is not owned, there is not disposed
            Assert.Throws<ObjectDisposedException>(() => l.State != null);
            Assert.Equal(503, state.LuaVersion());

            // State already hosted
            var ioex = Assert.Throws<InvalidOperationException>(() => new Lua(state));
            Assert.Equal("This state is already hosted.", ioex.Message);

            Assert.Throws<ArgumentNullException>(() => new Lua((ILuaState)null));
        }

        [Fact]
        public void TestDisposeLuaValue()
        {
            var state = new LuaState();
            Lua l;
            LuaValue v;
            using (l = new Lua(state))
            {
                l.State.LuaNewTable();
                var lref = l.State.LuaRef();
                Assert.Equal(LuaType.Table, l.State.LuaPushRef(lref));
                l.State.LuaPop(1);
                v = new LuaTable(l, lref, true);
                Assert.Equal(lref, v.Reference);
                Assert.Same(l, v.Lua);
                v.Dispose();
                Assert.Null(v.Lua);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                Assert.Equal(LuaType.Nil, l.State.LuaPushRef(lref));
                l.State.LuaPop(1);

                l.State.LuaNewTable();
                lref = l.State.LuaRef();
                Assert.Equal(LuaType.Table, l.State.LuaPushRef(lref));
                l.State.LuaPop(1);
                v = new LuaTable(l, lref, false);
                Assert.Equal(lref, v.Reference);
                Assert.Same(l, v.Lua);
                v.Dispose();
                Assert.Null(v.Lua);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                Assert.Equal(LuaType.Table, l.State.LuaPushRef(lref));
            }
        }

        [Fact]
        public void TestThrowError()
        {
            var state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LuaSetTop(5);
                l.State.LuaPushNil();
                l.State.LuaSetGlobal("err");
                l.State.LoadString("error(err)");
                var fnc = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => fnc.Call());
                Assert.Equal("Unknown Lua error.", ex.Message);
                Assert.Equal(6, l.State.LuaGetTop());
            }

            state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LuaSetTop(5);
                l.State.LuaPushString("Custom error");
                l.State.LuaSetGlobal("err");
                l.State.LoadString("error(err)");
                var fnc = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => fnc.Call());
                Assert.Equal("[string \"error(err)\"]:1: Custom error", ex.Message);
                Assert.Equal(6, l.State.LuaGetTop());
            }

            LuaException myEx = new LuaException("My error");
            state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LuaSetTop(5);
                l.State.LuaPushLightUserData(myEx);
                l.State.LuaSetGlobal("err");
                l.State.LoadString("error(err)");
                var fnc = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => fnc.Call());
                Assert.Equal("My error", ex.Message);
                Assert.Same(myEx, ex);
                Assert.Equal(6, l.State.LuaGetTop());
            }

        }

        [Fact]
        public void TestCallValue()
        {
            // No results
            var state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.LoadString("a=0");
                var lref = l.State.LuaRef();
                var result = l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
            }

            // Call failed
            state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LoadString("error()");
                var lref = l.State.LuaRef();
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LoadString("error('Error in the call')");
                var lref = l.State.LuaRef();
                var ex = Assert.Throws<LuaException>(() => l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("[string \"error('Error in the call')\"]:1: Error in the call", ex.Message);
            }

            // Multiple results
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return b, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.State.LuaRef();
                var result = l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
            }

            // Multiple results typed
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.State.LuaRef();
                var result = l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
            }

            // Multiple results typed
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.State.LuaRef();
                var result = l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
            }

            // Multiple results typed
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.State.LuaRef();
                var result = l.PublicCallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
            }
        }

        [Fact]
        public void TestCallFunction()
        {
            // No results
            var state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.LoadString("a=0");
                var func = l.ToFunction(-1);
                var result = l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
            }

            // Call failed
            state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LoadString("error()");
                var func = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            state = new LuaState();
            state.LuaLOpenLibs();
            using (var l = new PublicLua(state))
            {
                l.State.LoadString("error('Error in the call')");
                var func = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("[string \"error('Error in the call')\"]:1: Error in the call", ex.Message);
            }

            // Multiple results
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return b, d==nil, 123.45, 'Test'
end
return test
");
                var func = l.ToFunction(-1);
                var result = l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
            }

            // Multiple results typed
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var func = l.ToFunction(-1);
                var result = l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
            }

            // Multiple results typed
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                var func = (LuaCFunction)(st=>
                {
                    Assert.Equal(3, st.LuaGetTop());
                    Assert.Equal("field1", st.LuaToString(1));
                    Assert.True(st.LuaIsNil(2));
                    Assert.Equal(12.34, st.LuaToNumber(3));

                    st.LuaPushValue(1);
                    st.LuaPushBoolean(true);
                    st.LuaPushNumber(123.45);
                    st.LuaPushString("Test");
                    return 4;
                });
                var result = l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
            }

            // Multiple results typed
            state = new LuaState();
            using (var l = new PublicLua(state))
            {
                l.State.DoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var func = l.ToFunction(-1);
                var result = l.PublicCallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
            }

        }

        [Fact]
        public void TestSetFieldValueByField()
        {
            var state = new LuaState();
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.State.LuaNewTable();
                var lref = l.State.LuaRef();
                l.PublicSetFieldValue(lref, "field1", 1234);

                l.State.LuaPushRef(lref);
                l.State.LuaGetField(-1, "field1");
                Assert.Equal(1234, l.State.LuaToNumber(-1));
            }
        }

        [Fact]
        public void TestGetFieldValueByField()
        {
            var state = new LuaState();
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.State.LuaNewTable();
                l.State.LuaPushValue(-1);
                var lref = l.State.LuaRef();
                l.State.LuaPushNumber(123.45);
                l.State.LuaSetField(1, "field1");

                Assert.Equal(123.45, l.PublicGetFieldValue(lref, "field1"));
            }
        }

        [Fact]
        public void TestSetFieldValueByInteger()
        {
            var state = new LuaState();
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.State.LuaNewTable();
                var lref = l.State.LuaRef();
                l.PublicSetFieldValue(lref, 24, 1234);

                l.State.LuaPushRef(lref);
                l.State.LuaGetI(-1, 24);
                Assert.Equal(1234, l.State.LuaToNumber(-1));
            }
        }

        [Fact]
        public void TestGetFieldValueByInteger()
        {
            var state = new LuaState();
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.State.LuaNewTable();
                l.State.LuaPushValue(-1);
                var lref = l.State.LuaRef();
                l.State.LuaPushNumber(123.45);
                l.State.LuaSetI(1, 24);

                Assert.Equal(123.45, l.PublicGetFieldValue(lref, 24));
            }
        }

        [Fact]
        public void TestSetFieldValueByObject()
        {
            var state = new LuaState();
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.State.LuaNewTable();
                var lref = l.State.LuaRef();
                l.PublicSetFieldValue(lref, 987.65, 1234);

                l.State.LuaPushRef(lref);
                l.State.LuaPushNumber(987.65);
                l.State.LuaGetTable(-2);
                Assert.Equal(1234, l.State.LuaToNumber(-1));
            }
        }

        [Fact]
        public void TestGetFieldValueByObject()
        {
            var state = new LuaState();
            PublicLua l = null;
            using (l = new PublicLua(state))
            {
                l.State.LuaNewTable();
                l.State.LuaPushValue(-1);
                var lref = l.State.LuaRef();
                l.State.LuaPushNumber(987.65);
                l.State.LuaPushNumber(123.45);
                l.State.LuaSetTable(1);

                Assert.Equal(123.45, l.PublicGetFieldValue(lref, 987.65));
            }
        }

        [Fact]
        public void TestToTable()
        {
            var state = new LuaState();
            Lua l;
            using (l = new Lua(state))
            {
                l.State.LuaNewTable();
                l.State.LuaPushString("Test");
                // Existing table
                using (var table = l.ToTable(1))
                {
                    Assert.IsType<LuaTable>(table);
                    var tb = table as LuaTable;
                    Assert.Same(l, tb.Lua);
                }
                // Not a table
                Assert.Null(l.ToTable(2));
            }

            // Invalid ref
            //state = new LuaState();
            //using (l = new Lua(state))
            //{
            //    l.State.LuaPushNil();
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToTable(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }

        [Fact]
        public void TestToUserData()
        {
            var state = new LuaState();
            Lua l;
            using (l = new Lua(state))
            {
                l.State.LuaPushLightUserData(this);
                l.State.LuaNewUserData(12);
                // Existing light userdata
                using (var userdata = l.ToUserData(1))
                {
                    Assert.IsType<LuaUserData>(userdata);
                    var ud = userdata as LuaUserData;
                    Assert.Same(l, ud.Lua);
                }
                // Existing userdata
                using (var userdata = l.ToUserData(2))
                {
                    Assert.IsType<LuaUserData>(userdata);
                    var ud = userdata as LuaUserData;
                    Assert.Same(l, ud.Lua);
                }
                // Not an userdata
                Assert.Null(l.ToUserData(3));
            }

            //// Invalid ref
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

            var state = new LuaState();
            Lua l;
            using (l = new Lua(state))
            {
                l.State.LuaPushCFunction(func);
                l.State.LuaLLoadString("return 123");
                // Existing c function
                using (var function = l.ToFunction(1))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(l, fn.Lua);
                    Assert.Same(func, fn.Function);
                }
                // Existing lua function
                using (var function = l.ToFunction(2))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(l, fn.Lua);
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
            var state = new LuaState();
            Lua l;
            using (l = new Lua(state))
            {
                l.Push(null);
                Assert.Equal(LuaType.Nil, l.State.LuaType(-1));

                l.Push(true);
                Assert.Equal(LuaType.Boolean, l.State.LuaType(-1));
                Assert.Equal(true, l.State.LuaToBoolean(-1));

                l.Push(false);
                Assert.Equal(LuaType.Boolean, l.State.LuaType(-1));
                Assert.Equal(false, l.State.LuaToBoolean(-1));

                l.Push(12f);
                Assert.Equal(LuaType.Number, l.State.LuaType(-1));
                Assert.Equal(12d, l.State.LuaToNumber(-1));

                l.Push(34.56d);
                Assert.Equal(LuaType.Number, l.State.LuaType(-1));
                Assert.Equal(34.56d, l.State.LuaToNumber(-1));

                l.Push(78.9m);
                Assert.Equal(LuaType.Number, l.State.LuaType(-1));
                Assert.Equal(78.9d, l.State.LuaToNumber(-1));

                l.Push(98);
                Assert.Equal(LuaType.Number, l.State.LuaType(-1));
                Assert.Equal(98, l.State.LuaToNumber(-1));

                l.Push("Test");
                Assert.Equal(LuaType.String, l.State.LuaType(-1));
                Assert.Equal("Test", l.State.LuaToString(-1));

                var ioex = Assert.Throws<InvalidOperationException>(() => l.Push(new Mock<ILuaNativeUserData>().Object));
                Assert.Equal("Can't push a userdata", ioex.Message);

                LuaCFunction func = st => 0;
                l.Push(func);
                Assert.Equal(LuaType.Function, l.State.LuaType(-1));
                Assert.Same(func, l.State.LuaToCFunction(-1));

                l.Push(l.State);
                Assert.Equal(LuaType.Thread, l.State.LuaType(-1));
                Assert.Same(l.State, l.State.LuaToThread(-1));
                ioex = Assert.Throws<InvalidOperationException>(() => l.Push(new Mock<ILuaState>().Object));
                Assert.Equal("Can't push a different thread", ioex.Message);

                l.State.LuaNewTable();
                var tbl = new LuaTable(l, l.State.LuaRef());
                l.Push(tbl);
                Assert.Equal(LuaType.Table, l.State.LuaType(-1));

                l.State.LuaNewUserData(12);
                var ud = new LuaUserData(l, l.State.LuaRef());
                l.Push(ud);
                Assert.Equal(LuaType.UserData, l.State.LuaType(-1));

                l.Push(this);
                Assert.Equal(LuaType.LightUserData, l.State.LuaType(-1));
            }
        }

        [Fact]
        public void TestToValue()
        {
            Lua l = null;
            var state = new LuaState();
            using (l = new Lua(state))
            {
                l.State.LuaPushNil();
                l.State.LuaPushBoolean(true);
                l.State.LuaPushBoolean(false);
                l.State.LuaPushNumber(123.45);
                l.State.LuaPushString("Test");
                l.State.LuaPushLightUserData(this);
                l.State.LuaNewUserData(12);
                l.State.LuaNewTable();
                l.State.LuaPushCFunction(s => 0);
                l.State.LuaPushThread();

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
            String filename = Path.GetTempFileName();
            using (var l=new Lua(new LuaState()))
            {
                File.WriteAllText(filename, "return 12.34, nil, false, 'test'");
                var result = l.DoFile(filename);
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                File.WriteAllText(filename, "return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => l.DoFile(filename));
                Assert.Equal(filename + ":1: <eof> expected near 'false'", ex.Message);
            }
            File.Delete(filename);
        }

        [Fact]
        public void TestDoStringText()
        {
            using (var l = new Lua(new LuaState()))
            {
                var result = l.DoString("return 12.34, nil, false, 'test'", "myScript");
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                var ex = Assert.Throws<LuaException>(() => l.DoString("return 12.34, nil false, 'test'", "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);
            }
        }

        [Fact]
        public void TestDoStringBinary()
        {
            using (var l = new Lua(new LuaState()))
            {
                byte[] bytes = Encoding.UTF8.GetBytes("return 12.34, nil, false, 'test'");
                var result = l.DoString(bytes, "myScript");
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                bytes = Encoding.UTF8.GetBytes("return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => l.DoString(bytes, "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);

                // Compile
                using (var ms = new MemoryStream())
                {
                    l.State.LoadString("return 12.34, nil, false, 'test'");
                    l.State.LuaDump((s, p, c) => { ms.Write(p, 0, p.Length); return 0; }, null, true);
                    bytes = ms.ToArray();
                }
                result = l.DoString(bytes, "myCompiledScript");
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                ex = Assert.Throws<LuaException>(() => l.DoString(bytes.Take(6).ToArray(), "myCompiledScript"));
                Assert.Equal("myCompiledScript: truncated precompiled chunk", ex.Message);

            }
        }

        [Fact]
        public void TestLoadFile()
        {
            String filename = Path.GetTempFileName();
            using (var l = new Lua(new LuaState()))
            {
                File.WriteAllText(filename, "return 12.34, nil, false, 'test'");
                var function= l.LoadFile(filename);
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                File.WriteAllText(filename, "return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => l.LoadFile(filename));
                Assert.Equal(filename + ":1: <eof> expected near 'false'", ex.Message);
            }
            File.Delete(filename);
        }

        [Fact]
        public void TestLoadStringText()
        {
            using (var l = new Lua(new LuaState()))
            {
                var function = l.LoadString("return 12.34, nil, false, 'test'", "myScript");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                var ex = Assert.Throws<LuaException>(() => l.LoadString("return 12.34, nil false, 'test'", "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);
            }
        }

        [Fact]
        public void TestLoadStringBinary()
        {
            using (var l = new Lua(new LuaState()))
            {
                byte[] bytes = Encoding.UTF8.GetBytes("return 12.34, nil, false, 'test'");
                var function = l.LoadString(bytes, "myScript");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                bytes = Encoding.UTF8.GetBytes("return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => l.LoadString(bytes, "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);

                // Compile
                using (var ms = new MemoryStream())
                {
                    l.State.LoadString("return 12.34, nil, false, 'test'");
                    l.State.LuaDump((s, p, c) => { ms.Write(p, 0, p.Length); return 0; }, null, true);
                    bytes = ms.ToArray();
                }
                function = l.LoadString(bytes, "myScript");
                Assert.NotNull(function);
                result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                ex = Assert.Throws<LuaException>(() => l.LoadString(bytes.Take(6).ToArray(), "myCompiledScript"));
                Assert.Equal("myCompiledScript: truncated precompiled chunk", ex.Message);

            }
        }

        [Fact]
        public void TestGlobalsAccess()
        {
            using (var l = new Lua(new LuaState()))
            {
                Assert.Null(l["var1"]);
                l["Var1"] = "One";
                l["var1"] = 123.45;
                Assert.Equal(123.45, l["var1"]);
                Assert.Equal("One", l["Var1"]);
            }
        }

        [Fact]
        public void TestPop()
        {
            using (var l = new Lua(new LuaState()))
            {
                l.Push(123.45);
                l.Push(this);
                l.Push("Test");

                Assert.Equal(3, l.State.LuaGetTop());

                Assert.Equal("Test", l.Pop());
                Assert.Equal(2, l.State.LuaGetTop());
                Assert.Equal(this, l.Pop());
                Assert.Equal(1, l.State.LuaGetTop());
                Assert.Equal(123.45, l.Pop());
                Assert.Equal(0, l.State.LuaGetTop());
                Assert.Equal(null, l.Pop());
                Assert.Equal(0, l.State.LuaGetTop());

                l.Push(123.45);
                l.Push(this);
                l.Push("Test");

                Assert.Equal(3, l.State.LuaGetTop());

                Assert.Equal(0, l.Pop<int>());
                Assert.Equal(2, l.State.LuaGetTop());
                Assert.Equal(null, l.Pop<LuaState>());
                Assert.Equal(1, l.State.LuaGetTop());
                Assert.Equal("123.45", l.Pop<String>());
                Assert.Equal(0, l.State.LuaGetTop());
                Assert.Equal(null, l.Pop());
                Assert.Equal(0, l.State.LuaGetTop());
            }
        }

        [Fact]
        public void TestPopValues()
        {
            using (var l = new Lua(new LuaState()))
            {
                l.Push(123.45);
                l.Push(this);
                l.Push("Test");
                l.Push(true);
                l.Push(987);
                l.Push(false);
                l.State.LuaPop(2);

                Assert.Equal(4, l.State.LuaGetTop());

                var res = l.PopValues(2);

                Assert.Equal(2, l.State.LuaGetTop());
                Assert.Equal(new Object[] { "Test", true }, res);

                res = l.PopValues(5);

                Assert.Equal(0, l.State.LuaGetTop());
                Assert.Equal(new Object[] { 123.45, this, null, null, null }, res);

            }
        }

    }
}
