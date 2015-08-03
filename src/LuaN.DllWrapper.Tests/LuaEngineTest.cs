using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaEngineTest
    {

        [Fact]
        public void TestCreate()
        {
            var engine = new LuaEngine();
            Assert.Equal("Lua 5.3.0  Copyright (C) 1994-2015 Lua.org, PUC-Rio", engine.LuaCopyright);
            Assert.Equal("lua53.dll", engine.EngineName);
            Assert.Equal(-1001000, engine.FirstPseudoIndex);
            Assert.Equal("R. Ierusalimschy, L. H. de Figueiredo, W. Celes", engine.LuaAuthors);
            Assert.Equal("Lua 5.3.0", engine.LuaRelease);
            Assert.Equal("Lua 5.3", engine.LuaVersion);
            Assert.Equal("5", engine.LuaVersionMajor);
            Assert.Equal("3", engine.LuaVersionMinor);
            Assert.Equal(503, engine.LuaVersionNum);
            Assert.Equal("0", engine.LuaVersionRelease);
            Assert.Equal(-1, engine.MultiReturns);
            Assert.Equal(-1001000, engine.RegistryIndex);
            Assert.Equal(20, engine.MinStack);

            Assert.NotNull(LuaEngine.Current);
            Assert.NotSame(engine, LuaEngine.Current);
        }

        [Fact]
        public void TestNewState()
        {
            var engine = LuaEngine.Current;
            using (var L = engine.NewState())
            {
                Assert.Same(engine, L.Engine);
            }
        }

    }
}
