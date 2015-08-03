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
        class UserDataIndex
        {
            int _NextRef = 1;
            IDictionary<IntPtr, UserDataRef> _DataRefs = new Dictionary<IntPtr, UserDataRef>();
            IDictionary<SByte, IList<UserDataRef>> _DataIndex = new SortedDictionary<sbyte, IList<UserDataRef>>();
            UserDataRef FindData(Object data, out IList<UserDataRef> lst, out SByte hash)
            {
                hash = (SByte)(data.GetHashCode() >> 24);
                if (!_DataIndex.TryGetValue(hash, out lst))
                    return null;
                return lst.FirstOrDefault(u => u.Data == data);
            }
            public UserDataRef FindData(Object data)
            {
                SByte hash;
                IList<UserDataRef> lst;
                return FindData(data, out lst, out hash);
            }
            public UserDataRef FindRef(IntPtr udRef)
            {
                UserDataRef result;
                if (_DataRefs.TryGetValue(udRef, out result))
                    return result;
                return null;
            }
            public UserDataRef Add(Object data)
            {
                SByte hash;
                IList<UserDataRef> lst;
                var result = FindData(data, out lst, out hash);
                if (result == null)
                {
                    result = new UserDataRef()
                    {
                        Data = data,
                        Ref = new IntPtr(_NextRef++),
                        RefCount = 0
                    };
                    _DataRefs[result.Ref] = result;
                    if (lst == null)
                    {
                        lst = new List<UserDataRef>();
                        _DataIndex[hash] = lst;
                    }
                    lst.Add(result);
                }
                return result;
            }
            public bool Remove(IntPtr udRef)
            {
                var ud = FindRef(udRef);
                if (ud == null) return false;
                var hash = (SByte)(ud.Data.GetHashCode() >> 24);
                IList<UserDataRef> lst;
                if (_DataIndex.TryGetValue(hash, out lst))
                {
                    lst.Remove(ud);
                    if (lst.Count == 0)
                        _DataIndex.Remove(hash);
                }
                return true;
            }
        }

        static UserDataIndex _Index = new UserDataIndex();

        /// <summary>
        /// Get a ref
        /// </summary>
        public static UserDataRef GetUserData(Object data)
        {
            if (data == null) return null;
            var result = _Index.FindData(data);
            if(result== null)
                result = _Index.Add(data);
            return result;
        }

        /// <summary>
        /// Get the user data from a ref
        /// </summary>
        public static UserDataRef GetUserDataFromRef(IntPtr dref)
        {
            return _Index.FindRef(dref);
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
                _Index.Remove(this.Ref);
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
