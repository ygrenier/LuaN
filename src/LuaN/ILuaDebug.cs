using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Interface to the Lua debug informations
    /// </summary>
    public interface ILuaDebug : IDisposable
    {
        /// <summary>
        /// Event 
        /// </summary>
        LuaHookEvent Event { get; }

        /// <summary>
        /// Name (n)
        /// </summary>
        String Name { get; }

        /// <summary>
        /// 'global', 'local', 'field', 'method' (n)
        /// </summary>
        String NameWhat { get; }

        /// <summary>
        /// 'Lua', 'C', 'main', 'tail' (S)
        /// </summary>
        String What { get; }

        /// <summary>
        /// Source (S)
        /// </summary>
        String Source { get; }

        /// <summary>
        /// Current line (l)
        /// </summary>
        int CurrentLine { get; }

        /// <summary>
        /// Line where of definition (S)
        /// </summary>
        int LineDefined { get; }

        /// <summary>
        /// Last line of definition (S)
        /// </summary>
        int LastLineDefined { get; }

        /// <summary>
        /// number of upvalues (u)
        /// </summary>
        byte NUps { get; }

        /// <summary>
        /// number of parameters (u)
        /// </summary>
        byte NParams { get; }

        /// <summary>
        /// (u)
        /// </summary>
        bool IsVarArg { get; }

        /// <summary>
        /// (t)
        /// </summary>
        bool IsTailCall { get; }

        /// <summary>
        /// Short source (S)
        /// </summary>
        string ShortSource { get; }

    }

}
