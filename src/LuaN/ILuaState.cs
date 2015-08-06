using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Lua state Interface
    /// </summary>
    public interface ILuaState : IDisposable
    {
        #region Lua engine informations

        /// <summary>
        /// Engine provider
        /// </summary>
        ILuaEngine Engine { get; }

        /// <summary>
        /// Option for multiple returns in 'PCall' and 'Call' 
        /// </summary>
        int MultiReturns { get; }

        /// <summary>
        /// First pseudo index
        /// </summary>
        int FirstPseudoIndex { get; }

        /// <summary>
        /// Index of the registry
        /// </summary>
        int RegistryIndex { get; }

        /// <summary>
        /// Minumum stack size
        /// </summary>
        int MinStack { get; }

        #endregion

        #region State management

        /// <summary>
        /// Get the version number used to create this state
        /// </summary>
        Double LuaVersion();

        /// <summary>
        /// Define the panic function
        /// </summary>
        LuaCFunction LuaAtPanic(LuaCFunction panicf);

        /// <summary>
        /// Creates a new thread, pushes it on the stack.
        /// </summary>
        ILuaState LuaNewThread();

        #endregion

        #region Stack management

        #region basic stack manipulation
        /// <summary>
        /// Get the absolute stack index
        /// </summary>
        int LuaAbsIndex(int idx);
        /// <summary>
        /// Get the top of the stack
        /// </summary>
        int LuaGetTop();
        /// <summary>
        /// Set the top of the stack
        /// </summary>
        void LuaSetTop(int idx);
        /// <summary>
        /// Push a value on the stack
        /// </summary>
        void LuaPushValue(int idx);
        /// <summary>
        /// Rotates the n stack elements between the valid index idx and the top of the stack
        /// </summary>
        void LuaRotate(int idx, int n);
        /// <summary>
        /// Copies the element at index fromidx into the valid index toidx, replacing the value at that position
        /// </summary>
        void LuaCopy(int fromidx, int toidx);
        /// <summary>
        /// Ensures that the stack has space for at least n extra slots
        /// </summary>
        bool LuaCheckStack(int n);
        /// <summary>
        /// Exchange values between different threads of the same state
        /// </summary>
        void LuaXMove(ILuaState to, int n);
        #endregion

        #region access functions (stack -> C)
        /// <summary>
        /// Returns true if the value at the given index is a number or a string convertible to a number, and false otherwise.
        /// </summary>
        Boolean LuaIsNumber(int idx);
        /// <summary>
        /// Returns true if the value at the given index is a string or a number (which is always convertible to a string), and false otherwise.
        /// </summary>
        Boolean LuaIsString(int idx);
        /// <summary>
        /// Returns true if the value at the given index is a C function, and false otherwise.
        /// </summary>
        Boolean LuaIsCFunction(int idx);
        /// <summary>
        /// Returns true if the value at the given index is an integer (that is, the value is a number and is represented as an integer), and false otherwise.
        /// </summary>
        Boolean LuaIsInteger(int idx);
        /// <summary>
        /// Returns true if the value at the given index is a userdata (either full or light), and false otherwise.
        /// </summary>
        Boolean LuaIsUserData(int idx);
        /// <summary>
        /// Returns the type of the value in the given valid index, or LUA_TNONE for a non-valid (but acceptable) index
        /// </summary>
        LuaType LuaType(int idx);
        /// <summary>
        /// Returns the name of the type encoded by the value tp, which must be one the values returned by Type.
        /// </summary>
        String LuaTypeName(LuaType tp);

        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Number
        /// </summary>
        Double LuaToNumberX(int idx, out bool isnum);
        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Integer
        /// </summary>
        Int64 LuaToIntegerX(int idx, out bool isnum);
        /// <summary>
        /// Converts the Lua value at the given index to a C boolean value
        /// </summary>
        Boolean LuaToBoolean(int idx);
        /// <summary>
        /// Converts the Lua value at the given index to a string.
        /// </summary>
        String LuaToString(int idx);
        /// <summary>
        /// Returns the raw "length" of the value at the given index
        /// </summary>
        /// <remarks>
        /// Returns the raw "length" of the value at the given index: for strings, this is the string length; 
        /// for tables, this is the result of the length operator ('#') with no metamethods; for userdata, this is the size 
        /// of the block of memory allocated for the userdata; for other values, it is 0.
        /// </remarks>
        UInt32 LuaRawLen(int idx);
        /// <summary>
        /// Converts a value at the given index to a C function
        /// </summary>
        LuaCFunction LuaToCFunction(int idx);
        /// <summary>
        /// If the value at the given index is a full userdata, returns its block address. If the value is a light userdata, returns its pointer. Otherwise, returns NULL.
        /// </summary>
        Object LuaToUserData(int idx);
        /// <summary>
        /// Converts the value at the given index to a Lua thread
        /// </summary>
        ILuaState LuaToThread(int idx);
        #endregion

        #region push functions (C -> stack)
        /// <summary>
        /// Push a nil value
        /// </summary>
        void LuaPushNil();
        /// <summary>
        /// Push a number value
        /// </summary>
        void LuaPushNumber(Double n);
        /// <summary>
        /// Push a integer value
        /// </summary>
        void LuaPushInteger(Int64 n);
        /// <summary>
        /// Push a String value
        /// </summary>
        void LuaPushString(String s);
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
        void LuaPushCClosure(LuaCFunction fn, int n);
        /// <summary>
        /// Push a boolean value
        /// </summary>
        void LuaPushBoolean(Boolean b);
        /// <summary>
        /// Push a light user data
        /// </summary>
        void LuaPushLightUserData(Object userData);
        /// <summary>
        /// Push a thread
        /// </summary>
        /// <returns>True if the thread is the main thread of its state.</returns>
        bool LuaPushThread();
        #endregion

        #region get functions (Lua -> stack)
        /// <summary>
        /// Pushes onto the stack the value of the global name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaGetGlobal(String name);
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given index and k is the value at the top of the stack.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaGetTable(int idx);
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="k"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaGetField(int idx, String k);
        /// <summary>
        /// Pushes onto the stack the value t[i], where t is the value at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaGetI(int idx, int n);
        /// <summary>
        /// Similar to GetTable, but does a raw access
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaRawGet(int idx);
        /// <summary>
        /// Pushes onto the stack the value t[n], where t is the table at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="n"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaRawGetI(int idx, int n);
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the table at the given index and k is the pointer p represented as a light userdata.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="p"></param>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaRawGetP(int idx, Object p);

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// </summary>
        void LuaCreateTable(int narr, int nrec);
        /// <summary>
        /// This function allocates a new block of memory with the given size, pushes onto the stack a new full userdata with the block address.
        /// </summary>
        ILuaUserData LuaNewUserData(UInt32 sz);
        /// <summary>
        /// If the value at the given index has a metatable, the function pushes that metatable onto the stack and returns true. Otherwise, the function returns false and pushes nothing on the stack.
        /// </summary>
        bool LuaGetMetatable(int objindex);
        /// <summary>
        /// Pushes onto the stack the Lua value associated with the userdata at the given index.
        /// </summary>
        /// <returns>Returns the type of the pushed value.</returns>
        LuaType LuaGetUserValue(int idx);
        #endregion

        #region set functions (stack -> Lua)
        /// <summary>
        /// Pops a value from the stack and sets it as the new value of global name.
        /// </summary>
        void LuaSetGlobal(String name);
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given index, v is the value at the top of the stack, and k is the value just below the top.
        /// </summary>
        /// <remarks>
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        void LuaSetTable(int idx);
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        void LuaSetField(int idx, String k);
        /// <summary>
        /// Does the equivalent to t[n] = v, where t is the value at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </remarks>
        void LuaSetI(int idx, int n);
        /// <summary>
        /// Similar to SetTable, but does a raw assignment (i.e., without metamethods).
        /// </summary>
        void LuaRawSet(int idx);
        /// <summary>
        /// Does the equivalent of t[i] = v, where t is the table at the given index and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods.
        /// </remarks>
        void LuaRawSetI(int idx, int n);
        /// <summary>
        /// Does the equivalent of t[p] = v, where t is the table at the given index, p is encoded as a light userdata, and v is the value at the top of the stack.
        /// </summary>
        /// <remarks>
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods.
        /// </remarks>
        void LuaRawSetP(int idx, Object p);

        /// <summary>
        /// Pops a table from the stack and sets it as the new metatable for the value at the given index.
        /// </summary>
        void LuaSetMetatable(int objindex);
        /// <summary>
        /// Pops a value from the stack and sets it as the new value associated to the userdata at the given index.
        /// </summary>
        void LuaSetUserValue(int idx);
        #endregion

        #endregion

        #region Comparison and arithmetic functions
        /// <summary>
        /// Arithmetic operation
        /// </summary>
        void LuaArith(LuaArithOperator op);
        /// <summary>
        /// Do raw equality
        /// </summary>
        bool LuaRawEqual(int idx1, int idx2);
        /// <summary>
        /// Compare two values
        /// </summary>
        bool LuaCompare(int idx1, int idx2, LuaRelOperator op);
        #endregion

        #region 'load' and 'call' functions (load and run Lua code)
        /// <summary>
        /// Calls a function.
        /// </summary>
        void LuaCall(int nargs, int nresults);
        /// <summary>
        /// Call with yield support
        /// </summary>
        void LuaCallK(int nargs, int nresults, int ctx, LuaKFunction k);
        /// <summary>
        /// Calls a function in protected mode.
        /// </summary>
        LuaStatus LuaPCall(int nargs, int nresults, int errfunc);
        /// <summary>
        /// Call in protected mode with yield support
        /// </summary>
        LuaStatus LuaPCallK(int nargs, int nresults, int errfunc, int ctx, LuaKFunction k);
        /// <summary>
        /// Loads a Lua chunk without running it.
        /// </summary>
        LuaStatus LuaLoad(LuaReader reader, Object dt, String chunkname, String mode);
        /// <summary>
        /// Dumps a function as a binary chunk.
        /// </summary>
        int LuaDump(LuaWriter writer, Object data, int strip);
        #endregion

        //#region coroutine functions
        ///// <summary>
        ///// Yields a coroutine (thread). 
        ///// </summary>
        //LuaStatus YieldK(int nresults, int ctx, LuaKFunction k);
        ///// <summary>
        ///// Starts and resumes a coroutine in the given thread
        ///// </summary>
        //LuaStatus Resume(ILuaState from, int narg);
        ///// <summary>
        ///// Returns the status of the thread
        ///// </summary>
        //LuaStatus Status();
        ///// <summary>
        ///// Returns true if the given coroutine can yield, and false otherwise. 
        ///// </summary>
        //Boolean IsYieldable();
        ///// <summary>
        ///// This function is equivalent to YieldK, but it has no continuation.
        ///// </summary>
        //LuaStatus Yield(int nresults);
        //#endregion

        //#region garbage-collection function and options
        ///// <summary>
        ///// Controls the garbage collector.
        ///// </summary>
        //int GC(LuaGcFunction what, int data);
        //#endregion

        #region miscellaneous functions
        /// <summary>
        /// Generates a Lua error, using the value at the top of the stack as the error object.
        /// </summary>
        void LuaError();
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
        Double LuaToNumber(int idx);
        /// <summary>
        /// Converts the Lua value at the given index to the C type integer
        /// </summary>
        Int64 LuaToInteger(int idx);
        /// <summary>
        /// Pops n elements from the stack. 
        /// </summary>
        void LuaPop(int n);
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
        bool LuaIsNil(int n);
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

        ///// <summary>
        ///// Assert
        ///// </summary>
        //void Assert(bool cond);

        ///// <summary>
        ///// Event raised when "print" is called
        ///// </summary>
        //event EventHandler<WriteEventArgs> OnPrint;

        ///// <summary>
        ///// Event raised when lua_writestring is called
        ///// </summary>
        //event EventHandler<WriteEventArgs> OnWriteString;

        ///// <summary>
        ///// Event raised when lua_writeline is called
        ///// </summary>
        //event EventHandler<WriteEventArgs> OnWriteLine;

        ///// <summary>
        ///// Event raised when lua_writestringerror is called
        ///// </summary>
        //event EventHandler<WriteEventArgs> OnWriteStringError;

    }
}
