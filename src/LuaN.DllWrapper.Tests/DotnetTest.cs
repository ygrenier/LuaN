using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class DotnetTest
    {

        [Fact]
        public void TestToStringMeta()
        {
            using (var state=new LuaState())
            {
                state.RegisterDotnetMetatable();

                DateTime dt = DateTime.Now;
                state.PushNetObject(dt);
                state.LuaPushValue(-1);
                Assert.True(state.LuaGetMetatable(-1));

                Assert.Equal(dt.ToString(), state.LuaLToString(1));
            }
        }

        class TestClass
        {
            private Dictionary<String, String> _Values = new Dictionary<string, string>()
            {
                { "Value1", "Value1" },
                { "Value2", "Value2" },
            };
            private int PrivateField;
            public String PublicField;
            public int Property1 { get { return PrivateField; } set { PrivateField = value; } }
            public String Property2 { get; set; }
            public int Property3 { get { return PrivateField; } }
            public String this[String name] { get { return _Values[name]; } set { _Values[name] = value; } }
            public String this[int index] { get { return String.Format("Value {0}", index); } }
        }

        [Fact]
        public void TestIndexMeta()
        {
            using (var state = new LuaState())
            {
                state.RegisterDotnetMetatable();

                var obj = new TestClass
                {
                    PublicField = "Field",
                    Property1 = 123,
                    Property2 = "Property"
                };
                state.PushNetObject(obj);
                state.LuaSetGlobal("obj");

                // Read
                state.LuaDoString("return obj.PublicField");
                Assert.Equal("Field", state.ToValue(-1));

                state.LuaDoString("return obj.Property1");
                Assert.Equal(123d, state.ToValue(-1));

                state.LuaDoString("return obj.Property2");
                Assert.Equal("Property", state.ToValue(-1));

                state.LuaDoString("return obj['Property3']");
                Assert.Equal(123d, state.ToValue(-1));

                Assert.Equal(LuaStatus.Ok, state.LuaDoString("return obj.Value1"));
                Assert.Equal("Value1", state.ToValue(-1));

                Assert.Equal(LuaStatus.Ok, state.LuaDoString("return obj[25]"));
                Assert.Equal("Value 25", state.ToValue(-1));

                Assert.Equal(LuaStatus.ErrorRun, state.LuaDoString("return obj.PrivateField"));
                //Assert.Equal("[string \"return obj.PrivateField\"]:1: Unknown member 'PrivateField' on object 'LuaN.DllWrapper.Tests.DotnetTest+TestClass'.", state.ToValue(-1));
                Assert.Equal("[string \"return obj.PrivateField\"]:1: La clé donnée était absente du dictionnaire.", state.ToValue(-1));

                Assert.Equal(LuaStatus.ErrorRun, state.LuaDoString("return obj.Unknown"));
                //Assert.Equal("[string \"return obj.Unknown\"]:1: Unknown member 'Unknown' on object 'LuaN.DllWrapper.Tests.DotnetTest+TestClass'.", state.ToValue(-1));
                Assert.Equal("[string \"return obj.Unknown\"]:1: La clé donnée était absente du dictionnaire.", state.ToValue(-1));

                // Write
                state.LuaDoString("obj.PublicField = 123.45; return obj.PublicField");
                Assert.Equal("123.45", state.ToValue(-1));

                state.LuaDoString("obj.Property1 = '321'; return obj.Property1");
                Assert.Equal(321d, state.ToValue(-1));

                state.LuaDoString("obj.Property2 = 'NewProperty'; return obj.Property2");
                Assert.Equal("NewProperty", state.ToValue(-1));

                Assert.Equal(LuaStatus.ErrorRun, state.LuaDoString("obj['Property3'] = 'New value';"));
                Assert.Equal("[string \"obj['Property3'] = 'New value';\"]:1: Attempt to set the readonly 'Property3' property on object 'LuaN.DllWrapper.Tests.DotnetTest+TestClass'.", state.ToValue(-1));

                Assert.Equal(LuaStatus.Ok, state.LuaDoString("obj.Value1 = 'New Value 1'; return obj.Value1"));
                Assert.Equal("New Value 1", state.ToValue(-1));

                Assert.Equal(LuaStatus.ErrorRun, state.LuaDoString("obj[25] = 'New 25'; return obj[25]"));
                Assert.Equal("[string \"obj[25] = 'New 25'; return obj[25]\"]:1: Attempt to set the readonly indexed property.", state.ToValue(-1));

            }
        }

        //[Fact]
        //public void TestGcMeta()
        //{
        //    int gcount = 0;
        //    using (var state = new LuaState())
        //    {
        //        state.RegisterDotnetMetatable();

        //        state.LuaLNewMetatable("mt");
        //        state.LuaPushValue(1);

        //        state.LuaPushValue(-1);
        //        state.LuaPushCFunction(s=>
        //        {
        //            gcount++;
        //            return 0;
        //        });
        //        state.LuaSetField(-2, "__gc");

        //        DateTime dt = DateTime.Now;
        //        state.LuaPushLightUserData(dt);
        //        state.LuaLSetMetatable("mt");

        //        var ud = state.LuaNewUserData(12);
        //        state.LuaLSetMetatable("mt");

        //        Assert.NotNull(state.UserDataIndex.FindData(dt));

        //        state.LuaSetTop(0);
        //        state.LuaGC(LuaGcFunction.Collect, 0);
        //    }
        //    Assert.Equal(1, gcount);
        //}

    }
}
