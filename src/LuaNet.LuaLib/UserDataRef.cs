using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{
    /// <summary>
    /// Reference to a .Net user data
    /// </summary>
    class UserDataRef
    {
        static int _NextRef = 1;
        static Dictionary<IntPtr, UserDataRef> _DataRefs = new Dictionary<IntPtr, UserDataRef>();

        /// <summary>
        /// Get a ref
        /// </summary>
        public static UserDataRef GetUserData(Object data)
        {
            if (data == null) return null;
            UserDataRef result = _DataRefs.Values.FirstOrDefault(u => u.Data == data);
            if (result == null)
            {
                result = new UserDataRef
                {
                    Data = data,
                    RefCount = 0,
                    Ref = new IntPtr(_NextRef++)
                };
                _DataRefs[result.Ref] = result;
            }
            return result;
        }

        /// <summary>
        /// Get the user data from a ref
        /// </summary>
        public static UserDataRef GetUserDataFromRef(IntPtr dref)
        {
            UserDataRef result = null;
            if (_DataRefs.TryGetValue(dref, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Increment the reference count
        /// </summary>
        public void IncRef()
        {
            RefCount++;
        }

        /// <summary>
        /// Decrement the reference count
        /// </summary>
        public void DecRef()
        {
            RefCount--;
            if (RefCount <= 0)
                _DataRefs.Remove(this.Ref);
        }

        /// <summary>
        /// User Data
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Reference
        /// </summary>
        public IntPtr Ref { get; private set; }

        /// <summary>
        /// Count of references
        /// </summary>
        public int RefCount { get; private set; }

    }
}
