using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaNet
{
    /// <summary>
    /// Lua type of value
    /// </summary>
    public enum LuaType : int
    {
        /// <summary>
        /// None
        /// </summary>
        None = (-1),
        /// <summary>
        /// Nil
        /// </summary>
        Nil = 0,
        /// <summary>
        /// Boolean
        /// </summary>
        Boolean = 1,
        /// <summary>
        /// Light user data
        /// </summary>
        LightUserData = 2,
        /// <summary>
        /// Number
        /// </summary>
        Number = 3,
        /// <summary>
        /// String
        /// </summary>
        String = 4,
        /// <summary>
        /// Table
        /// </summary>
        Table = 5,
        /// <summary>
        /// Function
        /// </summary>
        Function = 6,
        /// <summary>
        /// User data
        /// </summary>
        UserData = 7,
        /// <summary>
        /// Thread
        /// </summary>
        Thread = 8
    }

    /// <summary>
    /// Lua Thread Status
    /// </summary>
    public enum LuaStatus
    {
        OK = 0,
        Yield = 1,
        ErrRun = 2,
        ErrSyntax = 3,
        ErrMem = 4,
        ErrGcMm = 5,
        ErrErr = 6,
        ErrFile = ErrErr + 1
    }

    /// <summary>
    /// Arithmetic operators
    /// </summary>
    public enum LuaArithOperator
    {
        Add = 0,	/* ORDER TM, ORDER OP */
        Sub = 1,
        Mul = 2,
        Mod = 3,
        Pow = 4,
        Div = 5,
        IDiv = 6,
        BAnd = 7,
        BOr = 8,
        BXor = 9,
        Shl = 10,
        Shr = 11,
        Unm = 12,
        BNot = 13
    }

    /// <summary>
    /// Relationnal operators
    /// </summary>
    public enum LuaRelOperator
    {
        EQ = 0,
        LT = 1,
        LE = 2
    }

    /// <summary>
    /// GC function
    /// </summary>
    public enum LuaGcFunction
    {
        Stop = 0,
        Restart = 1,
        Collect = 2,
        Count = 3,
        Countb = 4,
        Step = 5,
        SetPause = 6,
        SetStepMul = 7,
        IsRunning = 9
    }

    /// <summary>
    /// Event codes
    /// </summary>
    public enum LuaHookEvent{
        HookCall = 0,
        HookRet = 1,
        HookLine = 2,
        HookCount = 3,
        HookTailCall = 4
    }

    /// <summary>
    /// Event masks
    /// </summary>
    [Flags]
    public enum LuaHookMask
    {
        None = 0,
        MaskCall = (1 << LuaHookEvent.HookCall),
        MaskRet = (1 << LuaHookEvent.HookRet),
        MaskLine = (1 << LuaHookEvent.HookLine),
        MaskCount = (1 << LuaHookEvent.HookCount),
        All = MaskCall | MaskRet | MaskLine | MaskCount
    }

}
