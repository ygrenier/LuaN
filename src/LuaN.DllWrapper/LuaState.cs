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
        List<LuaCFunctionWrapper> _CFunctionWrappers = new List<LuaCFunctionWrapper>();
        List<LuaKFunctionWrapper> _KFunctionWrappers = new List<LuaKFunctionWrapper>();
        List<LuaReaderWrapper> _ReaderWrappers = new List<LuaReaderWrapper>();
        List<LuaWriterWrapper> _WriterWrappers = new List<LuaWriterWrapper>();
        IntPtr _NativeState;
        bool _OwnNativeState = true;
        LuaState _MainState;
        LuaCFunction _OriginalAtPanic = null;

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
            UserDataIndex = new UserDataIndex();
            _NativeState = nativeState;
            lock (_RegisteredStates)
            _RegisteredStates[_NativeState] = this;
            _OwnNativeState = ownState;
            SetDefaultAtPanic();
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
                RestoreOriginalAtPanic();
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
                _CFunctionWrappers.Clear();
                _KFunctionWrappers.Clear();
                foreach (var reader in _ReaderWrappers)
                    reader.Dispose();
                _ReaderWrappers.Clear();
                _WriterWrappers.Clear();
                UserDataIndex.Reset();
                UserDataIndex = null;
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

        /// <summary>
        /// Default AtPanic function, raised an LuaException
        /// </summary>
        static private int DefaultLuaStateAtPanic(ILuaState state)
        {
            if (state != null)
            {
                if (state.LuaGetTop() > 0)
                    throw new LuaException(state.LuaToString(-1));
            }
            throw new LuaException();
        }

        #endregion

        #region Objects binding between Lua and .Net

        /// <summary>
        /// Wrap a native C function
        /// </summary>
        public LuaCFunction WrapFunction(LuaDll.lua_CFunction function)
        {
            if (function == null) return null;
            var wrapper = _CFunctionWrappers.FirstOrDefault(w => w.NativeFunction == function);
            if(wrapper== null)
            {
                wrapper = new LuaCFunctionWrapper(function);
                _CFunctionWrappers.Add(wrapper);
            }
            return wrapper.CFunction;
        }

        /// <summary>
        /// Wrap a .Net C function
        /// </summary>
        public LuaDll.lua_CFunction WrapFunction(LuaCFunction function)
        {
            if (function == null) return null;
            var wrapper = _CFunctionWrappers.FirstOrDefault(w => w.CFunction == function);
            if (wrapper == null)
            {
                wrapper = new LuaCFunctionWrapper(function);
                _CFunctionWrappers.Add(wrapper);
            }
            return wrapper.NativeFunction;
        }

        /// <summary>
        /// Wrap a .Net K function
        /// </summary>
        public LuaDll.lua_KFunction WrapKFunction(LuaKFunction function)
        {
            if (function == null) return null;
            var wrapper = _KFunctionWrappers.FirstOrDefault(w => w.KFunction == function);
            if (wrapper == null)
            {
                wrapper = new LuaKFunctionWrapper(function);
                _KFunctionWrappers.Add(wrapper);
            }
            return wrapper.NativeFunction;
        }

        /// <summary>
        /// Wrap a .Net reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public LuaDll.lua_Reader WrapReader(LuaReader reader)
        {
            if (reader == null) return null;
            var wrapper = _ReaderWrappers.FirstOrDefault(w => w.Reader == reader);
            if(wrapper== null)
            {
                wrapper = new LuaReaderWrapper(reader);
                _ReaderWrappers.Add(wrapper);
            }
            return wrapper.NativeReader;
        }

        /// <summary>
        /// Wrap a .Net writer
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public LuaDll.lua_Writer WrapWriter(LuaWriter writer)
        {
            if (writer == null) return null;
            var wrapper = _WriterWrappers.FirstOrDefault(w => w.Writer == writer);
            if (wrapper == null)
            {
                wrapper = new LuaWriterWrapper(writer);
                _WriterWrappers.Add(wrapper);
            }
            return wrapper.NativeWriter;
        }

        /// <summary>
        /// Get the user data from a pseudo-pointer
        /// </summary>
        public Object GetUserData(IntPtr ptr)
        {
            var rud = UserDataIndex.FindPointer(ptr);
            return rud != null ? rud.Data : null;
        }

        /// <summary>
        /// Get the pseudo-pointer for an object
        /// </summary>
        public IntPtr GetUserDataPtr(Object uData)
        {
            if (uData == null) return IntPtr.Zero;
            var rud = UserDataIndex.FindData(uData);
            if (rud == null)
                rud = UserDataIndex.Add(uData);
            return rud.Pointer;
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
        /// Define the default AtPanic function
        /// </summary>
        public void SetDefaultAtPanic()
        {
            var o = LuaAtPanic(DefaultLuaStateAtPanic);
            if (_OriginalAtPanic == null) _OriginalAtPanic = o;
        }

        /// <summary>
        /// Restore the original at panic
        /// </summary>
        public bool RestoreOriginalAtPanic()
        {
            if (_OriginalAtPanic != null)
            {
                LuaAtPanic(_OriginalAtPanic);
                _OriginalAtPanic = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the version number used to create this state
        /// </summary>
        public Double LuaVersion()
        {
            return LuaDll.lua_version(NativeState);
        }

        /// <summary>
        /// Define the panic function
        /// </summary>
        public LuaCFunction LuaAtPanic(LuaCFunction panicf)
        {
            if (panicf == null) throw new ArgumentNullException("panicf");
            var oldFunc = LuaDll.lua_atpanic(NativeState, WrapFunction(panicf));
            return WrapFunction(oldFunc);
        }

        /// <summary>
        /// Creates a new thread, pushes it on the stack.
        /// </summary>
        public ILuaState LuaNewThread()
        {
            var thread = LuaDll.lua_newthread(NativeState);
            LuaDll.lua_pop(NativeState, 1);
            return new LuaState(this.Engine, thread, false);
        }

        /// <summary>
        /// Get a LuaState from a ILuaState
        /// </summary>
        LuaState GetAsLuaState(ILuaState state, String argName)
        {
            if (state == null) return null;
            LuaState ls = state as LuaState;
            if (ls == null)
                throw new InvalidOperationException(String.Format("The '{0}' state is not a supported state.", argName));
            return ls;
        }

        #endregion

        #region Stack management

        #region basic stack manipulation
        /// <summary>
        /// Get the absolute stack index
        /// </summary>
        public int LuaAbsIndex(int idx)
        {
            return LuaDll.lua_absindex(NativeState, idx);
        }
        /// <summary>
        /// Get the top of the stack
        /// </summary>
        public int LuaGetTop()
        {
            return LuaDll.lua_gettop(NativeState);
        }
        /// <summary>
        /// Set the top of the stack
        /// </summary>
        public void LuaSetTop(int idx)
        {
            LuaDll.lua_settop(NativeState, idx);
        }
        /// <summary>
        /// Push a value on the stack
        /// </summary>
        public void LuaPushValue(int idx)
        {
            LuaDll.lua_pushvalue(NativeState, idx);
        }
        /// <summary>
        /// Rotates the n stack elements between the valid index idx and the top of the stack
        /// </summary>
        public void LuaRotate(int idx, int n)
        {
            LuaDll.lua_rotate(NativeState, idx, n);
        }
        /// <summary>
        /// Copies the element at index fromidx into the valid index toidx, replacing the value at that position
        /// </summary>
        public void LuaCopy(int fromidx, int toidx)
        {
            LuaDll.lua_copy(NativeState, fromidx, toidx);
        }
        /// <summary>
        /// Ensures that the stack has space for at least n extra slots
        /// </summary>
        public bool LuaCheckStack(int n)
        {
            return LuaDll.lua_checkstack(NativeState, n) != 0;
        }
        /// <summary>
        /// Exchange values between different threads of the same state
        /// </summary>
        public void LuaXMove(ILuaState to, int n)
        {
            if (to == null) throw new ArgumentNullException("to");
            LuaState stTo = GetAsLuaState(to, "to");
            LuaDll.lua_xmove(NativeState, stTo.NativeState, n);
        }
        #endregion

        #region access functions (stack -> C)
        /// <summary>
        /// Returns true if the value at the given index is a number or a string convertible to a number, and false otherwise.
        /// </summary>
        public Boolean LuaIsNumber(int idx)
        {
            return LuaDll.lua_isnumber(NativeState, idx) != 0;
        }
        /// <summary>
        /// Returns true if the value at the given index is a string or a number (which is always convertible to a string), and false otherwise.
        /// </summary>
        public Boolean LuaIsString(int idx)
        {
            return LuaDll.lua_isstring(NativeState, idx) != 0;
        }
        /// <summary>
        /// Returns true if the value at the given index is a C function, and false otherwise.
        /// </summary>
        public Boolean LuaIsCFunction(int idx)
        {
            return LuaDll.lua_iscfunction(NativeState, idx) != 0;
        }
        /// <summary>
        /// Returns true if the value at the given index is an integer (that is, the value is a number and is represented as an integer), and false otherwise.
        /// </summary>
        public Boolean LuaIsInteger(int idx)
        {
            return LuaDll.lua_isinteger(NativeState, idx) != 0;
        }
        /// <summary>
        /// Returns true if the value at the given index is a userdata (either full or light), and false otherwise.
        /// </summary>
        public Boolean LuaIsUserData(int idx)
        {
            return LuaDll.lua_isuserdata(NativeState, idx) != 0;
        }
        /// <summary>
        /// Returns the type of the value in the given valid index, or LUA_TNONE for a non-valid (but acceptable) index
        /// </summary>
        public LuaType LuaType(int idx)
        {
            return (LuaType)LuaDll.lua_type(NativeState, idx);
        }
        /// <summary>
        /// Returns the name of the type encoded by the value tp, which must be one the values returned by Type.
        /// </summary>
        public String LuaTypeName(LuaType tp)
        {
            return LuaDll.lua_typename(NativeState, (int)tp);
        }

        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Number
        /// </summary>
        public Double LuaToNumberX(int idx, out bool isnum)
        {
            int nisnum;
            var result = LuaDll.lua_tonumberx(NativeState, idx, out nisnum);
            isnum = nisnum != 0;
            return result;
        }
        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Integer
        /// </summary>
        public Int64 LuaToIntegerX(int idx, out bool isnum)
        {
            int nisnum;
            var result = LuaDll.lua_tointegerx(NativeState, idx, out nisnum);
            isnum = nisnum != 0;
            return result;
        }
        /// <summary>
        /// Converts the Lua value at the given index to a C boolean value
        /// </summary>
        public Boolean LuaToBoolean(int idx)
        {
            return LuaDll.lua_toboolean(NativeState, idx) != 0;
        }
        /// <summary>
        /// Converts the Lua value at the given index to a string.
        /// </summary>
        public String LuaToString(int idx)
        {
            return LuaDll.lua_tostring(NativeState, idx);
        }
        /// <summary>
        /// Returns the raw "length" of the value at the given index
        /// </summary>
        /// <remarks>
        /// Returns the raw "length" of the value at the given index: for strings, this is the string length; 
        /// for tables, this is the result of the length operator ('#') with no metamethods; for userdata, this is the size 
        /// of the block of memory allocated for the userdata; for other values, it is 0.
        /// </remarks>
        public UInt32 LuaRawLen(int idx)
        {
            return LuaDll.lua_rawlen(NativeState, idx);
        }
        /// <summary>
        /// Converts a value at the given index to a C function
        /// </summary>
        public LuaCFunction LuaToCFunction(int idx)
        {
            return WrapFunction(LuaDll.lua_tocfunction(NativeState, idx));
        }
        /// <summary>
        /// If the value at the given index is a full userdata, returns its block address. If the value is a light userdata, returns its pointer. Otherwise, returns NULL.
        /// </summary>
        public Object LuaToUserData(int idx)
        {
            return GetUserData(LuaDll.lua_touserdata(NativeState, idx));
        }
        /// <summary>
        /// Converts the value at the given index to a Lua thread
        /// </summary>
        public ILuaState LuaToThread(int idx)
        {
            var ptr = LuaDll.lua_tothread(NativeState, idx);
            return ptr != IntPtr.Zero ? FindState(ptr, true) : null;
        }
        #endregion

        #region push functions (C -> stack)
        /// <summary>
        /// Push a nil value
        /// </summary>
        public void LuaPushNil()
        {
            LuaDll.lua_pushnil(NativeState);
        }
        /// <summary>
        /// Push a number value
        /// </summary>
        public void LuaPushNumber(Double n)
        {
            LuaDll.lua_pushnumber(NativeState, n);
        }
        /// <summary>
        /// Push a integer value
        /// </summary>
        public void LuaPushInteger(Int64 n)
        {
            LuaDll.lua_pushinteger(NativeState, n);
        }
        /// <summary>
        /// Push a String value
        /// </summary>
        public void LuaPushString(String s)
        {
            LuaDll.lua_pushstring(NativeState, s);
        }
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, String arg0);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, Double arg0);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, int arg0);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, String arg0, String arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, String arg0, Double arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, String arg0, int arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, Double arg0, String arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, Double arg0, Double arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, Double arg0, int arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, int arg0, String arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, int arg0, Double arg1);
        ///// <summary>
        ///// Push a formatted string value
        ///// </summary>
        //String PushFString(String fmt, int arg0, int arg1);
        /// <summary>
        /// Push a C closure
        /// </summary>
        public void LuaPushCClosure(LuaCFunction fn, int n)
        {
            LuaDll.lua_pushcclosure(NativeState, WrapFunction(fn), n);
        }
        /// <summary>
        /// Push a boolean value
        /// </summary>
        public void LuaPushBoolean(Boolean b)
        {
            LuaDll.lua_pushboolean(NativeState, b ? 1 : 0);
        }
        /// <summary>
        /// Push a light user data
        /// </summary>
        public void LuaPushLightUserData(Object userData)
        {
            LuaDll.lua_pushlightuserdata(NativeState, GetUserDataPtr(userData));
        }
        /// <summary>
        /// Push a thread
        /// </summary>
        /// <returns>True if the thread is the main thread of its state.</returns>
        public bool LuaPushThread()
        {
            return LuaDll.lua_pushthread(NativeState) == 1;
        }
        #endregion

        #region get functions (Lua -> stack)
        /// <summary>
        /// Pushes onto the stack the value of the global name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaGetGlobal(String name)
        {
            return (LuaType)LuaDll.lua_getglobal(NativeState, name);
        }
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given index and k is the value at the top of the stack.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaGetTable(int idx)
        {
            return (LuaType)LuaDll.lua_gettable(NativeState, idx);
        }
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="k"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaGetField(int idx, String k)
        {
            return (LuaType)LuaDll.lua_getfield(NativeState, idx, k);
        }
        /// <summary>
        /// Pushes onto the stack the value t[i], where t is the value at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaGetI(int idx, int n)
        {
            return (LuaType)LuaDll.lua_geti(NativeState, idx, n);
        }
        /// <summary>
        /// Similar to GetTable, but does a raw access
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaRawGet(int idx)
        {
            return (LuaType)LuaDll.lua_rawget(NativeState, idx);
        }
        /// <summary>
        /// Pushes onto the stack the value t[n], where t is the table at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="n"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaRawGetI(int idx, int n)
        {
            return (LuaType)LuaDll.lua_rawgeti(NativeState, idx, n);
        }
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the table at the given index and k is the pointer p represented as a light userdata.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="p"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaRawGetP(int idx, Object p)
        {
            return (LuaType)LuaDll.lua_rawgetp(NativeState, idx, GetUserDataPtr(p));
        }

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// </summary>
        public void LuaCreateTable(int narr, int nrec)
        {
            LuaDll.lua_createtable(NativeState, narr, nrec);
        }
        /// <summary>
        /// This function allocates a new block of memory with the given size, pushes onto the stack a new full userdata with the block address.
        /// </summary>
        public ILuaUserData LuaNewUserData(UInt32 sz)
        {
            var ptr = LuaDll.lua_newuserdata(NativeState, sz);
            var result = new LuaUserData(ptr, sz);
            UserDataIndex.Add(result);
            return result;
        }
        /// <summary>
        /// If the value at the given index has a metatable, the function pushes that metatable onto the stack and returns true. Otherwise, the function returns false and pushes nothing on the stack.
        /// </summary>
        public bool LuaGetMetatable(int objindex)
        {
            return LuaDll.lua_getmetatable(NativeState, objindex) != 0;
        }
        /// <summary>
        /// Pushes onto the stack the Lua value associated with the userdata at the given index.
        /// </summary>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType LuaGetUserValue(int idx)
        {
            return (LuaType)LuaDll.lua_getuservalue(NativeState, idx);
        }
        #endregion

        #region set functions (stack -> Lua)
        /// <summary>
        /// Pops a value from the stack and sets it as the new value of global name.
        /// </summary>
        public void LuaSetGlobal(String name)
        {
            LuaDll.lua_setglobal(NativeState, name);
        }
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given index, v is the value at the top of the stack, and k is the value just below the top.
        /// </summary>
        /// <remarks>
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        public void LuaSetTable(int idx)
        {
            LuaDll.lua_settable(NativeState, idx);
        }
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        public void LuaSetField(int idx, String k)
        {
            LuaDll.lua_setfield(NativeState, idx, k);
        }
        /// <summary>
        /// Does the equivalent to t[n] = v, where t is the value at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        public void LuaSetI(int idx, int n)
        {
            LuaDll.lua_seti(NativeState, idx, n);
        }
        /// <summary>
        /// Similar to SetTable, but does a raw assignment (i.e., without metamethods).
        /// </summary>
        public void LuaRawSet(int idx)
        {
            LuaDll.lua_rawset(NativeState, idx);
        }
        /// <summary>
        /// Does the equivalent of t[i] = v, where t is the table at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods.
        /// </remarks>
        public void LuaRawSetI(int idx, int n)
        {
            LuaDll.lua_rawseti(NativeState, idx, n);
        }
        /// <summary>
        /// Does the equivalent of t[p] = v, where t is the table at the given index, p is encoded as a light userdata, and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods.
        /// </remarks>
        public void LuaRawSetP(int idx, Object p)
        {
            LuaDll.lua_rawsetp(NativeState, idx, GetUserDataPtr(p));
        }

        /// <summary>
        /// Pops a table from the stack and sets it as the new metatable for the value at the given index.
        /// </summary>
        public void LuaSetMetatable(int objindex)
        {
            LuaDll.lua_setmetatable(NativeState, objindex);
        }
        /// <summary>
        /// Pops a value from the stack and sets it as the new value associated to the userdata at the given index.
        /// </summary>
        public void LuaSetUserValue(int idx)
        {
            LuaDll.lua_setuservalue(NativeState, idx);
        }
        #endregion

        #endregion

        #region Comparison and arithmetic functions
        /// <summary>
        /// Arithmetic operation
        /// </summary>
        public void LuaArith(LuaArithOperator op)
        {
            LuaDll.lua_arith(NativeState, (int)op);
        }
        /// <summary>
        /// Do raw equality
        /// </summary>
        public bool LuaRawEqual(int idx1, int idx2)
        {
            return LuaDll.lua_rawequal(NativeState, idx1, idx2) != 0;
        }
        /// <summary>
        /// Compare two values
        /// </summary>
        public bool LuaCompare(int idx1, int idx2, LuaRelOperator op)
        {
            return LuaDll.lua_compare(NativeState, idx1, idx2, (int)op) != 0;
        }
        #endregion

        #region 'load' and 'call' functions (load and run Lua code)
        /// <summary>
        /// Calls a function.
        /// </summary>
        public void LuaCall(int nargs, int nresults)
        {
            LuaDll.lua_call(NativeState, nargs, nresults);
        }
        /// <summary>
        /// Call with yield support
        /// </summary>
        public void LuaCallK(int nargs, int nresults, int ctx, LuaKFunction k)
        {
            // TODO Implements
            throw new NotImplementedException();
        }
        /// <summary>
        /// Calls a function in protected mode.
        /// </summary>
        public LuaStatus LuaPCall(int nargs, int nresults, int errfunc)
        {
            return (LuaStatus)LuaDll.lua_pcall(NativeState, nargs, nresults, errfunc);
        }
        /// <summary>
        /// Call in protected mode with yield support
        /// </summary>
        public LuaStatus LuaPCallK(int nargs, int nresults, int errfunc, int ctx, LuaKFunction k)
        {
            // TODO Implements
            throw new NotImplementedException();
        }
        /// <summary>
        /// Loads a Lua chunk without running it.
        /// </summary>
        public LuaStatus LuaLoad(LuaReader reader, Object dt, String chunkname, String mode)
        {
            return (LuaStatus)LuaDll.lua_load(NativeState, WrapReader(reader), GetUserDataPtr(dt), chunkname, mode);
        }
        /// <summary>
        /// Dumps a function as a binary chunk.
        /// </summary>
        public int LuaDump(LuaWriter writer, Object data, int strip)
        {
            return LuaDll.lua_dump(NativeState, WrapWriter(writer), GetUserDataPtr(data), strip);
        }
        #endregion

        #region coroutine functions
        /// <summary>
        /// Yields a coroutine (thread). 
        /// </summary>
        public LuaStatus LuaYieldK(int nresults, Int64 ctx, LuaKFunction k)
        {
            return (LuaStatus)LuaDll.lua_yieldk(NativeState, nresults, ctx, WrapKFunction(k));
        }
        /// <summary>
        /// Starts and resumes a coroutine in the given thread
        /// </summary>
        public LuaStatus LuaResume(ILuaState from, int narg)
        {
            IntPtr fromState = IntPtr.Zero;
            LuaState ls = GetAsLuaState(from, "from");
            if (ls != null) fromState = ls.NativeState;
            return (LuaStatus)LuaDll.lua_resume(NativeState, fromState, narg);
        }
        /// <summary>
        /// Returns the status of the thread
        /// </summary>
        public LuaStatus LuaStatus()
        {
            return (LuaStatus)LuaDll.lua_status(NativeState);
        }
        /// <summary>
        /// Returns true if the given coroutine can yield, and false otherwise. 
        /// </summary>
        public Boolean LuaIsYieldable()
        {
            return LuaDll.lua_isyieldable(NativeState) != 0;
        }
        /// <summary>
        /// This function is equivalent to YieldK, but it has no continuation.
        /// </summary>
        public LuaStatus LuaYield(int nresults)
        {
            return (LuaStatus)LuaDll.lua_yield(NativeState, nresults);
        }
        #endregion

        #region garbage-collection function and options
        /// <summary>
        /// Controls the garbage collector.
        /// </summary>
        public int LuaGC(LuaGcFunction what, int data)
        {
            return LuaDll.lua_gc(NativeState, (int)what, data);
        }
        #endregion

        #region miscellaneous functions
        /// <summary>
        /// Generates a Lua error, using the value at the top of the stack as the error object.
        /// </summary>
        public void LuaError()
        {
            LuaDll.lua_error(NativeState);
        }
        ///// <summary>
        ///// Pops a key from the stack, and pushes a key–value pair from the table at the given index (the "next" pair after the given key).
        ///// </summary>
        //bool Next(int idx);
        ///// <summary>
        ///// Concatenates the n values at the top of the stack, pops them, and leaves the result at the top.
        ///// </summary>
        //void Concat(int n);
        ///// <summary>
        ///// Returns the length of the value at the given index. It is equivalent to the '#' operator in Lua .
        ///// </summary>
        //void Len(int idx);
        ///// <summary>
        ///// Converts the zero-terminated string s to a number, pushes that number into the stack, and returns the total size of the string, that is, its length plus one.
        ///// </summary>
        //int StringToNumber(String s);
        ////lua_Alloc lua_getallocf(lua_State L, IntPtr ud);
        ////void lua_setallocf(lua_State L, lua_Alloc f, IntPtr ud);
        #endregion

        #region some useful macros
        /// <summary>
        /// Converts the Lua value at the given index to the C type Number 
        /// </summary>
        public Double LuaToNumber(int idx)
        {
            return LuaDll.lua_tonumber(NativeState, idx);
        }
        /// <summary>
        /// Converts the Lua value at the given index to the C type integer
        /// </summary>
        public Int64 LuaToInteger(int idx)
        {
            return LuaDll.lua_tointeger(NativeState, idx);
        }
        /// <summary>
        /// Pops n elements from the stack. 
        /// </summary>
        public void LuaPop(int n)
        {
            LuaDll.lua_pop(NativeState, n);
        }
        ///// <summary>
        ///// Creates a new empty table and pushes it onto the stack.
        ///// </summary>
        //ILuaState NewTable();
        ///// <summary>
        ///// Sets the C function f as the new value of global name.
        ///// </summary>
        //ILuaState Register(String n, LuaFunction f);
        ///// <summary>
        ///// Pushes a function onto the stack. 
        ///// </summary>
        //ILuaState PushFunction(LuaFunction f);
        ///// <summary>
        ///// Returns true if the value at the given index is a function (either C or Lua), and false otherwise. 
        ///// </summary>
        //bool IsFunction(int n);
        ///// <summary>
        ///// Returns true if the value at the given index is a table, and false otherwise. 
        ///// </summary>
        //bool IsTable(int n);
        ///// <summary>
        ///// Returns true if the value at the given index is a light userdata, and false otherwise. 
        ///// </summary>
        //bool IsLightUserData(int n);
        /// <summary>
        /// Returns true if the value at the given index is nil, and false otherwise. 
        /// </summary>
        public bool LuaIsNil(int n)
        {
            return LuaDll.lua_isnil(NativeState, n);
        }
        ///// <summary>
        ///// Returns true if the value at the given index is a boolean, and false otherwise. 
        ///// </summary>
        //bool IsBoolean(int n);
        ///// <summary>
        ///// Returns true if the value at the given index is a thread, and false otherwise. 
        ///// </summary>
        //bool IsThread(int n);
        ///// <summary>
        ///// Returns true if the value at the given index is none, and false otherwise. 
        ///// </summary>
        //bool IsNone(int n);
        ///// <summary>
        ///// Returns true if the value at the given index is none or nil, and false otherwise. 
        ///// </summary>
        //bool IsNoneOrNil(int n);
        ///// <summary>
        ///// Pushes a literal string onto the stack
        ///// </summary>
        //String PushLiteral(String s);
        ///// <summary>
        ///// Pushes the global environment onto the stack. 
        ///// </summary>
        //ILuaState PushGlobalTable();
        ///// <summary>
        ///// Converts the Lua value at the given index to a string.
        ///// </summary>
        //String ToString(int i);
        ///// <summary>
        ///// Moves the top element into the given valid index, shifting up the elements above this index to open space. 
        ///// </summary>
        //ILuaState Insert(int idx);
        ///// <summary>
        ///// Removes the element at the given valid index, shifting down the elements above this index to fill the gap. 
        ///// </summary>
        //ILuaState Remove(int idx);
        ///// <summary>
        ///// Moves the top element into the given valid index without shifting any element (therefore replacing the value at that given index), 
        ///// and then pops the top element. 
        ///// </summary>
        //ILuaState Replace(int idx);
        #endregion

        //#region compatibility macros for unsigned conversions
        ///// <summary>
        ///// Push an unsigned int
        ///// </summary>
        //ILuaState PushUnsigned(UInt32 n);
        ///// <summary>
        ///// Converts the Lua value at the given index to an unsigned int
        ///// </summary>
        //UInt32 ToUnsigned(int idx, out bool isnum);
        ///// <summary>
        ///// Converts the Lua value at the given index to an unsigned int
        ///// </summary>
        //UInt32 ToUnsigned(int idx);
        //#endregion

        //#region Debug API
        ///// <summary>
        ///// Create a new debug info struct
        ///// </summary>
        //ILuaDebug NewLuaDebug();
        ///// <summary>
        ///// Gets information about the interpreter runtime stack. 
        ///// </summary>
        //bool GetStack(int level, ILuaDebug ar);
        ///// <summary>
        ///// Gets information about a specific function or function invocation. 
        ///// </summary>
        //bool GetInfo(LuaGetInfoWhat what, ILuaDebug ar);
        ///// <summary>
        ///// Gets information about a local variable of a given activation record or a given function. 
        ///// </summary>
        //String GetLocal(ILuaDebug ar, int n);
        ///// <summary>
        ///// Sets the value of a local variable of a given activation record. 
        ///// </summary>
        //String SetLocal(ILuaDebug ar, int n);
        ///// <summary>
        ///// Gets information about the n-th upvalue of the closure at index funcindex. 
        ///// </summary>
        //String GetUpvalue(int funcindex, int n);
        ///// <summary>
        ///// Sets the value of a closure's upvalue. 
        ///// </summary>
        //String SetUpvalue(int funcindex, int n);
        ///// <summary>
        ///// Returns a unique identifier for the upvalue numbered n from the closure at index funcindex. 
        ///// </summary>
        //Int32 UpvalueId(int fidx, int n);
        ///// <summary>
        ///// Make the n1-th upvalue of the Lua closure at index funcindex1 refer to the n2-th upvalue of the Lua closure at index funcindex2. 
        ///// </summary>
        //void UpvalueJoin(int fidx1, int n1, int fidx2, int n2);
        ///// <summary>
        ///// Sets the debugging hook function. 
        ///// </summary>
        //void SetHook(LuaHook func, LuaHookMask mask, int count);
        ///// <summary>
        ///// Returns the current hook function. 
        ///// </summary>
        //LuaHook GetHook();
        ///// <summary>
        ///// Returns the current hook mask. 
        ///// </summary>
        //LuaHookMask GetHookMask();
        ///// <summary>
        ///// Returns the current hook count. 
        ///// </summary>
        //int GetHookCount();
        //#endregion

        //#region lauxlib

        ///// <summary>
        ///// Checks whether the core running the call, the core that created the Lua state, and the code making the call are all using the same version of Lua.
        ///// </summary>
        //ILuaState CheckVersion();
        ///// <summary>
        ///// Pushes onto the stack the field e from the metatable of the object at index obj and returns the type of pushed value.
        ///// </summary>
        //LuaType GetMetaField(int obj, String e);
        ///// <summary>
        ///// Calls a metamethod. 
        ///// </summary>
        //Boolean CallMeta(int obj, String e);
        ////    extern static IntPtr _luaL_tolstring(lua_State L, int idx, out int len);
        ////    public static String luaL_tolstring(lua_State L, int idx, out int len)
        ///// <summary>
        ///// Raises an error reporting a problem with argument arg of the C function that called it
        ///// </summary>
        //void ArgError(int arg, String extramsg);
        ///// <summary>
        ///// Checks whether the function argument arg is a string and returns this string; if l is not null fills l with the string's length.
        ///// </summary>
        //String CheckLString(int arg, out UInt32 l);
        ///// <summary>
        ///// If the function argument arg is a string, returns this string. If this argument is absent or is nil, returns d. Otherwise, raises an error. 
        ///// </summary>
        //String OptLString(int arg, String def, out UInt32 l);
        ///// <summary>
        ///// Checks whether the function argument arg is a number and returns this number. 
        ///// </summary>
        //Double CheckNumber(int arg);
        ///// <summary>
        ///// If the function argument arg is a number, returns this number. If this argument is absent or is nil, returns d. Otherwise, raises an error. 
        ///// </summary>
        //Double OptNumber(int arg, Double def);
        ///// <summary>
        ///// Checks whether the function argument arg is an integer (or can be converted to an integer) and returns this integer cast to a Int32. 
        ///// </summary>
        //Int32 CheckInteger(int arg);
        ///// <summary>
        ///// If the function argument arg is an integer (or convertible to an integer), returns this integer. If this argument is absent or is nil, returns d. Otherwise, raises an error. 
        ///// </summary>
        //Int32 OptInteger(int arg, Int32 def);
        ///// <summary>
        ///// Grows the stack size to top + sz elements, raising an error if the stack cannot grow to that size.
        ///// </summary>
        //ILuaState CheckStack(int sz, String msg);
        ///// <summary>
        ///// Checks whether the function argument arg has type t.
        ///// </summary>
        //ILuaState CheckType(int arg, LuaType t);
        ///// <summary>
        ///// Checks whether the function has an argument of any type (including nil) at position arg. 
        ///// </summary>
        //ILuaState CheckAny(int arg);
        ///// <summary>
        ///// Create and register a new metatable
        ///// </summary>
        ///// <remarks>
        ///// If the registry already has the key tname, returns false. Otherwise, creates a new table to be used as a metatable for userdata, adds 
        ///// to this new table the pair __name = tname, adds to the registry the pair [tname] = new table, and returns true. 
        ///// (The entry __name is used by some error-reporting functions.) 
        ///// In both cases pushes onto the stack the final value associated with tname in the registry. 
        ///// </remarks>
        //bool NewMetatable(String tname);
        ///// <summary>
        ///// Sets the metatable of the object at the top of the stack as the metatable associated with name tname in the registry
        ///// </summary>
        //ILuaState SetMetatable(String tname);
        ///// <summary>
        ///// Checks whether the function argument arg is a userdata of the type tname and returns the userdata address. 
        ///// </summary>
        //Object Checkudata(int arg, String tname);
        ///// <summary>
        ///// This function works like CheckUData, except that, when the test fails, it returns null instead of raising an error. 
        ///// </summary>
        //Object TestUData(int arg, String tname);
        ///// <summary>
        ///// Pushes onto the stack a string identifying the current position of the control at level lvl in the call stack
        ///// </summary>
        //ILuaState Where(int lvl);
        ///// <summary>
        ///// Raises an error.
        ///// </summary>
        //void Error(String message);
        ///// <summary>
        ///// Raises an error.
        ///// </summary>
        //void Error(String fmt, String arg0);
        ///// <summary>
        ///// Raises an error.
        ///// </summary>
        //void Error(String fmt, String arg0, String arg1);

        ////    //[DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        ////    //public extern static int luaL_checkoption(lua_State L, int arg, String def, String[] lst);
        ////    // TODO Check the String[] transmission

        ////    [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        ////    public extern static int luaL_fileresult(lua_State L, int stat, String fname);
        ////    [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        ////    public extern static int luaL_execresult(lua_State L, int stat);

        ///// <summary>
        ///// Creates and returns a reference, in the table at index 'table', for the object at the top of the stack (and pops the object). 
        ///// </summary>
        //int LRef(int table);
        ///// <summary>
        ///// Releases reference reference from the table at index table.
        ///// </summary>
        ///// <remarks>
        ///// The entry is removed from the table, so that the referred object can be collected. The reference reference is also freed to be used again. 
        ///// </remarks>
        //void LUnref(int table, int reference);

        ///// <summary>
        ///// Loads a file as a Lua chunk. 
        ///// </summary>
        //LuaStatus LoadFile(String filename, String mode);
        ///// <summary>
        ///// Loads a file as a Lua chunk. 
        ///// </summary>
        //LuaStatus LoadFile(String filename);

        ////    [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        ////    public extern static int luaL_loadbufferx(lua_State L, String buff, int sz, String name, String mode);

        ///// <summary>
        ///// Loads a string as a Lua chunk. 
        ///// </summary>
        //LuaStatus LoadString(String s);
        ///// <summary>
        ///// Returns the "length" of the value at the given index as a number; it is equivalent to the '#' operator in Lua 
        ///// </summary>
        //Int32 LLen(int idx);
        ///// <summary>
        ///// Creates a copy of string s by replacing any occurrence of the string p with the string r. 
        ///// Pushes the resulting string on the stack and returns it. 
        ///// </summary>
        //String GSub(String s, String p, String r);
        ///// <summary>
        ///// Registers all functions in the array l (see luaL_Reg) into the table on the top of the stack
        ///// </summary>
        //ILuaState SetFuncs(IEnumerable<Tuple<String, LuaFunction>> l, int nup);
        ///// <summary>
        ///// Ensures that the value t[fname], where t is the value at index idx, is a table, and pushes that table onto the stack. 
        ///// Returns true if it finds a previous table there and false if it creates a new table. 
        ///// </summary>
        //bool GetSubTable(int idx, String fname);
        ///// <summary>
        ///// Creates and pushes a traceback of the stack L1. 
        ///// </summary>
        ///// <remarks>
        ///// If msg is not NULL it is appended at the beginning of the traceback. The level parameter tells at which level to start the traceback. 
        ///// </remarks>
        //ILuaState Traceback(ILuaState L1, String msg, int level);
        ////    public extern static void luaL_requiref(lua_State L, String modname, lua_CFunction openf, int glb);

        //#region some useful macros
        ///// <summary>
        ///// Creates a new table with a size optimized to store all entries in the array l
        ///// </summary>
        //ILuaState NewLibTable(Tuple<String, LuaFunction>[] l);
        ///// <summary>
        ///// Creates a new table and registers there the functions in list l. 
        ///// </summary>
        //ILuaState NewLib(Tuple<String, LuaFunction>[] l);
        ///// <summary>
        ///// Checks whether cond is true. If it is not, raises an error with a standard message 
        ///// </summary>
        //ILuaState ArgCheck(bool cond, int arg, String extramsg);
        ////    public static String luaL_checkstring(lua_State L, int n)
        ////    public static String luaL_optstring(lua_State L, int n, String def)
        ///// <summary>
        ///// Returns the name of the type of the value at the given index. 
        ///// </summary>
        //String TypeName(int idx);
        ///// <summary>
        ///// Loads and runs the given file.
        ///// </summary>
        //LuaStatus DoFile(String fn);
        ///// <summary>
        ///// Loads and runs the given string.
        ///// </summary>
        //LuaStatus DoString(String s);
        ////    public static int luaL_getmetatable(lua_State L, String n) { return lua_getfield(L, LUA_REGISTRYINDEX, (n)); }
        ////    //public static int luaL_opt(lua_State L, lua_CFunction f, int n, int d) { return (lua_isnoneornil(L, (n)) ? (d) : f(L, (n))); }
        ////    //#define luaL_opt(L,f,n,d)	(lua_isnoneornil(L,(n)) ? (d) : f(L,(n)))
        ///// <summary>
        ///// Loads a buffer as a Lua chunk. 
        ///// </summary>
        //LuaStatus LoadBuffer(String s, int sz, String n);
        //#endregion

        //#region Acces to the "Abstraction Layer" for basic report of messages and errors
        ///// <summary>
        ///// print a string
        ///// </summary>
        //ILuaState WriteString(String s);
        ///// <summary>
        ///// print a newline and flush the output
        ///// </summary>
        //ILuaState WriteLine();
        ///// <summary>
        ///// print an error message
        ///// </summary>
        //ILuaState WriteStringError(String s, String p);
        //#endregion

        ////    /*
        ////    ** {============================================================
        ////    ** Compatibility with deprecated conversions
        ////    ** =============================================================
        ////    */
        ////    //#if defined(LUA_COMPAT_APIINTCASTS)

        ////    public static lua_Unsigned luaL_checkunsigned(lua_State L, int a) { return (lua_Unsigned)luaL_checkinteger(L, a); }
        ////    public static lua_Unsigned luaL_optunsigned(lua_State L, int a, int d)
        ////    { return (lua_Unsigned)luaL_optinteger(L, a, d); }

        ////    public static int luaL_checkint(lua_State L, int n) { return luaL_checkinteger(L, (n)); }
        ////    public static int luaL_optint(lua_State L, int n, int d) { return luaL_optinteger(L, (n), d); }

        ////    public static long luaL_checklong(lua_State L, int n) { return (long)luaL_checkinteger(L, (n)); }
        ////    public static long luaL_optlong(lua_State L, int n, int d) { return (long)luaL_optinteger(L, (n), d); }

        ////    //#endif
        ////    /* }============================================================ */

        //#endregion

        //#region lualib

        //int OpenBase();
        //int OpenCoroutine();
        //int OpenTable();
        //int OpenIo();
        //int OpenOs();
        //int OpenString();
        //int OpenUtf8();
        //int OpenBit32();
        //int OpenMath();
        //int OpenDebug();
        //int OpenPackage();
        //ILuaState OpenLibs();

        //#endregion

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

        /// <summary>
        /// The user data index
        /// </summary>
        public UserDataIndex UserDataIndex { get; private set; }

    }
}
