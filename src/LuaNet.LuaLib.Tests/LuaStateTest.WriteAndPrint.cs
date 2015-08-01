using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaNet.LuaLib.Tests
{
    public partial class LuaStateTest
    {
        [Fact]
        public void TestPrint()
        {
            using (var L = new LuaState())
            {
                L.OpenLibs();

                L.GetGlobal("tostring");
                L.GetGlobal("_G");
                L.Call(1, 1);
                var gRef = L.ToString(-1);
                L.Pop(1);

                List<String> output = new List<string>();
                L.OnPrint += (s, e) => output.Add(String.Format("Print: {0}", e.Text));
                L.OnWriteLine += (s, e) => output.Add(String.Format("WriteLine: {0}", e.Text));
                L.OnWriteString += (s, e) => output.Add(String.Format("Write: {0}", e.Text));
                L.OnWriteStringError += (s, e) => output.Add(String.Format("Error: {0}", e.Text));
                L.GetGlobal("print");
                L.PushString("Content line");
                L.PushInteger(1234);
                L.GetGlobal("_G");
                L.PCall(3, 0, 0);

                Assert.Equal(new String[]
                {
                    "Print: Content line\t1234\t"+gRef,
                    "Write: Content line\t1234\t"+gRef,
                    "WriteLine: \r\n"
                }, output);
            }
        }
    }
}
