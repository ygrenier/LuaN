using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Default ILuaUserData implementation
    /// </summary>
    public class LuaUserData : LuaValue, ILuaUserData
    {

        /// <summary>
        /// Create a new userdata reference
        /// </summary>
        public LuaUserData(ILuaState state, int reference, bool ownRef = true)
        {
            this.State = state;
            this.Reference = reference;
            this.ReferenceOwned = ownRef;
        }

        /// <summary>
        /// Call this userdata
        /// </summary>
        public object[] Call(params object[] args)
        {
            return State.CallValue(Reference, args);
        }

        /// <summary>
        /// Call this userdata
        /// </summary>
        public object[] Call(object[] args, Type[] typedResult)
        {
            return State.CallValue(Reference, args, typedResult);
        }

        /// <summary>
        /// Access to the named fields
        /// </summary>
        public object this[String field]
        {
            get { return GetFieldValue(field); }
            set { SetFieldValue(field, value); }
        }

        /// <summary>
        /// Access to the fields
        /// </summary>
        public object this[object index]
        {
            get { return GetFieldValue(index); }
            set { SetFieldValue(index, value); }
        }

    }

}
