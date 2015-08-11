using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaValueTest
    {
        [Fact]
        public void TestDispose()
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

                // Create a value for testing finalize
                v = new LuaTable(l, 555);
            }
            // Force the finalisation of the last value
            GC.Collect();
        }

        [Fact]
        public void TestPush()
        {
            var state = new LuaState();
            Lua l;
            LuaValue v;
            using (l = new Lua(state))
            {
                l.State.LuaNewTable();
                using (v = (LuaValue)l.ToTable(-1))
                {
                    v.Push(l.State);
                    Assert.True(l.State.LuaCompare(1, 2, LuaRelOperator.EQ));
                }
                Assert.Throws<ArgumentException>(() => v.Push(l.State));
                Assert.Throws<ArgumentNullException>(() => v.Push(null));
            }
        }
    }
}
