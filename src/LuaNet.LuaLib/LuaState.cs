using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet.LuaLib
{

    /// <summary>
    /// Lua state proxy
    /// </summary>
    public sealed class LuaState : ILuaState
    {
        static Dictionary<IntPtr, LuaState> _RegisteredStates = new Dictionary<IntPtr, LuaState>();
        IntPtr _NativeState;
        bool _OwnNativeState = true;
        LuaFunction _OriginalAtPanic = null;
        LuaState _MainState;

        #region Ctor and Dispose

        /// <summary>
        /// Create a new Lua state
        /// </summary>
        public LuaState()
        {
            var ptr = Lua.luaL_newstate();
            if (ptr == IntPtr.Zero)
                throw new OutOfMemoryException("Cannot create state: not enough memory");
            InitState(ptr, true);
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        private LuaState(IntPtr ptr, bool ownState)
        {
            InitState(ptr, ownState);
        }

        /// <summary>
        /// Initialize the state
        /// </summary>
        private void InitState(IntPtr nativeState, bool ownState)
        {
            _NativeState = nativeState;
            lock (_RegisteredStates)
            _RegisteredStates[_NativeState] = this;
            _OwnNativeState = ownState;
            SetDefaultAtPanic();
            _MainState = null;
            if (!ownState)
            {
                // Retreive the main thread
                Lua.lua_geti(nativeState, Lua.LUA_REGISTRYINDEX, Lua.LUA_RIDX_MAINTHREAD);
                var mt = Lua.lua_tothread(nativeState, -1);
                Lua.lua_pop(nativeState, 1);
                _MainState = FindState(mt, false);
            }
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()
        {
            if (_NativeState != IntPtr.Zero)
            {
                RestoreOriginalAtPanic();
                if (_OwnNativeState)
                {
                    // Unregister all 'child' states
                    List<LuaState> childs = null;
                    lock(_RegisteredStates)
                        childs = _RegisteredStates.Values.Where(c => c._MainState == this).ToList();
                    foreach (var child in childs)
                        child.Dispose();
                    Lua.lua_close(_NativeState);
                }
                lock (_RegisteredStates)
                    _RegisteredStates.Remove(_NativeState);
                _NativeState = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Recherche un état lua enregistré
        /// </summary>
        internal static LuaState FindState(IntPtr l, bool creatIfNotExists)
        {
            LuaState result;
            if (!_RegisteredStates.TryGetValue(l, out result))
            {
                if (creatIfNotExists && l != IntPtr.Zero)
                {
                    result = new LuaState(l, false);
                }
                else
                    return null;
            }
            return result;
        }

        #endregion

        #region Lua engine informations

        /// <summary>
        /// Engine name
        /// </summary>
        public String LuaEngine { get { return Lua.LuaDllName; } }

        /// <summary>
        /// Major Version part
        /// </summary>
        public String LuaVersionMajor { get { return Lua.LUA_VERSION_MAJOR; } }
        /// <summary>
        /// Minor Version part
        /// </summary>
        public String LuaVersionMinor { get { return Lua.LUA_VERSION_MINOR; } }
        /// <summary>
        /// Release Version part
        /// </summary>
        public String LuaVersionRelease { get { return Lua.LUA_VERSION_RELEASE; } }
        /// <summary>
        /// Version number
        /// </summary>
        public Double LuaVersionNum { get { return Lua.LUA_VERSION_NUM; } }

        /// <summary>
        /// Lua Version
        /// </summary>
        public String LuaVersion { get { return Lua.LUA_VERSION; } }
        /// <summary>
        /// Lua Release
        /// </summary>
        public String LuaRelease { get { return Lua.LUA_RELEASE; } }
        /// <summary>
        /// Lua Copyright
        /// </summary>
        public String LuaCopyright { get { return Lua.LUA_COPYRIGHT; } }
        /// <summary>
        /// Lua Authors
        /// </summary>
        public String LuaAuthors { get { return Lua.LUA_AUTHORS; } }

        /// <summary>
        /// Option for multiple returns in 'PCall' and 'Call' 
        /// </summary>
        public int MultiReturns { get { return Lua.LUA_MULTRET; } }

        /// <summary>
        /// Index of the registry
        /// </summary>
        public int RegistryIndex { get { return Lua.LUA_REGISTRYINDEX; } }

        /// <summary>
        /// Minimum stack size
        /// </summary>
        public int MinStack { get { return Lua.LUA_MINSTACK; } }

        #endregion

        #region State management

        /// <summary>
        /// Get the version number used to create this state
        /// </summary>
        public Double Version() { return Lua.lua_version(NativeState); }

        /// <summary>
        /// Define the panic function
        /// </summary>
        public LuaFunction AtPanic(LuaFunction panicf)
        {
            if (panicf == null) throw new ArgumentNullException("panicf");
            var oldFunc = Lua.lua_atpanic(NativeState, panicf.ToCFunction());
            return oldFunc.ToFunction();
        }

        /// <summary>
        /// Default AtPanic function, raised an LuaAtPanicException
        /// </summary>
        static private int DefaultLuaStateAtPanic(ILuaState state)
        {
            throw new LuaAtPanicException(state != null ? state.ToString(-1) : String.Empty);
        }

        /// <summary>
        /// Define the default AtPanic function
        /// </summary>
        public void SetDefaultAtPanic()
        {
            var o = AtPanic(DefaultLuaStateAtPanic);
            if (_OriginalAtPanic == null) _OriginalAtPanic = o;
        }

        /// <summary>
        /// Restore the original at panic
        /// </summary>
        public bool RestoreOriginalAtPanic()
        {
            if (_OriginalAtPanic != null)
            {
                AtPanic(_OriginalAtPanic);
                _OriginalAtPanic = null;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Creates a new thread, pushes it on the stack.
        /// </summary>
        public ILuaState NewThread()
        {
            var thread = Lua.lua_newthread(NativeState);
            return new LuaState(thread, false);
        }

        #endregion

        /// <summary>
        /// Récupère le LuaState correspondant à un ILuaState
        /// </summary>
        LuaState GetAsLuaState(ILuaState state, String argName)
        {
            if (state == null) return null;
            LuaState ls = state as LuaState;
            if (ls == null)
            {
                if (!String.IsNullOrWhiteSpace(argName))
                    throw new InvalidOperationException(String.Format("The '{0}' state is not a supported state.", argName));
                else
                    throw new InvalidOperationException("The state is not a supported state.");
            }
            return ls;
        }

        #region Stack management

        #region basic stack manipulation
        /// <summary>
        /// Get the absolute stack index
        /// </summary>
        public int AbsIndex(int idx)
        {
            return Lua.lua_absindex(NativeState, idx);
        }
        /// <summary>
        /// Get the top of the stack
        /// </summary>
        public int GetTop()
        {
            return Lua.lua_gettop(NativeState);
        }
        /// <summary>
        /// Set the top of the stack
        /// </summary>
        public ILuaState SetTop(int idx)
        {
            Lua.lua_settop(NativeState, idx);
            return this;
        }
        /// <summary>
        /// Push a value on the stack
        /// </summary>
        public ILuaState PushValue(int idx)
        {
            Lua.lua_pushvalue(NativeState, idx);
            return this;
        }
        /// <summary>
        /// Rotates the n stack elements between the valid index idx and the top of the stack
        /// </summary>
        public ILuaState Rotate(int idx, int n)
        {
            Lua.lua_rotate(NativeState, idx, n);
            return this;
        }
        /// <summary>
        /// Copies the element at index fromidx into the valid index toidx, replacing the value at that position
        /// </summary>
        public ILuaState Copy(int fromidx, int toidx)
        {
            Lua.lua_copy(NativeState, fromidx, toidx);
            return this;
        }
        /// <summary>
        /// Ensures that the stack has space for at least n extra slots
        /// </summary>
        public bool CheckStack(int n) { return Lua.lua_checkstack(NativeState, n) != 0; }
        /// <summary>
        /// Exchange values between different threads of the same state
        /// </summary>
        public ILuaState XMove(ILuaState to, int n)
        {
            if (to == null) throw new ArgumentNullException("to");
            LuaState stTo = GetAsLuaState(to, "to");
            Lua.lua_xmove(NativeState, stTo.NativeState, n);
            return this;
        }
        #endregion

        #region access functions (stack -> C)
        /// <summary>
        /// Returns true if the value at the given index is a number or a string convertible to a number, and false otherwise.
        /// </summary>
        public Boolean IsNumber(int idx) { return Lua.lua_isnumber(NativeState, idx) != 0; }
        /// <summary>
        /// Returns true if the value at the given index is a string or a number (which is always convertible to a string), and false otherwise.
        /// </summary>
        public Boolean IsString(int idx) { return Lua.lua_isstring(NativeState, idx) != 0; }
        /// <summary>
        /// Returns true if the value at the given index is a C function, and false otherwise.
        /// </summary>
        public Boolean IsCFunction(int idx) { return Lua.lua_iscfunction(NativeState, idx) != 0; }
        /// <summary>
        /// Returns true if the value at the given index is an integer (that is, the value is a number and is represented as an integer), and false otherwise.
        /// </summary>
        public Boolean IsInteger(int idx) { return Lua.lua_isinteger(NativeState, idx) != 0; }
        /// <summary>
        /// Returns true if the value at the given index is a userdata (either full or light), and false otherwise.
        /// </summary>
        public Boolean IsUserData(int idx) { return Lua.lua_isuserdata(NativeState, idx) != 0; }
        /// <summary>
        /// Returns the type of the value in the given valid index, or LUA_TNONE for a non-valid (but acceptable) index
        /// </summary>
        public LuaType Type(int idx) { return (LuaType)Lua.lua_type(NativeState, idx); }
        /// <summary>
        /// Returns the name of the type encoded by the value tp, which must be one the values returned by Type.
        /// </summary>
        public String TypeName(LuaType tp) { return Lua.lua_typename(NativeState, (int)tp); }
        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Number
        /// </summary>
        public Double ToNumber(int idx, out bool isnum)
        {
            int isn;
            var res = Lua.lua_tonumberx(NativeState, idx, out isn);
            isnum = isn != 0;
            return res;
        }
        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Integer
        /// </summary>
        public int ToInteger(int idx, out bool isnum)
        {
            int isn;
            var res = Lua.lua_tointegerx(NativeState, idx, out isn);
            isnum = isn != 0;
            return (int)res;
        }
        /// <summary>
        /// Converts the Lua value at the given index to a C boolean value
        /// </summary>
        public Boolean ToBoolean(int idx) { return Lua.lua_toboolean(NativeState, idx) != 0; }
        /// <summary>
        /// Returns the raw "length" of the value at the given index
        /// </summary>
        /// <remarks>
        /// Returns the raw "length" of the value at the given index: for strings, this is the string length; 
        /// for tables, this is the result of the length operator ('#') with no metamethods; for userdata, this is the size 
        /// of the block of memory allocated for the userdata; for other values, it is 0.
        /// </remarks>
        public UInt32 RawLen(int idx) { return Lua.lua_rawlen(NativeState, idx); }
        /// <summary>
        /// Converts a value at the given index to a C function
        /// </summary>
        public LuaFunction ToCFunction(int idx) { return Lua.lua_tocfunction(NativeState, idx).ToFunction(); }
        /// <summary>
        /// If the value at the given index is a full userdata, returns its block address. If the value is a light userdata, returns its pointer. Otherwise, returns NULL.
        /// </summary>
        public Object ToUserData(int idx)
        {
            var ptr = Lua.lua_touserdata(NativeState, idx);
            return UserDataRef.GetData(ptr);
        }
        /// <summary>
        /// Converts the value at the given index to a Lua thread
        /// </summary>
        public ILuaState ToThread(int idx)
        {
            IntPtr st = Lua.lua_tothread(NativeState, idx);
            if (st == IntPtr.Zero) return null;
            return LuaState.FindState(st, true);
        }
        #endregion

        #region push functions (C -> stack)
        /// <summary>
        /// Push a nil value
        /// </summary>
        public ILuaState PushNil() { Lua.lua_pushnil(NativeState); return this; }
        /// <summary>
        /// Push a number value
        /// </summary>
        public ILuaState PushNumber(Double n) { Lua.lua_pushnumber(NativeState, n); return this; }
        /// <summary>
        /// Push a integer value
        /// </summary>
        public ILuaState PushInteger(int n) { Lua.lua_pushinteger(NativeState, n); return this; }
        /// <summary>
        /// Push a String value
        /// </summary>
        public ILuaState PushString(String s) { Lua.lua_pushstring(NativeState, s); return this; }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, String arg0) { return Lua.lua_pushfstring(NativeState, fmt, arg0); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, Double arg0) { return Lua.lua_pushfstring(NativeState, fmt, arg0); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, int arg0) { return Lua.lua_pushfstring(NativeState, fmt, arg0); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, String arg0, String arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, String arg0, Double arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, String arg0, int arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, Double arg0, String arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, Double arg0, Double arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, Double arg0, int arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, int arg0, String arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, int arg0, Double arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a formatted string value
        /// </summary>
        public String PushFString(String fmt, int arg0, int arg1) { return Lua.lua_pushfstring(NativeState, fmt, arg0, arg1); }
        /// <summary>
        /// Push a C closure
        /// </summary>
        public ILuaState PushCClosure(LuaFunction fn, int n) { Lua.lua_pushcclosure(NativeState, fn.ToCFunction(), n); return this; }
        /// <summary>
        /// Push a boolean value
        /// </summary>
        public ILuaState PushBoolean(Boolean b) { Lua.lua_pushboolean(NativeState, b ? 1 : 0); return this; }
        /// <summary>
        /// Push a light user data
        /// </summary>
        public ILuaState PushLightUserData(Object userData)
        {
            if (userData == null) throw new ArgumentNullException("userData");
            IntPtr ptr = UserDataRef.GetRef(userData);
            Lua.lua_pushlightuserdata(NativeState, ptr);
            return this;
        }
        /// <summary>
        /// Push a thread
        /// </summary>
        public int PushThread() { return Lua.lua_pushthread(NativeState); }
        #endregion

        #region get functions (Lua -> stack)
        /// <summary>
        /// Pushes onto the stack the value of the global name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType GetGlobal(String name) { return (LuaType)Lua.lua_getglobal(NativeState, name); }
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given index and k is the value at the top of the stack.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType GetTable(int idx) { return (LuaType)Lua.lua_gettable(NativeState, idx); }
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="k"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType GetField(int idx, String k) { return (LuaType)Lua.lua_getfield(NativeState, idx, k); }
        /// <summary>
        /// Pushes onto the stack the value t[i], where t is the value at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType GetI(int idx, int n) { return (LuaType)Lua.lua_geti(NativeState, idx, n); }
        /// <summary>
        /// Similar to GetTable, but does a raw access
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType RawGet(int idx) { return (LuaType)Lua.lua_rawget(NativeState, idx); }
        /// <summary>
        /// Pushes onto the stack the value t[n], where t is the table at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="n"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType RawGetI(int idx, int n) { return (LuaType)Lua.lua_rawgeti(NativeState, idx, n); }
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the table at the given index and k is the pointer p represented as a light userdata.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="p"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType RawGetP(int idx, IntPtr p) { return (LuaType)Lua.lua_rawgetp(NativeState, idx, p); }

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// </summary>
        public ILuaState CreateTable(int narr, int nrec) { Lua.lua_createtable(NativeState, narr, nrec); return this; }
        /// <summary>
        /// This function allocates a new block of memory with the given size, pushes onto the stack a new full userdata with the block address.
        /// </summary>
        public IntPtr NewUserData(UInt32 sz) { return Lua.lua_newuserdata(NativeState, sz); }
        /// <summary>
        /// If the value at the given index has a metatable, the function pushes that metatable onto the stack and returns true. Otherwise, the function returns false and pushes nothing on the stack.
        /// </summary>
        public bool GetMetatable(int objindex) { return Lua.lua_getmetatable(NativeState, objindex) != 0; }
        /// <summary>
        /// Pushes onto the stack the Lua value associated with the userdata at the given index.
        /// </summary>
        /// <returns>Returns the type of the pushed value.</returns>
        public LuaType GetUserValue(int idx) { return (LuaType)Lua.lua_getuservalue(NativeState, idx); }
        #endregion

        #region set functions (stack -> Lua)
        /// <summary>
        /// Pops a value from the stack and sets it as the new value of global name.
        /// </summary>
        public ILuaState SetGlobal(String name) { Lua.lua_setglobal(NativeState, name); return this; }
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given index, v is the value at the top of the stack, and k is the value just below the top.
        /// </summary>
        /// <remarks>
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        public ILuaState SetTable(int idx) { Lua.lua_settable(NativeState, idx); return this; }
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        public ILuaState SetField(int idx, String k) { Lua.lua_setfield(NativeState, idx, k); return this; }
        /// <summary>
        /// Does the equivalent to t[n] = v, where t is the value at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        public ILuaState SetI(int idx, int n) { Lua.lua_seti(NativeState, idx, n); return this; }
        /// <summary>
        /// Similar to SetTable, but does a raw assignment (i.e., without metamethods).
        /// </summary>
        public ILuaState RawSet(int idx) { Lua.lua_rawset(NativeState, idx); return this; }
        /// <summary>
        /// Does the equivalent of t[i] = v, where t is the table at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods.
        /// </remarks>
        public ILuaState RawSetI(int idx, int n) { Lua.lua_rawseti(NativeState, idx, n); return this; }
        /// <summary>
        /// Does the equivalent of t[p] = v, where t is the table at the given index, p is encoded as a light userdata, and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods.
        /// </remarks>
        public ILuaState RawSetP(int idx, IntPtr p) { Lua.lua_rawsetp(NativeState, idx, p); return this; }
        /// <summary>
        /// Pops a table from the stack and sets it as the new metatable for the value at the given index.
        /// </summary>
        public ILuaState SetMetatable(int objindex) { Lua.lua_setmetatable(NativeState, objindex); return this; }
        /// <summary>
        /// Pops a value from the stack and sets it as the new value associated to the userdata at the given index.
        /// </summary>
        public ILuaState SetUserValue(int idx) { Lua.lua_setuservalue(NativeState, idx); return this; }
        #endregion

        #endregion

        #region Comparison and arithmetic functions
        /// <summary>
        /// Arithmetic operation
        /// </summary>
        public void Arith(LuaArithOperator op) { Lua.lua_arith(NativeState, (int)op); }
        /// <summary>
        /// Do raw equality
        /// </summary>
        public bool RawEqual(int idx1, int idx2) { return Lua.lua_rawequal(NativeState, idx1, idx2) != 0; }
        /// <summary>
        /// Compare two values
        /// </summary>
        public bool Compare(int idx1, int idx2, LuaRelOperator op) { return Lua.lua_compare(NativeState, idx1, idx2, (int)op) != 0; }
        #endregion

        #region 'load' and 'call' functions (load and run Lua code)
        /// <summary>
        /// Calls a function.
        /// </summary>
        public ILuaState Call(int nargs, int nresults) { Lua.lua_call(NativeState, nargs, nresults); return this; }
        /// <summary>
        /// Call with yield support
        /// </summary>
        public ILuaState CallK(int nargs, int nresults, int ctx, LuaKFunction k) { throw new NotImplementedException(); }
        /// <summary>
        /// Calls a function in protected mode.
        /// </summary>
        public LuaStatus PCall(int nargs, int nresults, int errfunc) { return (LuaStatus)Lua.lua_pcall(NativeState, nargs, nresults, errfunc); }
        /// <summary>
        /// Call in protected mode with yield support
        /// </summary>
        public LuaStatus PCallK(int nargs, int nresults, int errfunc, int ctx, LuaKFunction k) { throw new NotImplementedException(); }
        /// <summary>
        /// Loads a Lua chunk without running it.
        /// </summary>
        public LuaStatus Load(LuaReader reader, Object dt, String chunkname, String mode)
        { return (LuaStatus)Lua.lua_load(NativeState, reader.ToLuaReader(), UserDataRef.GetRef(dt), chunkname, mode); }
        /// <summary>
        /// Dumps a function as a binary chunk.
        /// </summary>
        public int Dump(LuaWriter writer, Object data, int strip)
        {
            return Lua.lua_dump(NativeState, writer.ToLuaWriter(), UserDataRef.GetRef(data), strip);
        }
        #endregion

        #region coroutine functions
        /// <summary>
        /// Yields a coroutine (thread). 
        /// </summary>
        public LuaStatus YieldK(int nresults, int ctx, LuaKFunction k) { throw new NotImplementedException(); }
        /// <summary>
        /// Starts and resumes a coroutine in the given thread
        /// </summary>
        public LuaStatus Resume(ILuaState from, int narg)
        {
            IntPtr fromState = IntPtr.Zero;
            LuaState ls = GetAsLuaState(from, "from");
            if (ls != null) fromState = ls.NativeState;
            return (LuaStatus)Lua.lua_resume(NativeState, fromState, narg);
        }
        /// <summary>
        /// Returns the status of the thread
        /// </summary>
        public LuaStatus Status() { return (LuaStatus)Lua.lua_status(NativeState); }
        /// <summary>
        /// Returns true if the given coroutine can yield, and false otherwise. 
        /// </summary>
        public Boolean IsYieldable() { return Lua.lua_isyieldable(NativeState) != 0; }
        /// <summary>
        /// This function is equivalent to YieldK, but it has no continuation.
        /// </summary>
        public LuaStatus Yield(int nresults) { return (LuaStatus)Lua.lua_yield(NativeState, nresults); }
        #endregion

        #region garbage-collection function and options
        /// <summary>
        /// Controls the garbage collector.
        /// </summary>
        public int GC(LuaGcFunction what, int data) { return Lua.lua_gc(NativeState, (int)what, data); }
        #endregion

        #region miscellaneous functions
        /// <summary>
        /// Generates a Lua error, using the value at the top of the stack as the error object.
        /// </summary>
        public int Error() { return Lua.lua_error(NativeState); }
        /// <summary>
        /// Pops a key from the stack, and pushes a key–value pair from the table at the given index (the "next" pair after the given key).
        /// </summary>
        public bool Next(int idx) { return Lua.lua_next(NativeState, idx) != 0; }
        /// <summary>
        /// Concatenates the n values at the top of the stack, pops them, and leaves the result at the top.
        /// </summary>
        public void Concat(int n) { Lua.lua_concat(NativeState, n); }
        /// <summary>
        /// Returns the length of the value at the given index. It is equivalent to the '#' operator in Lua .
        /// </summary>
        public void Len(int idx) { Lua.lua_len(NativeState, idx); }
        /// <summary>
        /// Converts the zero-terminated string s to a number, pushes that number into the stack, and returns the total size of the string, that is, its length plus one.
        /// </summary>
        public int StringToNumber(String s) { return Lua.lua_stringtonumber(NativeState, s); }
        //lua_Alloc lua_getallocf(lua_State L, IntPtr ud);
        //void lua_setallocf(lua_State L, lua_Alloc f, IntPtr ud);
        #endregion

        #region some useful macros
        /// <summary>
        /// Converts the Lua value at the given index to the C type Number 
        /// </summary>
        public Double ToNumber(int idx) { return Lua.lua_tonumber(NativeState, idx); }
        /// <summary>
        /// Converts the Lua value at the given index to the C type integer
        /// </summary>
        public int ToInteger(int idx) { return (int)Lua.lua_tointeger(NativeState, idx); }
        /// <summary>
        /// Pops n elements from the stack. 
        /// </summary>
        public ILuaState Pop(int n) { Lua.lua_pop(NativeState, n); return this; }
        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// </summary>
        public ILuaState NewTable() { Lua.lua_newtable(NativeState); return this; }
        /// <summary>
        /// Sets the C function f as the new value of global name.
        /// </summary>
        public ILuaState Register(String n, LuaFunction f) { Lua.lua_register(NativeState, n, f.ToCFunction()); return this; }
        /// <summary>
        /// Pushes a function onto the stack. 
        /// </summary>
        public ILuaState PushFunction(LuaFunction f)
        {
            if (f == null) throw new ArgumentNullException("f");
            Lua.lua_pushcfunction(NativeState, f.ToCFunction());
            return this;
        }
        /// <summary>
        /// Returns true if the value at the given index is a function (either C or Lua), and false otherwise. 
        /// </summary>
        public bool IsFunction(int n) { return Lua.lua_isfunction(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is a table, and false otherwise. 
        /// </summary>
        public bool IsTable(int n) { return Lua.lua_istable(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is a light userdata, and false otherwise. 
        /// </summary>
        public bool IsLightUserData(int n) { return Lua.lua_islightuserdata(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is nil, and false otherwise. 
        /// </summary>
        public bool IsNil(int n) { return Lua.lua_isnil(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is a boolean, and false otherwise. 
        /// </summary>
        public bool IsBoolean(int n) { return Lua.lua_isboolean(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is a thread, and false otherwise. 
        /// </summary>
        public bool IsThread(int n) { return Lua.lua_isthread(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is none, and false otherwise. 
        /// </summary>
        public bool IsNone(int n) { return Lua.lua_isnone(NativeState, n); }
        /// <summary>
        /// Returns true if the value at the given index is none or nil, and false otherwise. 
        /// </summary>
        public bool IsNoneOrNil(int n) { return Lua.lua_isnoneornil(NativeState, n); }
        /// <summary>
        /// Pushes a literal string onto the stack
        /// </summary>
        public String PushLiteral(String s) { return Lua.lua_pushliteral(NativeState, s); }
        /// <summary>
        /// Pushes the global environment onto the stack. 
        /// </summary>
        public ILuaState PushGlobalTable() { Lua.lua_pushglobaltable(NativeState); return this; }
        /// <summary>
        /// Converts the Lua value at the given index to a string.
        /// </summary>
        public String ToString(int i) { return Lua.lua_tostring(NativeState, i); }
        /// <summary>
        /// Moves the top element into the given valid index, shifting up the elements above this index to open space. 
        /// </summary>
        public ILuaState Insert(int idx) { Lua.lua_insert(NativeState, idx); return this; }
        /// <summary>
        /// Removes the element at the given valid index, shifting down the elements above this index to fill the gap. 
        /// </summary>
        public ILuaState Remove(int idx) { Lua.lua_remove(NativeState, idx); return this; }
        /// <summary>
        /// Moves the top element into the given valid index without shifting any element (therefore replacing the value at that given index), 
        /// and then pops the top element. 
        /// </summary>
        public ILuaState Replace(int idx) { Lua.lua_replace(NativeState, idx); return this; }
        #endregion

        #region compatibility macros for unsigned conversions
        /// <summary>
        /// Push an unsigned int
        /// </summary>
        public ILuaState PushUnsigned(UInt32 n) { Lua.lua_pushunsigned(NativeState, n); return this; }
        /// <summary>
        /// Converts the Lua value at the given index to an unsigned int
        /// </summary>
        public UInt32 ToUnsigned(int idx, out bool isnum)
        {
            int dummy;
            var res = Lua.lua_tounsignedx(NativeState, idx, out dummy);
            isnum = dummy != 0;
            return (uint)res;
        }
        /// <summary>
        /// Converts the Lua value at the given index to an unsigned int
        /// </summary>
        public UInt32 ToUnsigned(int idx) { return (uint)Lua.lua_tounsigned(NativeState, idx); }
        #endregion

        #region Debug API
        /// <summary>
        /// Gets information about the interpreter runtime stack. 
        /// </summary>
        public int GetStack(int level, LuaDebug ar) { throw new NotImplementedException(); }
        /// <summary>
        /// Gets information about a specific function or function invocation. 
        /// </summary>
        public int GetInfo(String what, LuaDebug ar) { throw new NotImplementedException(); }
        /// <summary>
        /// Gets information about a local variable of a given activation record or a given function. 
        /// </summary>
        public String GetLocal(LuaDebug ar, int n) { throw new NotImplementedException(); }
        /// <summary>
        /// Sets the value of a local variable of a given activation record. 
        /// </summary>
        public String SetLocal(LuaDebug ar, int n) { throw new NotImplementedException(); }
        /// <summary>
        /// Gets information about the n-th upvalue of the closure at index funcindex. 
        /// </summary>
        public String GetUpvalue(int funcindex, int n) { return Lua.lua_getupvalue(NativeState, funcindex, n); }
        /// <summary>
        /// Sets the value of a closure's upvalue. 
        /// </summary>
        public String SetUpvalue(int funcindex, int n) { return Lua.lua_setupvalue(NativeState, funcindex, n); }
        /// <summary>
        /// Returns a unique identifier for the upvalue numbered n from the closure at index funcindex. 
        /// </summary>
        public Int32 UpvalueId(int fidx, int n) { return Lua.lua_upvalueid(NativeState, fidx, n).ToInt32(); }
        /// <summary>
        /// Make the n1-th upvalue of the Lua closure at index funcindex1 refer to the n2-th upvalue of the Lua closure at index funcindex2. 
        /// </summary>
        public void UpvalueJoin(int fidx1, int n1, int fidx2, int n2) { Lua.lua_upvaluejoin(NativeState, fidx1, n1, fidx2, n2); }
        /// <summary>
        /// Sets the debugging hook function. 
        /// </summary>
        public void SetHook(LuaHook func, LuaHookMask mask, int count) { throw new NotImplementedException(); }
        /// <summary>
        /// Returns the current hook function. 
        /// </summary>
        public LuaHook GetHook() { throw new NotImplementedException(); }
        /// <summary>
        /// Returns the current hook mask. 
        /// </summary>
        public LuaHookMask GetHookMask() { return (LuaHookMask)Lua.lua_gethookmask(NativeState); }
        /// <summary>
        /// Returns the current hook count. 
        /// </summary>
        public int GetHookCount() { return Lua.lua_gethookcount(NativeState); }
        #endregion

        #region lauxlib

        /// <summary>
        /// Checks whether the core running the call, the core that created the Lua state, and the code making the call are all using the same version of Lua.
        /// </summary>
        public ILuaState CheckVersion() { Lua.luaL_checkversion(NativeState); return this; }
        /// <summary>
        /// Pushes onto the stack the field e from the metatable of the object at index obj and returns the type of pushed value.
        /// </summary>
        public LuaType GetMetaField(int obj, String e) { return (LuaType)Lua.luaL_getmetafield(NativeState, obj, e); }
        /// <summary>
        /// Calls a metamethod. 
        /// </summary>
        public Boolean CallMeta(int obj, String e) { return Lua.luaL_callmeta(NativeState, obj, e) != 0; }
        /// <summary>
        /// Raises an error reporting a problem with argument arg of the C function that called it
        /// </summary>
        public void ArgError(int arg, String extramsg) { Lua.luaL_argerror(NativeState, arg, extramsg); }
        /// <summary>
        /// Checks whether the function argument arg is a string and returns this string; if l is not null fills l with the string's length.
        /// </summary>
        public String CheckLString(int arg, out uint l) { return Lua.luaL_checklstring(NativeState, arg, out l); }
        /// <summary>
        /// If the function argument arg is a string, returns this string. If this argument is absent or is nil, returns d. Otherwise, raises an error. 
        /// </summary>
        public String OptLString(int arg, String def, out uint l) { return Lua.luaL_optlstring(NativeState, arg, def, out l); }
        /// <summary>
        /// Checks whether the function argument arg is a number and returns this number. 
        /// </summary>
        public Double CheckNumber(int arg) { return Lua.luaL_checknumber(NativeState, arg); }
        /// <summary>
        /// If the function argument arg is a number, returns this number. If this argument is absent or is nil, returns d. Otherwise, raises an error. 
        /// </summary>
        public Double OptNumber(int arg, Double def) { return Lua.luaL_optnumber(NativeState, arg, def); }
        /// <summary>
        /// Checks whether the function argument arg is an integer (or can be converted to an integer) and returns this integer cast to a Int32. 
        /// </summary>
        public Int32 CheckInteger(int arg) { return (int)Lua.luaL_checkinteger(NativeState, arg); }
        /// <summary>
        /// If the function argument arg is an integer (or convertible to an integer), returns this integer. If this argument is absent or is nil, returns d. Otherwise, raises an error. 
        /// </summary>
        public Int32 OptInteger(int arg, Int32 def) { return (int)Lua.luaL_optinteger(NativeState, arg, def); }
        /// <summary>
        /// Grows the stack size to top + sz elements, raising an error if the stack cannot grow to that size.
        /// </summary>
        public ILuaState CheckStack(int sz, String msg) { Lua.luaL_checkstack(NativeState, sz, msg); return this; }
        /// <summary>
        /// Checks whether the function argument arg has type t.
        /// </summary>
        public ILuaState CheckType(int arg, LuaType t) { Lua.luaL_checktype(NativeState, arg, (int)t); return this; }
        /// <summary>
        /// Checks whether the function has an argument of any type (including nil) at position arg. 
        /// </summary>
        public ILuaState CheckAny(int arg) { Lua.luaL_checkany(NativeState, arg); return this; }
        /// <summary>
        /// Create and register a new metatable
        /// </summary>
        /// <remarks>
        /// If the registry already has the key tname, returns false. Otherwise, creates a new table to be used as a metatable for userdata, adds 
        /// to this new table the pair __name = tname, adds to the registry the pair [tname] = new table, and returns true. 
        /// (The entry __name is used by some error-reporting functions.) 
        /// In both cases pushes onto the stack the final value associated with tname in the registry. 
        /// </remarks>
        public bool NewMetatable(String tname) { return Lua.luaL_newmetatable(NativeState, tname) != 0; }
        /// <summary>
        /// Sets the metatable of the object at the top of the stack as the metatable associated with name tname in the registry
        /// </summary>
        public ILuaState SetMetatable(String tname) { Lua.luaL_setmetatable(NativeState, tname); return this; }
        /// <summary>
        /// Checks whether the function argument arg is a userdata of the type tname and returns the userdata address. 
        /// </summary>
        public Object Checkudata(int arg, String tname) { return UserDataRef.GetData(Lua.luaL_checkudata(NativeState, arg, tname)); }
        /// <summary>
        /// This function works like CheckUData, except that, when the test fails, it returns null instead of raising an error. 
        /// </summary>
        public Object TestUData(int arg, String tname) { return UserDataRef.GetData(Lua.luaL_testudata(NativeState, arg, tname)); }
        /// <summary>
        /// Pushes onto the stack a string identifying the current position of the control at level lvl in the call stack
        /// </summary>
        public ILuaState Where(int lvl) { Lua.luaL_where(NativeState, lvl); return this; }
        /// <summary>
        /// Raises an error.
        /// </summary>
        public void Error(String message) { Lua.luaL_error(NativeState, message, __arglist()); }
        /// <summary>
        /// Raises an error.
        /// </summary>
        public void Error(String fmt, String arg0) { Lua.luaL_error(NativeState, fmt, __arglist(arg0)); }
        /// <summary>
        /// Raises an error.
        /// </summary>
        public void Error(String fmt, String arg0, String arg1) { Lua.luaL_error(NativeState, fmt, __arglist(arg0, arg1)); }
        /// <summary>
        /// Loads a file as a Lua chunk. 
        /// </summary>
        public LuaStatus LoadFile(String filename, String mode) { return (LuaStatus)Lua.luaL_loadfilex(NativeState, filename, mode); }
        /// <summary>
        /// Loads a file as a Lua chunk. 
        /// </summary>
        public LuaStatus LoadFile(String filename) { return (LuaStatus)Lua.luaL_loadfile(NativeState, filename); }
        /// <summary>
        /// Loads a string as a Lua chunk. 
        /// </summary>
        public LuaStatus LoadString(String s) { return (LuaStatus)Lua.luaL_loadstring(NativeState, s); }
        /// <summary>
        /// Returns the "length" of the value at the given index as a number; it is equivalent to the '#' operator in Lua 
        /// </summary>
        public Int32 LLen(int idx) { return (int)Lua.luaL_len(NativeState, idx); }
        /// <summary>
        /// Creates a copy of string s by replacing any occurrence of the string p with the string r. 
        /// Pushes the resulting string on the stack and returns it. 
        /// </summary>
        public String GSub(String s, String p, String r) { return Lua.luaL_gsub(NativeState, s, p, r); }
        /// <summary>
        /// Registers all functions in the array l (see luaL_Reg) into the table on the top of the stack
        /// </summary>
        public ILuaState SetFuncs(IEnumerable<Tuple<String, LuaFunction>> l, int nup)
        {
            Lua.luaL_Reg[] regs = l.Select(t => new Lua.luaL_Reg
            {
                func = t.Item2.ToCFunction(),
                name = t.Item1
            }).ToArray();
            Lua.luaL_setfuncs(NativeState, regs, nup);
            return this;
        }
        /// <summary>
        /// Ensures that the value t[fname], where t is the value at index idx, is a table, and pushes that table onto the stack. 
        /// Returns true if it finds a previous table there and false if it creates a new table. 
        /// </summary>
        public bool GetSubTable(int idx, String fname) { return Lua.luaL_getsubtable(NativeState, idx, fname) != 0; }
        /// <summary>
        /// Creates and pushes a traceback of the stack L1. 
        /// </summary>
        /// <remarks>
        /// If msg is not NULL it is appended at the beginning of the traceback. The level parameter tells at which level to start the traceback. 
        /// </remarks>
        public ILuaState Traceback(ILuaState L1, String msg, int level)
        {
            var ls1 = L1 as LuaState;
            Lua.luaL_traceback(NativeState, ls1 != null ? ls1.NativeState : IntPtr.Zero, msg, level);
            return this;
        }
        #region some useful macros
        /// <summary>
        /// Creates a new table with a size optimized to store all entries in the array l
        /// </summary>
        public ILuaState NewLibTable(Tuple<String, LuaFunction>[] l)
        {
            Lua.luaL_newlibtable(NativeState, l.Select(t => new Lua.luaL_Reg
            {
                func = t.Item2.ToCFunction(),
                name = t.Item1
            }).ToArray());
            return this;
        }
        /// <summary>
        /// Creates a new table and registers there the functions in list l. 
        /// </summary>
        public ILuaState NewLib(Tuple<String, LuaFunction>[] l)
        {
            Lua.luaL_newlib(NativeState, l.Select(t => new Lua.luaL_Reg
            {
                func = t.Item2.ToCFunction(),
                name = t.Item1
            }).ToArray());
            return this;
        }
        /// <summary>
        /// Checks whether cond is true. If it is not, raises an error with a standard message 
        /// </summary>
        public ILuaState ArgCheck(bool cond, int arg, String extramsg)
        {
            Lua.luaL_argcheck(NativeState, cond, arg, extramsg);
            return this;
        }
        /// <summary>
        /// Returns the name of the type of the value at the given index. 
        /// </summary>
        public String TypeName(int idx) { return Lua.luaL_typename(NativeState, idx); }
        /// <summary>
        /// Loads and runs the given file.
        /// </summary>
        public LuaStatus DoFile(String fn) { return (LuaStatus)Lua.luaL_dofile(NativeState, fn); }
        /// <summary>
        /// Loads and runs the given string.
        /// </summary>
        public LuaStatus DoString(String s) { return (LuaStatus)Lua.luaL_dostring(NativeState, s); }
        /// <summary>
        /// Loads a buffer as a Lua chunk. 
        /// </summary>
        public LuaStatus LoadBuffer(String s, int sz, String n) { return (LuaStatus)Lua.luaL_loadbuffer(NativeState, s, sz, n); }
        #endregion

        #region Acces to the "Abstraction Layer" for basic report of messages and errors

        /// <summary>
        /// Process write
        /// </summary>
        void ProcessWrite(String s, EventHandler<WriteEventArgs> h, Action write)
        {
            WriteEventArgs e = new WriteEventArgs(s);
            if (h != null) h(null, e);
            if (!e.Handled)
                write();
        }

        /// <summary>
        /// print a string
        /// </summary>
        public ILuaState WriteString(String s)
        {
            ProcessWrite(s, OnWriteString, () => Lua.lua_writestring(s));
            return this;
        }

        /// <summary>
        /// print a newline and flush the output
        /// </summary>
        public ILuaState WriteLine()
        {
            ProcessWrite(Environment.NewLine, OnWriteLine, () => Lua.lua_writeline());
            return this;
        }

        /// <summary>
        /// print an error message
        /// </summary>
        public ILuaState WriteStringError(String s, String p)
        {
            ProcessWrite(String.Format(s.Replace("%s", "{0}"), p), OnWriteStringError, () => Lua.lua_writestringerror(s, p));
            return this;
        }

        #endregion

        #endregion

        #region lualib

        /// <summary>
        /// Overrided 'print' function
        /// </summary>
        private int LuaPrint(ILuaState state)
        {
            // Get the event
            //LuaState L = state as LuaState;
            //EventHandler<WriteEventArgs> h = L != null ? L.OnPrint : null;
            EventHandler<WriteEventArgs> h = this.OnPrint;

            // Number of arguments
            int n = state.GetTop();
            // Get the 'tostring' function
            state.GetGlobal("tostring");
            // Loop on each argument
            StringBuilder line = new StringBuilder();
            for (int i = 1; i <= n; i++)
            {
                // Function to be called
                state.PushValue(-1);
                // Value to print
                state.PushValue(i);
                // Convert to string
                state.Call(1, 1);
                // Get the result
                var s = state.ToString(-1);
                if (s == null)
                    state.Error("'tostring' must return a string to 'print'");

                // Build the line
                if (i > 1) line.Append('\t');
                line.Append(s);

                // Pop result
                state.Pop(1);
            }
            // Call the event
            WriteEventArgs pe = new WriteEventArgs(line.ToString()) { Handled = false };
            //if (h != null) h(L, pe);
            if (h != null) h(this, pe);
            // If the event is not handled, we print with the default behavior
            if (!pe.Handled)
            {
                WriteString(pe.Text);
                WriteLine();
            }
            // No result
            return 0;
        }

        void OverridePrint()
        {
            PushFunction(LuaPrint);
            SetGlobal("print");
        }

        public int OpenBase() { var res = Lua.luaopen_base(NativeState); OverridePrint(); return res; }
        public int OpenCoroutine() { return Lua.luaopen_coroutine(NativeState); }
        public int OpenTable() { return Lua.luaopen_table(NativeState); }
        public int OpenIo() { return Lua.luaopen_io(NativeState); }
        public int OpenOs() { return Lua.luaopen_os(NativeState); }
        public int OpenString() { return Lua.luaopen_string(NativeState); }
        public int OpenUtf8() { return Lua.luaopen_utf8(NativeState); }
        public int OpenBit32() { return Lua.luaopen_bit32(NativeState); }
        public int OpenMath() { return Lua.luaopen_math(NativeState); }
        public int OpenDebug() { return Lua.luaopen_debug(NativeState); }
        public int OpenPackage() { return Lua.luaopen_package(NativeState); }
        public ILuaState OpenLibs() { Lua.luaL_openlibs(NativeState); OverridePrint(); return this; }

        #endregion

        /// <summary>
        /// Assert
        /// </summary>
        public void Assert(bool cond) { Lua.lua_assert(cond); }

        /// <summary>
        /// Event raised when "print" is called
        /// </summary>
        public event EventHandler<WriteEventArgs> OnPrint;

        /// <summary>
        /// Event raised when lua_writestring is called
        /// </summary>
        public event EventHandler<WriteEventArgs> OnWriteString;

        /// <summary>
        /// Event raised when lua_writeline is called
        /// </summary>
        public event EventHandler<WriteEventArgs> OnWriteLine;

        /// <summary>
        /// Event raised when lua_writestringerror is called
        /// </summary>
        public event EventHandler<WriteEventArgs> OnWriteStringError;

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
