using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class ILuaStateExtensionsTest
    {
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
        }

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
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
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
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
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
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
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
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
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
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
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
                var result = l.CallValue(lref, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
            }
        }

    }
}
