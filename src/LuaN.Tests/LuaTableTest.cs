using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{

    public class LuaTableTest
    {

        [Fact]
        public void TestCreate()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
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
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaTable tbl;
            using (l = new Lua(state))
            {
                tbl = new LuaTable(l, 123, true);
                tbl["field1"] = 1234;
                
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaSetField(It.IsAny<int>(), "field1"), Times.Once());

                Assert.Equal(null, tbl["field2"]);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(2));
                mState.Verify(s => s.LuaGetField(It.IsAny<int>(), "field2"), Times.Once());

                tbl.Dispose();
            }
        }

        [Fact]
        public void TestAccesByInteger()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaTable tbl;
            using (l = new Lua(state))
            {
                tbl = new LuaTable(l, 123, true);
                tbl[777] = 1234;

                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaSetI(It.IsAny<int>(), 777), Times.Once());

                Assert.Equal(null, tbl[888]);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(2));
                mState.Verify(s => s.LuaGetI(It.IsAny<int>(), 888), Times.Once());

                tbl.Dispose();
            }
        }

        [Fact]
        public void TestAccesByObject()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;
            Lua l;
            LuaTable tbl;
            using (l = new Lua(state))
            {
                tbl = new LuaTable(l, 123, true);
                tbl[777.77] = 1234;

                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushNumber(777.77), Times.Once());
                //mState.Verify(s => s.LuaSetTable(It.IsAny<int>()), Times.Once());

                Assert.Equal(null, tbl[888.88]);
                mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 123), Times.Exactly(2));
                mState.Verify(s => s.LuaPushNumber(888.88), Times.Once());
                //mState.Verify(s => s.LuaGetTable(It.IsAny<int>()), Times.Once());

                tbl.Dispose();
            }
        }

    }

}
