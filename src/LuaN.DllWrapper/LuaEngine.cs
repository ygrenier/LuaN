using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.DllWrapper
{

    /// <summary>
    /// Lua Engine
    /// </summary>
    public class LuaEngine : ILuaEngine
    {

        #region Lua engine informations

        /// <summary>
        /// Major Version part
        /// </summary>
        public String LuaVersionMajor { get { return LuaDll.LUA_VERSION_MAJOR; } }
        /// <summary>
        /// Minor Version part
        /// </summary>
        public String LuaVersionMinor { get { return LuaDll.LUA_VERSION_MINOR; } }
        /// <summary>
        /// Release Version part
        /// </summary>
        public String LuaVersionRelease { get { return LuaDll.LUA_VERSION_RELEASE; } }
        /// <summary>
        /// Version number
        /// </summary>
        public Double LuaVersionNum { get { return LuaDll.LUA_VERSION_NUM; } }

        /// <summary>
        /// Lua Version
        /// </summary>
        public String LuaVersion { get { return LuaDll.LUA_VERSION; } }
        /// <summary>
        /// Lua Release
        /// </summary>
        public String LuaRelease { get { return LuaDll.LUA_RELEASE; } }
        /// <summary>
        /// Lua Copyright
        /// </summary>
        public String LuaCopyright { get { return LuaDll.LUA_COPYRIGHT; } }
        /// <summary>
        /// Lua Authors
        /// </summary>
        public String LuaAuthors { get { return LuaDll.LUA_AUTHORS; } }

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
        /// Minimum stack size
        /// </summary>
        public int MinStack { get { return LuaDll.LUA_MINSTACK; } }

        /// <summary>
        /// Name of the engine
        /// </summary>
        public String EngineName { get { return LuaDll.LuaDllName; } }

        #endregion


        #region State management

        /// <summary>
        /// Open a new state
        /// </summary>
        public ILuaState NewState()
        {
            return new LuaState(this, IntPtr.Zero, true);
        }

        #endregion

        /// <summary>
        /// Current engine
        /// </summary>
        public static LuaEngine Current { get { return _Current ?? (_Current = new LuaEngine()); } }
        static LuaEngine _Current;

    }
}
