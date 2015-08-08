using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaN.DllWrapper
{

    /// <summary>
    /// Wrapp the Dll user data
    /// </summary>
    public class LuaUserData : ILuaUserData
    {

        /// <summary>
        /// Create a new Lua user data
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="size"></param>
        public LuaUserData(IntPtr pointer, UInt32 size)
        {
            this.Pointer = pointer;
            this.Size = size;
        }

        /// <summary>
        /// Read the raw data
        /// </summary>
        public byte[] GetRawData()
        {
            Byte[] result = null;
            if(Pointer!=IntPtr.Zero && Size > 0)
            {
                result = new Byte[Size];
                Marshal.Copy(Pointer, result, 0, (int)Size);
            }
            return result;
        }

        /// <summary>
        /// Write the raw data
        /// </summary>
        public void SetRawData(byte[] data)
        {
            if(data!= null && Pointer != IntPtr.Zero && Size > 0)
            {
                Marshal.Copy(data, 0, Pointer, Math.Min(data.Length, (int)Size));
            }
        }

        /// <summary>
        /// Pointer
        /// </summary>
        public IntPtr Pointer { get; private set; }

        /// <summary>
        /// Size
        /// </summary>
        public uint Size { get; private set; }

    }

}
