﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaNet.LuaLib
{
    /// <summary>
    /// Proxy for the lua_Debug pointer
    /// </summary>
    class LuaDebugProxy : ILuaDebug, IDisposable
    {
        Lua.lua_Debug _Debug;
        bool _OwnPointer = false;

        /// <summary>
        /// New default proxy
        /// </summary>
        internal LuaDebugProxy() : this(IntPtr.Zero, LuaGetInfoWhat.None) { }

        /// <summary>
        /// New proxy from a pointer
        /// </summary>
        /// <param name="pointer"></param>
        internal LuaDebugProxy(IntPtr pointer, LuaGetInfoWhat dataFilled)
        {
            DataFilled = LuaGetInfoWhat.None;
            _NativePointer = pointer;
            _Debug = new Lua.lua_Debug();
            NativeContentUpdated(dataFilled);
        }

        ~LuaDebugProxy()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (_NativePointer != IntPtr.Zero && _OwnPointer)
            {
                Marshal.FreeHGlobal(_NativePointer);
                _OwnPointer = false;
            }
            _NativePointer = IntPtr.Zero;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Refresh the debug info from the pointer
        /// </summary>
        internal void NativeContentUpdated(LuaGetInfoWhat dataFilled)
        {
            if (dataFilled == LuaGetInfoWhat.None)
                DataFilled = LuaGetInfoWhat.None;
            else
                DataFilled = DataFilled | (dataFilled & LuaGetInfoWhat.AllFills);
            if (_NativePointer != IntPtr.Zero)
            {
                Marshal.PtrToStructure(_NativePointer, _Debug);
            }
        }

        String ReadStringField(LuaGetInfoWhat what, IntPtr ptr)
        {
            if ((DataFilled & what) == 0) return null;
            if (ptr == IntPtr.Zero) return null;
            try
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            catch { return null; }
        }

        /// <summary>
        /// Event 
        /// </summary>
        public LuaHookEvent Event { get { return (LuaHookEvent)_Debug.evnt; } }

        /// <summary>
        /// Name (n)
        /// </summary>
        public String Name { get { return ReadStringField(LuaGetInfoWhat.Name, _Debug.name); } }

        /// <summary>
        /// 'global', 'local', 'field', 'method' (n)
        /// </summary>
        public String NameWhat { get { return ReadStringField(LuaGetInfoWhat.Name, _Debug.namewhat); } }

        /// <summary>
        /// 'Lua', 'C', 'main', 'tail' (S)
        /// </summary>
        public String What { get { return ReadStringField(LuaGetInfoWhat.Source, _Debug.what); } }

        /// <summary>
        /// Source (S)
        /// </summary>
        public String Source { get { return ReadStringField(LuaGetInfoWhat.Source, _Debug.source); } }

        /// <summary>
        /// Current line (l)
        /// </summary>
        public int CurrentLine { get { return (DataFilled & LuaGetInfoWhat.CurrentLine) != 0 ? _Debug.currentline : -1; } }

        /// <summary>
        /// Line where of definition (S)
        /// </summary>
        public int LineDefined { get { return (DataFilled & LuaGetInfoWhat.Source) != 0 ? _Debug.linedefined : -1; } }

        /// <summary>
        /// Last line of definition (S)
        /// </summary>
        public int LastLineDefined { get { return (DataFilled & LuaGetInfoWhat.Source) != 0 ? _Debug.lastlinedefined : -1; } }

        /// <summary>
        /// number of upvalues (u)
        /// </summary>
        public byte NUps { get { return (DataFilled & LuaGetInfoWhat.ParamsUps) != 0 ? _Debug.nups : (byte)0; } }

        /// <summary>
        /// number of parameters (u)
        /// </summary>
        public byte NParams { get { return (DataFilled & LuaGetInfoWhat.ParamsUps) != 0 ? _Debug.nparams : (byte)0; } }

        /// <summary>
        /// (u)
        /// </summary>
        public bool IsVarArg { get { return (DataFilled & LuaGetInfoWhat.ParamsUps) != 0 ? _Debug.isvararg != 0 : false; } }

        /// <summary>
        /// (t)
        /// </summary>
        public bool IsTailCall { get { return (DataFilled & LuaGetInfoWhat.IsTailCall) != 0 ? _Debug.istailcall != 0 : false; } }

        /// <summary>
        /// Short source (S)
        /// </summary>
        public string ShortSource { get { return (DataFilled & LuaGetInfoWhat.Source) != 0 ? _Debug.short_src : null; } }

        /// <summary>
        /// Native debug pointer
        /// </summary>
        public IntPtr NativePointer
        {
            get
            {
                if (_NativePointer == IntPtr.Zero)
                {
                    _NativePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Lua.lua_Debug)));
                    _OwnPointer = true;
                }
                return _NativePointer;
            }
        }
        private IntPtr _NativePointer;

        /// <summary>
        /// Indicate the data filled
        /// </summary>
        public LuaGetInfoWhat DataFilled { get; private set; }
    }

}
