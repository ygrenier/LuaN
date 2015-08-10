using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Represents a native Lua user data
    /// </summary>
    public interface ILuaNativeUserData
    {
        /// <summary>
        /// Read the raw data
        /// </summary>
        /// <returns></returns>
        Byte[] GetRawData();

        /// <summary>
        /// Write the raw data
        /// </summary>
        /// <param name="data"></param>
        void SetRawData(Byte[] data);

        /// <summary>
        /// Size of the data
        /// </summary>
        UInt32 Size { get; }
    }
}
