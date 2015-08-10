using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Lua state host
    /// </summary>
    public class Lua : IDisposable
    {
        bool _StateOwned;

        #region Ctors & Dest. , Dispose

        /// <summary>
        /// Create a Lua host from an engine
        /// </summary>
        public Lua(ILuaEngine engine)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            // Create the state
            _State = engine.NewState();
            _StateOwned = true;
            InitState();
        }

        /// <summary>
        /// Create a Lua host from a existing state
        /// </summary>
        /// <remarks>
        /// Multiple Lua host can't share the same Lua state.
        /// </remarks>
        /// <exception cref="ArgumentNullException">When <paramref name="state"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When a lua state is already hosted.</exception>
        public Lua(ILuaState state, bool ownState = true)
        {
            if (state == null) throw new ArgumentNullException("state");
            // Check if the state is already hosted
            state.LuaPushString("LUAN HOSTED");
            state.LuaGetTable(state.RegistryIndex);
            if (state.LuaToBoolean(-1))
            {
                state.LuaSetTop(-2);
                throw new InvalidOperationException("This state is already hosted.");
            }
            else
            {
                state.LuaSetTop(-2);
                _State = state;
                _StateOwned = ownState;
                InitState();
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Lua()
        {
            Dispose(false);
        }

        /// <summary>
        /// Internal release resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_State != null)
            {
                if (_StateOwned)
                    _State.Dispose();
                _State = null;
            }
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.WaitForPendingFinalizers();
        }

        #endregion

        #region State management

        /// <summary>
        /// Initialize the state
        /// </summary>
        void InitState()
        {
            // Register state as hosted
            State.LuaPushString("LUAN HOSTED");
            State.LuaPushBoolean(true);
            State.LuaSetTable(State.RegistryIndex);
        }

        #endregion

        #region .Net objects management

        /// <summary>
        /// Release the resource of a LuaValue
        /// </summary>
        internal protected virtual void DisposeLuaValue(LuaValue value)
        {
            if (value != null && value.Lua == this)
            {
                if (value.Reference != LuaRef.RefNil && value.Reference != LuaRef.NoRef)
                {
                    State.LuaUnref(value.Reference);
                }
            }
        }

        /// <summary>
        /// Convert a Lua value to a .Net table
        /// </summary>
        public virtual ILuaTable ToTable(int idx)
        {
            // If the value is not a table return null
            if (State.LuaType(idx) != LuaType.Table)
                return null;
            // Create the reference
            State.LuaPushValue(idx);
            var vref= State.LuaRef();
            State.LuaPop(1);
            if (vref == LuaRef.RefNil || vref == LuaRef.NoRef)
                throw new InvalidOperationException("Can't create a reference for this value.");
            return new LuaTable(this, vref);
        }

        /// <summary>
        /// Convert a Lua value to the corresponding .Net object
        /// </summary>
        public virtual Object ToValue(int idx)
        {
            var tp = State.LuaType(idx);
            switch (tp)
            {
                case LuaType.Boolean:
                    return State.LuaToBoolean(idx);
                case LuaType.Number:
                    return State.LuaToNumber(idx);
                case LuaType.String:
                    return State.LuaToString(idx);
                case LuaType.LightUserData:
                case LuaType.UserData:
                    return State.LuaToUserData(idx);
                case LuaType.Table:
                    return ToTable(idx);
                case LuaType.Function:
                    throw new NotImplementedException();
                case LuaType.Thread:
                    return State.LuaToThread(idx);
                case LuaType.None:
                case LuaType.Nil:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Push a .Net object to the corresponding Lua value 
        /// </summary>
        public virtual void Push(object value)
        {
            if (value == null)
                State.LuaPushNil();
            else if (value is Boolean)
                State.LuaPushBoolean((Boolean)value);
            else if (value is Single || value is Double || value is Decimal)
                State.LuaPushNumber(Convert.ToDouble(value));
            else if (value is SByte || value is Byte || value is Int16 || value is UInt16 || value is Int32 || value is UInt16 || value is Int64 || value is UInt64)
                State.LuaPushInteger(Convert.ToInt64(value));
            else if (value is Char || value is String)
                State.LuaPushString(value.ToString());
            else if (value is ILuaUserData)
                throw new InvalidOperationException("Can't push a userdata");
            else if (value is LuaCFunction)
                State.LuaPushCFunction((LuaCFunction)value);
            else if (value is ILuaState)
                if (value == State)
                    State.LuaPushThread();
                else
                    throw new InvalidOperationException("Can't push a different thread");
            else if (value is ILuaValue)
                ((ILuaValue)value).Push(State);
            //else if (value is LuaValue)
            //{
            //    var lv = (LuaValue)value;
            //    if (lv.Lua != this) throw new InvalidOperationException("This value is not hosted by this host.");
            //    lv.Push();
            //}
            else
                State.LuaPushLightUserData(value);
            //case LuaType.LightUserData:
            //case LuaType.UserData:
            //    return L.LuaToUserData(idx);
        }

        #endregion

        /// <summary>
        /// Current state
        /// </summary>
        public ILuaState State
        {
            get
            {
                if (_State == null)
                    throw new ObjectDisposedException("Lua");
                return _State;
            }
        }
        private ILuaState _State;

    }
}
