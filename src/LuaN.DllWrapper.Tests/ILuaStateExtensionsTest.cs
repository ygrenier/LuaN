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
    public class ILuaStateExtensionsTest
    {

        [Fact]
        public void TestRef()
        {
            using (ILuaState L = new LuaState())
            {
                L.LuaPushNumber(123);
                var lref = L.LuaRef();

                Assert.Equal(0, L.LuaGetTop());

                Assert.Equal(LuaType.Number, L.LuaPushRef(lref));
                Assert.Equal(123, L.LuaToNumber(-1));

                Assert.Equal(LuaType.Number, L.LuaPushRef(L.RegistryIndex, lref));
                Assert.Equal(123, L.LuaToNumber(-1));

                L.LuaUnref(lref);

                Assert.Equal(LuaType.Nil, L.LuaPushRef(lref));
            }
        }

        [Fact]
        public void TestCallFunction()
        {
            // No results
            using (var l = new LuaState())
            {
                l.LuaLoadString("a=0");
                var func = l.ToFunction(-1);
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
            }

            // Call failed
            using (var l = new LuaState())
            {
                l.LuaLOpenLibs();
                l.LuaLoadString("error()");
                var func = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => l.CallFunction(func, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            using (var l = new LuaState())
            {
                l.LuaLOpenLibs();
                l.LuaLoadString("error('Error in the call')");
                var func = l.ToFunction(-1);
                var ex = Assert.Throws<LuaException>(() => l.CallFunction(func, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("[string \"error('Error in the call')\"]:1: Error in the call", ex.Message);
            }

            // Multiple results
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return b, d==nil, 123.45, 'Test'
end
return test
");
                var func = l.ToFunction(-1);
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
            }

            // Multiple results typed
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var func = l.ToFunction(-1);
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(LuaState), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
            }

            // Multiple results typed
            using (var l = new LuaState())
            {
                var func = (LuaCFunction)(st =>
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
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(LuaState), typeof(LuaState), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
            }

            // Multiple results typed
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var func = l.ToFunction(-1);
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(LuaState), typeof(LuaState), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
            }

        }

        [Fact]
        public void TestCallValue()
        {
            // No results
            using (var l = new LuaState())
            {
                l.LuaLoadString("a=0");
                var lref = l.LuaRef();
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
            }

            // Call failed
            using (var l = new LuaState())
            {
                l.LuaLOpenLibs();
                l.LuaLoadString("error()");
                var lref = l.LuaRef();
                var ex = Assert.Throws<LuaException>(() => l.CallValue(lref, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            using (var l = new LuaState())
            {
                l.LuaLOpenLibs();
                l.LuaLoadString("error('Error in the call')");
                var lref = l.LuaRef();
                var ex = Assert.Throws<LuaException>(() => l.CallValue(lref, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("[string \"error('Error in the call')\"]:1: Error in the call", ex.Message);
            }

            // Multiple results
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return b, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.LuaRef();
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
            }

            // Multiple results typed
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.LuaRef();
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(LuaState), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
            }

            // Multiple results typed
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.LuaRef();
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(LuaState), typeof(LuaState), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
            }

            // Multiple results typed
            using (var l = new LuaState())
            {
                l.LuaDoString(@"
function test(a,b,c,d)
 return a, d==nil, 123.45, 'Test'
end
return test
");
                var lref = l.LuaRef();
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(LuaState), typeof(LuaState), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
            }
        }

        [Fact]
        public void TestPop()
        {
            using (var l = new LuaState())
            {
                l.Push(123.45);
                l.Push(this);
                l.Push("Test");

                Assert.Equal(3, l.LuaGetTop());

                Assert.Equal("Test", l.Pop());
                Assert.Equal(2, l.LuaGetTop());
                Assert.Equal(this, l.Pop());
                Assert.Equal(1, l.LuaGetTop());
                Assert.Equal(123.45, l.Pop());
                Assert.Equal(0, l.LuaGetTop());
                Assert.Equal(null, l.Pop());
                Assert.Equal(0, l.LuaGetTop());

                l.Push(123.45);
                l.Push(this);
                l.Push("Test");

                Assert.Equal(3, l.LuaGetTop());

                Assert.Equal(0, l.Pop<int>());
                Assert.Equal(2, l.LuaGetTop());
                Assert.Equal(null, l.Pop<LuaState>());
                Assert.Equal(1, l.LuaGetTop());
                Assert.Equal("123.45", l.Pop<String>());
                Assert.Equal(0, l.LuaGetTop());
                Assert.Equal(null, l.Pop());
                Assert.Equal(0, l.LuaGetTop());
            }
        }

        [Fact]
        public void TestPopValues()
        {
            using (var l = new LuaState())
            {
                l.Push(123.45);
                l.Push(this);
                l.Push("Test");
                l.Push(true);
                l.Push(987);
                l.Push(false);
                l.LuaPop(2);

                Assert.Equal(4, l.LuaGetTop());

                var res = l.PopValues(2);

                Assert.Equal(2, l.LuaGetTop());
                Assert.Equal(new Object[] { "Test", true }, res);

                res = l.PopValues(5);

                Assert.Equal(0, l.LuaGetTop());
                Assert.Equal(new Object[] { 123.45, this, null, null, null }, res);

            }
        }

        [Fact]
        public void TestToTable()
        {
            using (var state = new LuaState())
            {
                state.LuaNewTable();
                state.LuaPushString("Test");
                // Existing table
                using (var table = state.ToTable(1))
                {
                    Assert.IsType<LuaTable>(table);
                    var tb = table as LuaTable;
                    Assert.Same(state, tb.State);
                }
                // Not a table
                Assert.Null(state.ToTable(2));
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
            using (ILuaState L = new LuaState())
            {
                L.LuaPushNumber(123);
                Assert.Null(L.ToUserData<ILuaStateExtensionsTest>(-1));
                Assert.Null(L.ToUserData<LuaState>(-1));
                Assert.Equal(DateTime.MinValue, L.ToUserData<DateTime>(-1));

                L.LuaPushLightUserData(this);
                Assert.Same(this, L.ToUserData<ILuaStateExtensionsTest>(-1));
                Assert.Null(L.ToUserData<LuaState>(-1));
                Assert.Equal(DateTime.MinValue, L.ToUserData<DateTime>(-1));
            }

            using (var state = new LuaState())
            {
                state.LuaPushLightUserData(this);
                state.LuaNewUserData(12);
                // Existing light userdata
                using (var userdata = state.ToUserData(1))
                {
                    Assert.IsType<LuaUserData>(userdata);
                    var ud = userdata as LuaUserData;
                    Assert.Same(state, ud.State);
                }
                // Existing userdata
                using (var userdata = state.ToUserData(2))
                {
                    Assert.IsType<LuaUserData>(userdata);
                    var ud = userdata as LuaUserData;
                    Assert.Same(state, ud.State);
                }
                // Not an userdata
                Assert.Null(state.ToUserData(3));
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
            using (var state = new LuaState())
            {
                state.LuaPushCFunction(func);
                state.LuaLLoadString("return 123");
                // Existing c function
                using (var function = state.ToFunction(1))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(state, fn.State);
                    Assert.Same(func, fn.Function);
                }
                // Existing lua function
                using (var function = state.ToFunction(2))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(state, fn.State);
                }
                // Not an LuaFunction
                Assert.Null(state.ToFunction(3));
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
        public void TestToValue()
        {
            using (var state = new LuaState())
            {
                state.LuaPushNil();
                state.LuaPushBoolean(true);
                state.LuaPushBoolean(false);
                state.LuaPushNumber(123.45);
                state.LuaPushString("Test");
                state.LuaPushLightUserData(this);
                state.LuaNewUserData(12);
                state.LuaNewTable();
                state.LuaPushCFunction(s => 0);
                state.LuaPushThread();

                Assert.Equal(null, state.ToValue(1));
                Assert.Equal(true, state.ToValue(2));
                Assert.Equal(false, state.ToValue(3));
                Assert.Equal(123.45, state.ToValue(4));
                Assert.Equal("Test", state.ToValue(5));
                Assert.Same(this, state.ToValue(6));
                Assert.IsAssignableFrom<ILuaUserData>(state.ToValue(7));
                var tbl = state.ToValue(8);
                Assert.IsAssignableFrom<ILuaTable>(tbl);
                Assert.IsAssignableFrom<ILuaFunction>(state.ToValue(9));
                Assert.Same(state, state.ToValue(10));
            }
        }

        [Fact]
        public void TestPush()
        {
            using (var state = new LuaState())
            {
                state.Push(null);
                Assert.Equal(LuaType.Nil, state.LuaType(-1));

                state.Push(true);
                Assert.Equal(LuaType.Boolean, state.LuaType(-1));
                Assert.Equal(true, state.LuaToBoolean(-1));

                state.Push(false);
                Assert.Equal(LuaType.Boolean, state.LuaType(-1));
                Assert.Equal(false, state.LuaToBoolean(-1));

                state.Push(12f);
                Assert.Equal(LuaType.Number, state.LuaType(-1));
                Assert.Equal(12d, state.LuaToNumber(-1));

                state.Push(34.56d);
                Assert.Equal(LuaType.Number, state.LuaType(-1));
                Assert.Equal(34.56d, state.LuaToNumber(-1));

                state.Push(78.9m);
                Assert.Equal(LuaType.Number, state.LuaType(-1));
                Assert.Equal(78.9d, state.LuaToNumber(-1));

                state.Push(98);
                Assert.Equal(LuaType.Number, state.LuaType(-1));
                Assert.Equal(98, state.LuaToNumber(-1));

                state.Push("Test");
                Assert.Equal(LuaType.String, state.LuaType(-1));
                Assert.Equal("Test", state.LuaToString(-1));

                var ioex = Assert.Throws<InvalidOperationException>(() => state.Push(new Mock<ILuaNativeUserData>().Object));
                Assert.Equal("Can't push a userdata", ioex.Message);

                LuaCFunction func = st => 0;
                state.Push(func);
                Assert.Equal(LuaType.Function, state.LuaType(-1));
                Assert.Same(func, state.LuaToCFunction(-1));

                state.Push(state);
                Assert.Equal(LuaType.Thread, state.LuaType(-1));
                Assert.Same(state, state.LuaToThread(-1));
                ioex = Assert.Throws<InvalidOperationException>(() => state.Push(new Mock<ILuaState>().Object));
                Assert.Equal("Can't push a different thread", ioex.Message);

                state.LuaNewTable();
                var tbl = new LuaTable(state, state.LuaRef());
                state.Push(tbl);
                Assert.Equal(LuaType.Table, state.LuaType(-1));

                state.LuaNewUserData(12);
                var ud = new LuaUserData(state, state.LuaRef());
                state.Push(ud);
                Assert.Equal(LuaType.UserData, state.LuaType(-1));

                state.Push(this);
                Assert.Equal(LuaType.LightUserData, state.LuaType(-1));
            }
        }

        [Fact]
        public void TestDoFile()
        {
            String filename = Path.GetTempFileName();
            using (var state = new LuaState())
            {
                File.WriteAllText(filename, "return 12.34, nil, false, 'test'");
                var result = state.DoFile(filename);
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                File.WriteAllText(filename, "return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => state.DoFile(filename));
                Assert.Equal(filename + ":1: <eof> expected near 'false'", ex.Message);
            }
            File.Delete(filename);
        }

        [Fact]
        public void TestDoStringText()
        {
            using (var state = new LuaState())
            {
                var result = state.DoString("return 12.34, nil, false, 'test'", "myScript");
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                var ex = Assert.Throws<LuaException>(() => state.DoString("return 12.34, nil false, 'test'", "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);
            }
        }

        [Fact]
        public void TestDoStringBinary()
        {
            using (var state = new LuaState())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("return 12.34, nil, false, 'test'");
                var result = state.DoString(bytes, "myScript");
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                bytes = Encoding.UTF8.GetBytes("return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => state.DoString(bytes, "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);

                // Compile
                using (var ms = new MemoryStream())
                {
                    state.LuaLoadString("return 12.34, nil, false, 'test'");
                    state.LuaDump((s, p, c) => { ms.Write(p, 0, p.Length); return 0; }, null, true);
                    bytes = ms.ToArray();
                }
                result = state.DoString(bytes, "myCompiledScript");
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                ex = Assert.Throws<LuaException>(() => state.DoString(bytes.Take(6).ToArray(), "myCompiledScript"));
                Assert.Equal("myCompiledScript: truncated precompiled chunk", ex.Message);

            }
        }

        [Fact]
        public void TestLoadFile()
        {
            String filename = Path.GetTempFileName();
            using (var state = new LuaState())
            {
                File.WriteAllText(filename, "return 12.34, nil, false, 'test'");
                var function = state.LoadFile(filename);
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                File.WriteAllText(filename, "return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => state.LoadFile(filename));
                Assert.Equal(filename + ":1: <eof> expected near 'false'", ex.Message);
            }
            File.Delete(filename);
        }

        [Fact]
        public void TestLoadStringText()
        {
            using (var state = new LuaState())
            {
                var function = state.LoadString("return 12.34, nil, false, 'test'", "myScript");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                var ex = Assert.Throws<LuaException>(() => state.LoadString("return 12.34, nil false, 'test'", "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);
            }
        }

        [Fact]
        public void TestLoadStringBinary()
        {
            using (var state = new LuaState())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("return 12.34, nil, false, 'test'");
                var function = state.LoadString(bytes, "myScript");
                Assert.NotNull(function);
                var result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                bytes = Encoding.UTF8.GetBytes("return 12.34, nil false, 'test'");
                var ex = Assert.Throws<LuaException>(() => state.LoadString(bytes, "myScript"));
                Assert.Equal("[string \"myScript\"]:1: <eof> expected near 'false'", ex.Message);

                // Compile
                using (var ms = new MemoryStream())
                {
                    state.LuaLoadString("return 12.34, nil, false, 'test'");
                    state.LuaDump((s, p, c) => { ms.Write(p, 0, p.Length); return 0; }, null, true);
                    bytes = ms.ToArray();
                }
                function = state.LoadString(bytes, "myScript");
                Assert.NotNull(function);
                result = function.Call();
                Assert.Equal(new object[] { 12.34d, null, false, "test" }, result);

                ex = Assert.Throws<LuaException>(() => state.LoadString(bytes.Take(6).ToArray(), "myCompiledScript"));
                Assert.Equal("myCompiledScript: truncated precompiled chunk", ex.Message);

            }
        }

    }
}
