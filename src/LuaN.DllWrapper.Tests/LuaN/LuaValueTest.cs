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
            LuaValue v;
            using (var state = new LuaState())
            {
                state.LuaNewTable();
                var lref = state.LuaRef();
                Assert.Equal(LuaType.Table, state.LuaPushRef(lref));
                state.LuaPop(1);
                v = new LuaTable(state, lref, true);
                Assert.Equal(lref, v.Reference);
                Assert.Same(state, v.State);
                v.Dispose();
                Assert.Null(v.State);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                Assert.Equal(LuaType.Nil, state.LuaPushRef(lref));
                state.LuaPop(1);

                state.LuaNewTable();
                lref = state.LuaRef();
                Assert.Equal(LuaType.Table, state.LuaPushRef(lref));
                state.LuaPop(1);
                v = new LuaTable(state, lref, false);
                Assert.Equal(lref, v.Reference);
                Assert.Same(state, v.State);
                v.Dispose();
                Assert.Null(v.State);
                Assert.Equal(LuaRef.NoRef, v.Reference);
                Assert.Equal(LuaType.Table, state.LuaPushRef(lref));

                // Create a value for testing finalize
                v = new LuaTable(state, 555);
            }
            // Force the finalisation of the last value
            GC.Collect();
        }

        [Fact]
        public void TestPush()
        {
            LuaValue v;
            using (var state = new LuaState())
            {
                state.LuaNewTable();
                using (v = (LuaValue)state.ToTable(-1))
                {
                    v.Push(state);
                    Assert.True(state.LuaCompare(1, 2, LuaRelOperator.EQ));
                }
                Assert.Throws<ArgumentException>(() => v.Push(state));
                Assert.Throws<ArgumentNullException>(() => v.Push(null));
            }
        }
    }
}
