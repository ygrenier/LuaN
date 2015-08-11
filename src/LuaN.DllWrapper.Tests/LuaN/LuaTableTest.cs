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
            var state = new LuaState();
            Lua l;
            LuaValue v;
            using (l = new Lua(state))
            {
                v = new LuaTable(l, 123, true);
                Assert.Same(l, v.Lua);
                Assert.Equal(123, v.Reference);
                v.Dispose();
            }
        }

        [Fact]
        public void TestAccesByField()
        {
            var state = new LuaState();
            Lua l;
            ILuaTable tbl;
            using (l = new Lua(state))
            {
                l.State.DoString(@"
table = { field1 = 1234, ['field 2'] = 'Test' }
");
                l.State.LuaGetGlobal("table");
                using (tbl = l.ToTable(-1))
                {
                    Assert.Equal(1234d, tbl["field1"]);
                    Assert.Equal(null, tbl["field2"]);
                    Assert.Equal("Test", tbl["field 2"]);

                    tbl["field1"] = null;
                    tbl["field2"] = 4321;
                }
                Assert.Equal(1, l.State.LuaGetTop());
                l.State.DoString(@"return table['field1'], table.field2, table['field 2']");
                Assert.Equal(4, l.State.LuaGetTop());
                Assert.Equal(null, l.ToValue(-3));
                Assert.Equal(4321d, l.ToValue(-2));
                Assert.Equal("Test", l.ToValue(-1));
            }
        }

        [Fact]
        public void TestAccesByInteger()
        {
            var state = new LuaState();
            Lua l;
            ILuaTable tbl;
            using (l = new Lua(state))
            {
                l.State.DoString(@"
table = { 1234, [3] = 'Test' }
");
                l.State.LuaGetGlobal("table");
                using (tbl = l.ToTable(-1))
                {
                    Assert.Equal(1234d, tbl[1]);
                    Assert.Equal(null, tbl[2]);
                    Assert.Equal("Test", tbl[3]);

                    tbl[1] = null;
                    tbl[2] = 4321;
                }
                Assert.Equal(1, l.State.LuaGetTop());
                l.State.DoString(@"return table[1], table[2], table[3]");
                Assert.Equal(4, l.State.LuaGetTop());
                Assert.Equal(null, l.ToValue(-3));
                Assert.Equal(4321d, l.ToValue(-2));
                Assert.Equal("Test", l.ToValue(-1));
            }
        }

        [Fact]
        public void TestAccesByObject()
        {
            var state = new LuaState();
            Lua l;
            ILuaTable tbl;
            using (l = new Lua(state))
            {
                l.State.LuaPushLightUserData(this);
                l.State.LuaSetGlobal("idx2");
                l.State.DoString(@"
idx1 = 'field1'
idx3 = 123.45
table = { [idx1] = 1234, [idx3] = 'Test' }
");
                l.State.LuaGetGlobal("table");
                using (tbl = l.ToTable(-1))
                {
                    Assert.Equal(1234d, tbl["field1"]);
                    Assert.Equal(null, tbl[this]);
                    Assert.Equal("Test", tbl[123.45]);

                    tbl["field1"] = null;
                    tbl[this] = 4321;
                }
                Assert.Equal(1, l.State.LuaGetTop());
                l.State.DoString(@"return table[idx1], table[idx2], table[idx3]");
                Assert.Equal(4, l.State.LuaGetTop());
                Assert.Equal(null, l.ToValue(-3));
                Assert.Equal(4321d, l.ToValue(-2));
                Assert.Equal("Test", l.ToValue(-1));
            }
        }

    }
}
