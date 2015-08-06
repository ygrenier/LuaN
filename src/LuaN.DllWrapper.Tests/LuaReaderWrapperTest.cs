using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaReaderWrapperTest
    {

        [Fact]
        public void TestFromReader()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaReader reader = (state, ud) =>
                {
                    Assert.Same(state, L);
                    Assert.Same(this, ud);
                    return Enumerable.Range(0, 24).Select(i => (byte)i).ToArray();
                };
                var wrapper = new LuaReaderWrapper(reader);
                Assert.Same(reader, wrapper.Reader);

                uint sz = 0;
                var res = wrapper.NativeReader(L.NativeState, L.GetUserDataPtr(this), ref sz);
                Assert.Equal(24u, sz);
                byte[] resb = new byte[sz];
                Marshal.Copy(res, resb, 0, (int)sz);
                for (int i = 0; i < 24; i++)
                    Assert.Equal((byte)i, resb[i]);

                Assert.Equal(IntPtr.Zero, wrapper.NativeReader(IntPtr.Zero, IntPtr.Zero, ref sz));

                Assert.Throws<ArgumentNullException>(() => new LuaReaderWrapper(null));

                wrapper.Dispose();
                wrapper.Dispose();
            }
        }

    }
}
