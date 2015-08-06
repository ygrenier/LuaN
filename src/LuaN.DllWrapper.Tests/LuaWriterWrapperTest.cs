using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaWriterWrapperTest
    {

        [Fact]
        public void TestFromWriter()
        {
            LuaState L;
            using (L = new LuaState())
            {
                LuaWriter writer = (state, p, ud) =>
                {
                    Assert.Same(state, L);
                    Assert.Same(this, ud);
                    Assert.Equal(24, p.Length);
                    for (int i = 0; i < 24; i++)
                        Assert.Equal((byte)i, p[i]);
                    return 0;
                };
                var wrapper = new LuaWriterWrapper(writer);
                Assert.Same(writer, wrapper.Writer);

                var buffer= Enumerable.Range(0, 24).Select(i => (byte)i).ToArray();
                var ptr = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, ptr, buffer.Length);
                Assert.Equal(0, wrapper.NativeWriter(L.NativeState, ptr, (uint)buffer.Length, L.GetUserDataPtr(this)));

                Assert.Equal(0, wrapper.NativeWriter(IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero));

                Assert.Throws<ArgumentNullException>(() => new LuaWriterWrapper(null));
            }
        }

    }
}
