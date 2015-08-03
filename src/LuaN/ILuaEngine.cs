using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Interface for Lua Engine
    /// </summary>
    public interface ILuaEngine
    {

        #region Lua engine informations
        
        /// <summary>
        /// Major Version part
        /// </summary>
        String LuaVersionMajor { get; }

        /// <summary>
        /// Minor Version part
        /// </summary>
        String LuaVersionMinor { get; }

        /// <summary>
        /// Release Version part
        /// </summary>
        String LuaVersionRelease { get; }

        /// <summary>
        /// Version number
        /// </summary>
        Double LuaVersionNum { get; }

        /// <summary>
        /// Lua Version
        /// </summary>
        String LuaVersion { get; }

        /// <summary>
        /// Lua Release
        /// </summary>
        String LuaRelease { get; }

        /// <summary>
        /// Lua Copyright
        /// </summary>
        String LuaCopyright { get; }

        /// <summary>
        /// Lua Authors
        /// </summary>
        String LuaAuthors { get; }

        /// <summary>
        /// Name of the engine
        /// </summary>
        String EngineName { get; }

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
        /// Open a new state
        /// </summary>
        ILuaState NewState();

        #endregion

    }

}
