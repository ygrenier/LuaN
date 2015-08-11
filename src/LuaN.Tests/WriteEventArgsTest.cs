using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{
    public class WriteEventArgsTest
    {
        [Fact]
        public void TestCreate()
        {
            var evnt = new WriteEventArgs("test");
            Assert.Equal("test", evnt.Text);
            Assert.Equal(false, evnt.Handled);
        }
    }
}
