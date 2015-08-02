using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{
    /// <summary>
    /// Reference to a .Net user data
    /// </summary>
    static class UserDataRef
    {
        static List<Object> _DataRefs = new List<object>();

        /// <summary>
        /// Find a ref
        /// </summary>
        public static IntPtr FindRef(Object data)
        {
            if (data == null) return IntPtr.Zero;
            var idx = _DataRefs.IndexOf(data);
            if (idx < 0) return IntPtr.Zero;
            return new IntPtr(idx + 1);
        }

        /// <summary>
        /// Get a ref
        /// </summary>
        public static IntPtr GetRef(Object data)
        {
            IntPtr res = FindRef(data);
            if (res == IntPtr.Zero && data!= null)
            {
                _DataRefs.Add(data);
                res = new IntPtr(_DataRefs.Count);
            }
            return res;
        }

        /// <summary>
        /// Get the data from a ref
        /// </summary>
        public static Object GetData(IntPtr dref)
        {
            int idx = dref.ToInt32() - 1;
            return idx >= 0 && idx < _DataRefs.Count ? _DataRefs[idx] : null;
        }

    }
}
