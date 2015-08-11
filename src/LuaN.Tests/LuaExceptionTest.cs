using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{
    public class LuaExceptionTest
    {
        [Fact]
        public void TestCreate()
        {
            var ex = new LuaException();
            Assert.Equal("Une exception de type 'LuaN.LuaException' a été levée.", ex.Message);

            ex = new LuaException("message");
            Assert.Equal("message", ex.Message);
        }
    }
}
