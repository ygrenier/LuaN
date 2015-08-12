using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaTableTest
    {
        [Fact]
        public void TestCreate()
        {
            LuaValue v;
            using (var state = new LuaState())
            {
                v = new LuaTable(state, 123, true);
                Assert.Same(state, v.State);
                Assert.Equal(123, v.Reference);
                v.Dispose();
            }
        }

        [Fact]
        public void TestAccesByField()
        {
            ILuaTable tbl;
            using (var state = new LuaState())
            {
                state.LuaDoString(@"
table = { field1 = 1234, ['field 2'] = 'Test' }
");
                state.LuaGetGlobal("table");
                using (tbl = state.ToTable(-1))
                {
                    Assert.Equal(1234d, tbl["field1"]);
                    Assert.Equal(null, tbl["field2"]);
                    Assert.Equal("Test", tbl["field 2"]);

                    tbl["field1"] = null;
                    tbl["field2"] = 4321;
                }
                Assert.Equal(1, state.LuaGetTop());
                state.LuaDoString(@"return table['field1'], table.field2, table['field 2']");
                Assert.Equal(4, state.LuaGetTop());
                Assert.Equal(null, state.ToValue(-3));
                Assert.Equal(4321d, state.ToValue(-2));
                Assert.Equal("Test", state.ToValue(-1));
            }
        }

        [Fact]
        public void TestAccesByInteger()
        {
            ILuaTable tbl;
            using (var state = new LuaState())
            {
                state.LuaDoString(@"
table = { 1234, [3] = 'Test' }
");
                state.LuaGetGlobal("table");
                using (tbl = state.ToTable(-1))
                {
                    Assert.Equal(1234d, tbl[1]);
                    Assert.Equal(null, tbl[2]);
                    Assert.Equal("Test", tbl[3]);

                    tbl[1] = null;
                    tbl[2] = 4321;
                }
                Assert.Equal(1, state.LuaGetTop());
                state.LuaDoString(@"return table[1], table[2], table[3]");
                Assert.Equal(4, state.LuaGetTop());
                Assert.Equal(null, state.ToValue(-3));
                Assert.Equal(4321d, state.ToValue(-2));
                Assert.Equal("Test", state.ToValue(-1));
            }
        }

        [Fact]
        public void TestAccesByObject()
        {
            ILuaTable tbl;
            using (var state = new LuaState())
            {
                state.LuaPushLightUserData(this);
                state.LuaSetGlobal("idx2");
                state.LuaDoString(@"
idx1 = 'field1'
idx3 = 123.45
table = { [idx1] = 1234, [idx3] = 'Test' }
");
                state.LuaGetGlobal("table");
                using (tbl = state.ToTable(-1))
                {
                    Assert.Equal(1234d, tbl["field1"]);
                    Assert.Equal(null, tbl[this]);
                    Assert.Equal("Test", tbl[123.45]);

                    tbl["field1"] = null;
                    tbl[this] = 4321;
                }
                Assert.Equal(1, state.LuaGetTop());
                state.LuaDoString(@"return table[idx1], table[idx2], table[idx3]");
                Assert.Equal(4, state.LuaGetTop());
                Assert.Equal(null, state.ToValue(-3));
                Assert.Equal(4321d, state.ToValue(-2));
                Assert.Equal("Test", state.ToValue(-1));
            }
        }

    }
}
