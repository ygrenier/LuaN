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
        /// Dispose this value
        /// </summary>
        protected void DisposeValue()
        {
            if (State != null)
            {
                if (Reference != LuaRef.RefNil && Reference != LuaRef.NoRef)
                {
                    State.LuaUnref(Reference);
                    Reference = LuaRef.NoRef;
                }
            }
        }

        /// <summary>
        /// Internal release resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (State != null)
            {
                if (ReferenceOwned && disposing)
                    DisposeValue();
                State = null;
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
        /// Get the value of the field of a referenced value
        /// </summary>
        protected virtual Object GetFieldValue(String field)
        {
            var oldTop = State.LuaGetTop();
            try
            {
                State.LuaPushRef(Reference);
                State.LuaGetField(-1, field);
                return State.ToValue(-1);
            }
            finally
            {
                State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Set the value of the field of a referenced value
        /// </summary>
        protected virtual void SetFieldValue(String field, object value)
        {
            var oldTop = State.LuaGetTop();
            try
            {
                State.LuaPushRef(Reference);
                State.Push(value);
                State.LuaSetField(-2, field);
            }
            finally
            {
                State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Get the value of the field of a referenced value
        /// </summary>
        protected virtual Object GetFieldValue(int index)
        {
            var oldTop = State.LuaGetTop();
            try
            {
                State.LuaPushRef(Reference);
                State.LuaGetI(-1, index);
                return State.ToValue(-1);
            }
            finally
            {
                State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Set the value of the field of a referenced value
        /// </summary>
        protected virtual void SetFieldValue(int index, object value)
        {
            var oldTop = State.LuaGetTop();
            try
            {
                State.LuaPushRef(Reference);
                State.Push(value);
                State.LuaSetI(-2, index);
            }
            finally
            {
                State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Get the value of the field of a referenced value
        /// </summary>
        protected virtual Object GetFieldValue(object index)
        {
            var oldTop = State.LuaGetTop();
            try
            {
                State.LuaPushRef(Reference);
                State.Push(index);
                State.LuaGetTable(-2);
                return State.ToValue(-1);
            }
            finally
            {
                State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Set the value of the field of a referenced value
        /// </summary>
        protected virtual void SetFieldValue(object index, object value)
        {
            var oldTop = State.LuaGetTop();
            try
            {
                State.LuaPushRef(Reference);
                State.Push(index);
                State.Push(value);
                State.LuaSetTable(-3);
            }
            finally
            {
                State.LuaSetTop(oldTop);
            }
        }

        /// <summary>
        /// Push the value
        /// </summary>
        internal protected virtual void Push()
        {
            if (State != null)
                State.LuaPushRef(Reference);
        }

        /// <summary>
        /// Push the value
        /// </summary>
        public void Push(ILuaState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (State != state)
                throw new ArgumentException("This value is not hosted by this state.", "state");
            Push();
        }

        /// <summary>
        /// Indicate if this instance own the reference
        /// </summary>
        protected bool ReferenceOwned { get; set; }

        /// <summary>
        /// Lua state
        /// </summary>
        public ILuaState State { get; protected set; }

        /// <summary>
        /// Reference
        /// </summary>
        public int Reference { get; protected set; }

    }
}
