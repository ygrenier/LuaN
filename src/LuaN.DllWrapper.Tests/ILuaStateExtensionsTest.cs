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
    }
}
