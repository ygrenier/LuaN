using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Default .Net behavior and utilities for Lua
    /// </summary>
    public static class LuaDotnetHelper
    {

        /// <summary>
        /// Throw an error from the stack and restore it
        /// </summary>
        public static void ThrowError(ILuaState L, int restoreTop)
        {
            Object err = L.ToValue(-1);

            L.LuaSetTop(restoreTop);

            if (err is LuaException)
                throw (LuaException)err;

            if (err == null)
                err = "Unknown Lua error.";

            throw new LuaException(err.ToString());
        }

        /// <summary>
        /// Extract some values from the stack to .Net objects
        /// </summary>
        /// <param name="L">State</param>
        /// <param name="fromIdx">First index to extract (included)</param>
        /// <param name="toIdx">Last index to extract (included)</param>
        /// <param name="typedResult">Optional list of types to convert.</param>
        /// <returns>Result of extraction</returns>
        public static Object[] ExtractValues(ILuaState L, int fromIdx, int toIdx, Type[] typedResult)
        {
            // Convert the result
            List<Object> result = new List<object>();
            if (toIdx != fromIdx)
            {
                int tPos = typedResult != null ? 0 : -1;
                for (int i = fromIdx; i <= toIdx; i++)
                {
                    Object val = L.ToValue(i);
                    if (typedResult != null)
                    {
                        // If we reach the end of the typedResult we stop the conversion
                        if (tPos >= typedResult.Length) break;
                        // Convert
                        Type tpConv = typedResult[tPos++];
                        if (tpConv != null)
                        {
                            try
                            {
                                val = Convert.ChangeType(val, tpConv, System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch { val = tpConv.IsValueType && Nullable.GetUnderlyingType(tpConv) == null ? Activator.CreateInstance(tpConv) : null; }
                        }
                    }
                    result.Add(val);
                }
                if (typedResult != null)
                {
                    while (tPos < typedResult.Length)
                    {
                        Type tpConv = typedResult[tPos++];
                        result.Add(
                            tpConv != null && tpConv.IsValueType && Nullable.GetUnderlyingType(tpConv) == null
                            ? Activator.CreateInstance(tpConv)
                            : null
                            );
                    }
                }
            }
            // Returns the result
            return result.ToArray();
        }

        /// <summary>
        /// Call the value pushed on the stack
        /// </summary>
        public static Object[] Call(ILuaState L, Object[] args, Type[] typedResult)
        {
            int nArgs = args != null ? args.Length : 0;

            int oldTop = L.LuaGetTop() - 1; // -1 because the called is pushed

            // Check the stack
            L.LuaLCheckStack(nArgs + 2, "Lua stack overflow.");

            // Push arguments
            if (args != null)
            {
                for (int i = 0; i < nArgs; i++)
                    L.Push(args[i]);
            }

            // Call
            if (L.LuaPCall(nArgs, L.MultiReturns, 0) != LuaStatus.Ok)
                LuaDotnetHelper.ThrowError(L, oldTop);

            // Convert the result
            int newTop = L.LuaGetTop();
            var result = LuaDotnetHelper.ExtractValues(L, oldTop + 1, newTop, typedResult);
            // Restore the stack
            L.LuaSetTop(oldTop);
            // Returns the result
            return result.ToArray();
        }

        /// <summary>
        /// Convert a Lua value to a .Net table
        /// </summary>
        public static ILuaTable ToTable(ILuaState L, int idx)
        {
            if (L == null) return null;
            // If the value is not a table return null
            if (L.LuaType(idx) != LuaType.Table)
                return null;
            // Create the reference
            L.LuaPushValue(idx);
            var vref = L.LuaRef();
            //if (vref == LuaRef.RefNil || vref == LuaRef.NoRef)
            //    throw new InvalidOperationException("Can't create a reference for this value.");
            return new LuaTable(L, vref);
        }

        /// <summary>
        /// Convert a lua value to a .Net userdata
        /// </summary>
        public static ILuaUserData ToUserData(ILuaState L, int idx)
        {
            if (L == null) return null;
            // If the value is not a userdata return null
            if (!L.LuaIsUserData(idx))
                return null;
            // Create the reference
            L.LuaPushValue(idx);
            var vref = L.LuaRef();
            //if (vref == LuaRef.RefNil || vref == LuaRef.NoRef)
            //    throw new InvalidOperationException("Can't create a reference for this value.");
            return new LuaUserData(L, vref);
        }

        /// <summary>
        /// Convert a lua value to a .Net function
        /// </summary>
        public static ILuaFunction ToFunction(ILuaState L, int idx)
        {
            if (L == null) return null;
            // C function ?
            if (L.LuaIsCFunction(idx))
            {
                return new LuaFunction(L, L.LuaToCFunction(idx));
            }
            else if (L.LuaIsFunction(idx))
            {
                // Create the reference
                L.LuaPushValue(idx);
                var vref = L.LuaRef();
                //if (vref == LuaRef.RefNil || vref == LuaRef.NoRef)
                //    throw new InvalidOperationException("Can't create a reference for this value.");
                return new LuaFunction(L, vref);
            }
            return null;
        }

    }

}
