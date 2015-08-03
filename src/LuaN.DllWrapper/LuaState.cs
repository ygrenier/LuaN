using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.DllWrapper
{
    /// <summary>
    /// Lua state
    /// </summary>
    public class LuaState : ILuaState
    {
        static Dictionary<IntPtr, LuaState> _RegisteredStates = new Dictionary<IntPtr, LuaState>();
        IntPtr _NativeState;
        bool _OwnNativeState = true;
        LuaState _MainState;

        #region Ctor & Dest

        /// <summary>
        /// Create a new state
        /// </summary>
        public LuaState() : this(null, IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        internal LuaState(ILuaEngine engine, IntPtr nativeState, bool ownState)
        {
            this.Engine = engine ?? LuaEngine.Current;
            // Create a state ?
            if (nativeState == IntPtr.Zero)
            {
                nativeState = LuaDll.luaL_newstate();
                if (nativeState == IntPtr.Zero)
                    throw new OutOfMemoryException("Cannot create state: not enough memory");
            }
            InitState(nativeState, ownState);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~LuaState()
        {
            Dispose(false);
        }

        /// <summary>
        /// Internal release resources
        /// </summary>
        protected void Dispose(bool disposing)
        {
            ReleaseState();
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region State management

        /// <summary>
        /// Initialize the state
        /// </summary>
        private void InitState(IntPtr nativeState, bool ownState)
        {
            _NativeState = nativeState;
            lock (_RegisteredStates)
            _RegisteredStates[_NativeState] = this;
            _OwnNativeState = ownState;
            //SetDefaultAtPanic();
            _MainState = null;
            if (!ownState)
            {
                // Retreive the main thread
                LuaDll.lua_geti(_NativeState, LuaDll.LUA_REGISTRYINDEX, LuaDll.LUA_RIDX_MAINTHREAD);
                var mt = LuaDll.lua_tothread(_NativeState, -1);
                LuaDll.lua_pop(_NativeState, 1);
                _MainState = FindState(mt, false);
            }
        }

        /// <summary>
        /// Release the state
        /// </summary>
        private void ReleaseState()
        {
            if (_NativeState != IntPtr.Zero)
            {
                //RestoreOriginalAtPanic();
                if (_OwnNativeState)
                {
                    // Unregister all 'child' states
                    List<LuaState> childs = null;
                    lock (_RegisteredStates)
                        childs = _RegisteredStates.Values.Where(c => c._MainState == this).ToList();
                    foreach (var child in childs)
                        child.Dispose();
                    LuaDll.lua_close(_NativeState);
                }
                lock (_RegisteredStates)
                    _RegisteredStates.Remove(_NativeState);
                _NativeState = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Find a registered state
        /// </summary>
        internal static LuaState FindState(IntPtr state, bool creatIfNotExists)
        {
            LuaState result;
            if (!_RegisteredStates.TryGetValue(state, out result))
            {
                if (creatIfNotExists && state != IntPtr.Zero)
                {
                    result = new LuaState(null, state, false);
                }
                else
                    return null;
            }
            return result;
        }

        #endregion

        #region Lua engine informations

        /// <summary>
        /// Engine provider
        /// </summary>
        public ILuaEngine Engine { get; private set; }

        /// <summary>
        /// Option for multiple returns in 'PCall' and 'Call' 
        /// </summary>
        public int MultiReturns { get { return LuaDll.LUA_MULTRET; } }

        /// <summary>
        /// First pseudo index
        /// </summary>
        public int FirstPseudoIndex { get { return LuaDll.LUAI_FIRSTPSEUDOIDX; } }

        /// <summary>
        /// Index of the registry
        /// </summary>
        public int RegistryIndex { get { return LuaDll.LUA_REGISTRYINDEX; } }

        /// <summary>
        /// Minumum stack size
        /// </summary>
        public int MinStack { get { return LuaDll.LUA_MINSTACK; } }

        #endregion

        #region State management

        /// <summary>
        /// Get the version number used to create this state
        /// </summary>
        public Double LuaVersion()
        {
            return LuaDll.lua_version(NativeState);
        }

        ///// <summary>
        ///// Define the panic function
        ///// </summary>
        //LuaFunction AtPanic(LuaFunction panicf);

        /// <summary>
        /// Creates a new thread, pushes it on the stack.
        /// </summary>
        public ILuaState LuaNewThread()
        {
            var thread = LuaDll.lua_newthread(NativeState);
            LuaDll.lua_pop(NativeState, 1);
            return new LuaState(this.Engine, thread, false);
        }

        #endregion

        /// <summary>
        /// Access to the native state
        /// </summary>
        public IntPtr NativeState
        {
            get
            {
                if (_NativeState == IntPtr.Zero)
                    throw new ObjectDisposedException("LuaState");
                return _NativeState;
            }
        }

    }
}
