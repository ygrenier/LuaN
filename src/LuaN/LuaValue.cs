using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Base of the Lua value wrappers
    /// </summary>
    public abstract class LuaValue : ILuaValue, IDisposable
    {

        /// <summary>
        /// Destructor
        /// </summary>
        ~LuaValue()
        {
            Dispose(false);
        }

        /// <summary>
        /// Internal release resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Lua != null)
            {
                if (ReferenceOwned && disposing)
                    Lua.DisposeLuaValue(this);
                Lua = null;
                Reference = LuaRef.NoRef;
            }
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Push the value
        /// </summary>
        internal protected virtual void Push()
        {
            if (Lua != null)
                Lua.State.LuaPushRef(Reference);
        }

        /// <summary>
        /// Push the value
        /// </summary>
        public void Push(ILuaState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (Lua == null || Lua.State != state)
                throw new ArgumentException("This value is not hosted by this state.", "state");
            Push();
        }

        /// <summary>
        /// Indicate if this instance own the reference
        /// </summary>
        protected bool ReferenceOwned { get; set; }

        /// <summary>
        /// Lua host
        /// </summary>
        public Lua Lua { get; protected set; }

        /// <summary>
        /// Reference
        /// </summary>
        public int Reference { get; protected set; }

    }
}
