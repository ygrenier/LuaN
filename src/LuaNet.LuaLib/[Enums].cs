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
        Ok = 0,
        Yield = 1,
        ErrorRun = 2,
        ErrorSyntax = 3,
        ErrorMemory = 4,
        ErrorGC = 5,
        ErrorError = 6,
        ErrorFile = ErrorError + 1
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

    /// <summary>
    /// What information to extract from GetInfo()
    /// </summary>
    [Flags]
    public enum LuaGetInfoWhat
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// '>' : GetInfo from the function pushed on the top of the stack, instead of the current function invoked
        /// </summary>
        FromTopOfStack = 1,
        /// <summary>
        /// 'n' : fills in the field name and namewhat
        /// </summary>
        Name = 2,
        /// <summary>
        /// 'S':  fills in the fields source, short_src, linedefined, lastlinedefined, and what
        /// </summary>
        Source = 4,
        /// <summary>
        /// 'l':  fills in the field currentline 
        /// </summary>
        CurrentLine = 8,
        /// <summary>
        /// 't':  fills in the field istailcall
        /// </summary>
        IsTailCall = 16,
        /// <summary>
        /// 'u':  fills in the fields nups, nparams, and isvararg
        /// </summary>
        ParamsUps = 32,
        /// <summary>
        /// 'f':  pushes onto the stack the function that is running at the given level
        /// </summary>
        PushFunction = 64,
        /// <summary>
        /// 'L':  pushes onto the stack a table whose indices are the numbers of the lines that are valid on the function. 
        /// (A valid line is a line with some associated code, that is, a line where you can put a break point. Non-valid lines include 
        /// empty lines and comments.) 
        /// </summary>
        PushLines = 128,
        /// <summary>
        /// All fills : Name | Source | CurrentLine | IsTailCall | ParamsUps
        /// </summary>
        AllFills = Name | Source | CurrentLine | IsTailCall | ParamsUps
    }

}
