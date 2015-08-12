using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Default ILuaTable implementation
    /// </summary>
    public class LuaTable : LuaValue, ILuaTable
    {
        /// <summary>
        /// Create a new table reference
        /// </summary>
        public LuaTable(ILuaState state, int reference, bool ownRef = true)
        {
            this.State = state;
            this.Reference = reference;
            this.ReferenceOwned = ownRef;
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
        /// Access to integer fields
        /// </summary>
        public object this[int index]
        {
            get { return GetFieldValue(index); }
            set { SetFieldValue(index, value); }
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
