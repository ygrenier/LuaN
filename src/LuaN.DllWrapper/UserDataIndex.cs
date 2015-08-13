using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.DllWrapper
{
    /// <summary>
    /// Index to manage .Net object with pseudo-pointer for Lua
    /// </summary>
    public class UserDataIndex
    {
        /// <summary>
        /// Reference to an object
        /// </summary>
        public class UserDataRef
        {
            /// <summary>
            /// Objet of the user data
            /// </summary>
            public Object Data { get; internal set; }
            /// <summary>
            /// Pseudo-pointer
            /// </summary>
            public IntPtr Pointer { get; internal set; }
        }

        int _NextRef = -1;
        IDictionary<IntPtr, UserDataRef> _DataRefs = new Dictionary<IntPtr, UserDataRef>();
        IDictionary<SByte, IList<UserDataRef>> _DataIndex = new SortedDictionary<sbyte, IList<UserDataRef>>();

        /// <summary>
        /// Reset the entire index
        /// </summary>
        public void Reset()
        {
            _DataIndex.Clear();
            _DataRefs.Clear();
            _NextRef = -1;
        }

        UserDataRef FindData(Object data, out IList<UserDataRef> lst, out SByte hash)
        {
            if (data == null)
            {
                lst = null;
                hash = 0;
                return null;
            }
            hash = (SByte)(data.GetHashCode() >> 24);
            if (!_DataIndex.TryGetValue(hash, out lst))
                return null;
            return lst.FirstOrDefault(u => Object.Equals(u.Data, data));
        }

        /// <summary>
        /// Find the reference for an userdata
        /// </summary>
        public UserDataRef FindData(Object data)
        {
            SByte hash;
            IList<UserDataRef> lst;
            return FindData(data, out lst, out hash);
        }

        /// <summary>
        /// Find the reference for a pointer
        /// </summary>
        public UserDataRef FindPointer(IntPtr ptr)
        {
            UserDataRef result;
            if (_DataRefs.TryGetValue(ptr, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Add an user data in the index and calculate his pseudo-pointer
        /// </summary>
        public UserDataRef Add(Object data)
        {
            SByte hash;
            IList<UserDataRef> lst;
            var result = FindData(data, out lst, out hash);
            if (result == null && data != null)
            {
                if (data is LuaNativeUserData)
                {
                    result = new UserDataRef()
                    {
                        Data = data,
                        Pointer = ((LuaNativeUserData)data).Pointer
                    };
                }
                else
                {
                    result = new UserDataRef()
                    {
                        Data = data,
                        Pointer = new IntPtr(_NextRef--)
                    };
                }
                _DataRefs[result.Pointer] = result;
                if (lst == null)
                {
                    lst = new List<UserDataRef>();
                    _DataIndex[hash] = lst;
                }
                lst.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Remove an user data from is pseudo-pointer
        /// </summary>
        public bool Remove(IntPtr udRef)
        {
            var ud = FindPointer(udRef);
            if (ud == null) return false;
            _DataRefs.Remove(ud.Pointer);
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

        /// <summary>
        /// Get the list of the userdata
        /// </summary>
        public IEnumerable<UserDataRef> GetUserDatas()
        {
            return _DataRefs.Values;
        }

        /// <summary>
        /// Count of userdata registered 
        /// </summary>
        public int Count { get { return _DataRefs.Count; } }

    }

}
