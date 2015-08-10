using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Base of the Lua value wrappers
    /// </summary>
    public abstract class LuaValue : IDisposable
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
