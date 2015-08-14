using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Default .Net behavior and utilities for Lua
    /// </summary>
    public static class LuaDotnetHelper
    {
        /// <summary>
        /// Name for he default metatable for the .Net object managed in Lua
        /// </summary>
        public const String DotnetObjectMetatableName = "LuaN:DotnetObject";

        /// <summary>
        /// Default Push
        /// </summary>
        public static void DefaultPush(ILuaState L, Object value)
        {
            if (value == null)
                L.LuaPushNil();
            else if (value is Boolean)
                L.LuaPushBoolean((Boolean)value);
            else if (value is Single || value is Double || value is Decimal)
                L.LuaPushNumber(Convert.ToDouble(value));
            else if (value is SByte || value is Byte || value is Int16 || value is UInt16 || value is Int32 || value is UInt16 || value is Int64 || value is UInt64)
                L.LuaPushInteger(Convert.ToInt64(value));
            else if (value is Char || value is String)
                L.LuaPushString(value.ToString());
            else if (value is ILuaNativeUserData)
                throw new InvalidOperationException("Can't push a userdata");
            else if (value is LuaCFunction)
                L.LuaPushCFunction((LuaCFunction)value);
            else if (value is ILuaState)
                if (value == L)
                    L.LuaPushThread();
                else
                    throw new InvalidOperationException("Can't push a different thread");
            else if (value is ILuaValue)
                ((ILuaValue)value).Push(L);
            else
                L.PushNetObject(value);
        }

        /// <summary>
        /// Default ToValue
        /// </summary>
        public static Object DefaultToValue(ILuaState L, int idx)
        {
            if (L == null) return null;
            var tp = L.LuaType(idx);
            switch (tp)
            {
                case LuaType.Boolean:
                    return L.LuaToBoolean(idx);
                case LuaType.Number:
                    return L.LuaToNumber(idx);
                case LuaType.String:
                    return L.LuaToString(idx);
                case LuaType.LightUserData:
                    return L.LuaToUserData(idx);
                case LuaType.UserData:
                    return L.ToUserData(idx);
                case LuaType.Table:
                    return L.ToTable(idx);
                case LuaType.Function:
                    return L.ToFunction(idx);
                case LuaType.Thread:
                    return L.LuaToThread(idx);
                case LuaType.None:
                case LuaType.Nil:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Default push a value as a .Net object
        /// </summary>
        public static void DefaultPushNetObject(ILuaState L, Object value)
        {
            L.LuaPushLightUserData(value);
            L.LuaLSetMetatable(LuaDotnetHelper.DotnetObjectMetatableName);
        }

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
        public static ILuaTable DefaultToTable(ILuaState L, int idx)
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
        public static ILuaUserData DefaultToUserData(ILuaState L, int idx)
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
        public static ILuaFunction DefaultToFunction(ILuaState L, int idx)
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

        /// <summary>
        /// Create the default metatable for the .Net objects
        /// </summary>
        public static void CreateDotnetObjectMetatable(ILuaState L, String metatableName = null)
        {
            var oldTop = L.LuaGetTop();

            if (String.IsNullOrWhiteSpace(metatableName))
                metatableName = DotnetObjectMetatableName;
            L.LuaLNewMetatable(metatableName);

            L.LuaPushValue(-1);
            L.LuaPushCFunction(DotnetObjectMeta_ToString);
            L.LuaSetField(-2, "__tostring");

            L.LuaPushValue(-1);
            L.LuaPushCFunction(DotnetObjectMeta_Index);
            L.LuaSetField(-2, "__index");

            L.LuaPushValue(-1);
            L.LuaPushCFunction(DotnetObjectMeta_NewIndex);
            L.LuaSetField(-2, "__newindex");

            L.LuaPushValue(-1);
            L.LuaPushCFunction(DotnetObjectMeta_Call);
            L.LuaSetField(-2, "__call");

            L.LuaPushValue(-1);
            L.LuaPushCFunction(DotnetObjectMeta_GC);
            L.LuaSetField(-2, "__gc");

            L.LuaSetTop(oldTop);
        }

        /// <summary>
        /// Try to find the best method from the args provided.
        /// </summary>
        /// <typeparam name="T">Type of the MethodBase</typeparam>
        /// <param name="methods">List of the methods</param>
        /// <param name="args">List of the arguments</param>
        /// <returns>The method, or null if not found.</returns>
        /// <remarks>
        /// If the the method is find, the list <paramref name="args"/> is rebuild whith the correct values.
        /// </remarks>
        public static T FindBestMethod<T>(IEnumerable<T> methods, IList<Object> args) where T : MethodBase
        {
            T methodFind = null;
            IList<Object> argsFind = null;
            // Save original args
            var originalArgs = new List<Object>(args);
            foreach (var mth in methods)
            {
                var prms = mth.GetParameters();
                if (prms.Length != args.Count) continue;
                // Check args and convert them
                var newArgs = new List<Object>();
                try
                {
                    for (int i = 0; i < prms.Length; i++)
                        newArgs.Add(Convert.ChangeType(args[i], prms[i].ParameterType, System.Globalization.CultureInfo.InvariantCulture));
                }
                catch { continue; }
                // Here we find our method
                methodFind = mth;
                argsFind = newArgs;
                break;
            }
            if (methodFind != null && argsFind != null)
            {
                // Copy the final args
                args.Clear();
                foreach (var a in argsFind)
                    args.Add(a);
            }
            return methodFind;
        }

        /// <summary>
        /// __tostring
        /// </summary>
        public static int DotnetObjectMeta_ToString(ILuaState L)
        {
            var obj = L.LuaToUserData(1);
            if (obj != null)
            {
                L.LuaPushString(obj.ToString());
            }
            else
            {
                L.LuaPushNil();
            }
            return 1;
        }

        /// <summary>
        /// __index
        /// </summary>
        public static int DotnetObjectMeta_Index(ILuaState L)
        {
            try
            {
                var obj = L.LuaToUserData(1);
                if (obj == null) L.LuaLError("Attempt to access a member of a null object.");
                var key = L.ToValue(2);
                if (key == null) L.LuaLError("Attempt to access a null index.");
                var tpObj = obj.GetType();
                // If key is 'string' try get members
                if (L.LuaType(2) == LuaType.String)
                {
                    String memberName = L.LuaToString(2);
                    var member = tpObj.GetMember(memberName).FirstOrDefault();
                    if (member != null)
                    {
                        // Property or Field ?
                        if (member is PropertyInfo)
                        {
                            L.Push(((PropertyInfo)member).GetValue(obj, null));
                            return 1;
                        }
                        else if (member is FieldInfo)
                        {
                            L.Push(((FieldInfo)member).GetValue(obj));
                            return 1;
                        }
                        else if (member is MethodInfo)
                        {
                            L.Push(new DotnetMethod(obj, tpObj.GetMember(memberName).OfType<MethodInfo>().ToArray()));
                            return 1;
                        }
                        else
                            L.LuaLError(String.Format("Not supported member '{0}' of type '{1}'", memberName, member.GetType().FullName));
                    }
                }
                // Search an indexed property
                PropertyInfo idxProperty = null;
                Object bestKey = null;
                int score = -1;
                foreach (var p in tpObj.GetProperties())
                {
                    var prms = p.GetIndexParameters();
                    if (prms.Length != 1) continue;
                    if (prms[0].ParameterType == key.GetType())
                    {
                        idxProperty = p;
                        score = 1000;
                        bestKey = key;
                        break;
                    };
                    try
                    {
                        var fKey = Convert.ChangeType(key, prms[0].ParameterType, System.Globalization.CultureInfo.InvariantCulture);
                        var fScore = prms[0].ParameterType == typeof(String) ? 100 : 500;
                        if (fScore > score)
                        {
                            bestKey = fKey;
                            score = fScore;
                            idxProperty = p;
                        }
                    }
                    catch { }
                }
                if (idxProperty == null)
                    L.LuaLError("Attempt to access an unknown member.");
                L.Push(idxProperty.GetValue(obj, new Object[] { bestKey }));
                return 1;
            }
            catch (LuaException) { throw; }
            catch (Exception ex)
            {
                L.LuaLError(ex.GetBaseException().Message);
            }
            return 0;
        }

        /// <summary>
        /// __newindex
        /// </summary>
        public static int DotnetObjectMeta_NewIndex(ILuaState L)
        {
            try
            {
                var obj = L.LuaToUserData(1);
                if (obj == null) L.LuaLError("Attempt to set a member of a null object.");
                var key = L.ToValue(2);
                if (key == null) L.LuaLError("Attempt to set a null index.");
                var value = L.ToValue(3);
                var tpObj = obj.GetType();
                // If key is 'string' try get members
                if (L.LuaType(2) == LuaType.String)
                {
                    String memberName = L.LuaToString(2);
                    var member = tpObj.GetMember(memberName).FirstOrDefault();
                    if (member != null)
                    {
                        // Property or Field ?
                        if (member is PropertyInfo)
                        {
                            if (!((PropertyInfo)member).CanWrite)
                                L.LuaLError(String.Format("Attempt to set the readonly '{0}' property on object '{1}'.", memberName, tpObj.FullName));
                            value = Convert.ChangeType(value, ((PropertyInfo)member).PropertyType, System.Globalization.CultureInfo.InvariantCulture);
                            ((PropertyInfo)member).SetValue(obj, value, null);
                            return 0;
                        }
                        else if (member is FieldInfo)
                        {
                            value = Convert.ChangeType(value, ((FieldInfo)member).FieldType, System.Globalization.CultureInfo.InvariantCulture);
                            ((FieldInfo)member).SetValue(obj, value);
                            return 0;
                        }
                        else if (member is MethodInfo)
                        {
                            L.LuaLError("Attempt to set a mathod.");
                        }
                        else
                            L.LuaLError(String.Format("Not supported member '{0}' of type '{1}'", memberName, member.GetType().FullName));
                    }
                }
                // Search an indexed property
                PropertyInfo idxProperty = null;
                Object bestKey = null;
                int score = -1;
                foreach (var p in tpObj.GetProperties())
                {
                    var prms = p.GetIndexParameters();
                    if (prms.Length != 1) continue;
                    if (prms[0].ParameterType == key.GetType())
                    {
                        idxProperty = p;
                        score = 1000;
                        bestKey = key;
                        break;
                    };
                    try
                    {
                        var fKey = Convert.ChangeType(key, prms[0].ParameterType, System.Globalization.CultureInfo.InvariantCulture);
                        var fScore = prms[0].ParameterType == typeof(String) ? 100 : 500;
                        if (fScore > score)
                        {
                            bestKey = fKey;
                            score = fScore;
                            idxProperty = p;
                        }
                    }
                    catch { }
                }
                if (idxProperty == null)
                    L.LuaLError("Attempt to set an unknown member.");
                if (!idxProperty.CanWrite)
                    L.LuaLError(String.Format("Attempt to set the readonly indexed property."));
                value = Convert.ChangeType(value, idxProperty.PropertyType, System.Globalization.CultureInfo.InvariantCulture);
                idxProperty.SetValue(obj, value, new Object[] { bestKey });
                return 0;
            }
            catch (LuaException) { throw; }
            catch (Exception ex)
            {
                L.LuaLError(ex.GetBaseException().Message);
            }
            return 0;
        }

        /// <summary>
        /// __call
        /// </summary>
        public static int DotnetObjectMeta_Call(ILuaState L)
        {
            try
            {
                var obj = L.LuaToUserData(1);
                if (obj == null) L.LuaLError("Attempt to access a member of a null object.");
                DotnetMethod dm = obj as DotnetMethod;
                if (dm == null && obj is Delegate)
                {
                    dm = new DotnetMethod(((Delegate)obj).Target, new MethodInfo[] { ((Delegate)obj).Method });
                }
                if (dm != null)
                {
                    // Build args
                    var top = L.LuaGetTop();
                    List<Object> args = new List<object>();
                    for (int i = 2; i <= top; i++)
                    {
                        var arg = L.ToValue(i);
                        if (!(i == 2 && arg == dm.Object))  // detect self
                            args.Add(arg);
                    }
                    // Find the method
                    var mth = FindBestMethod(dm.Methods, args);
                    if (mth == null) L.LuaLError("Can't call the method with these arguments.");
                    // Invoke
                    var res = mth.Invoke(dm.Object, args.ToArray());
                    if (mth.ReturnType != null && mth.ReturnType != typeof(void))
                    {
                        L.Push(res);
                        return 1;
                    }
                    else
                        return 0;
                }
                else
                    L.LuaLError("Attempts to call a non delegate object.");
            }
            catch (LuaException) { throw; }
            catch (Exception ex)
            {
                L.LuaLError(ex.GetBaseException().Message);
            }
            return 0;
        }

        /// <summary>
        /// __gc
        /// </summary>
        public static int DotnetObjectMeta_GC(ILuaState L)
        {
            try
            {
                var ldn = L.GetService<ILuaDotnet>();
                if (ldn != null)
                    ldn.CollectUserData(1);
            }
            catch (LuaException) { throw; }
            catch (Exception ex)
            {
                L.LuaLError(ex.GetBaseException().Message);
            }
            return 0;
        }

    }

    /// <summary>
    /// Method
    /// </summary>
    public class DotnetMethod
    {
        /// <summary>
        /// New .Net method
        /// </summary>
        public DotnetMethod(Object obj, MethodInfo[] methods)
        {
            this.Object = obj;
            this.Methods = methods;
        }
        /// <summary>
        /// Object parent
        /// </summary>
        public Object Object { get; private set; }
        /// <summary>
        /// Methods
        /// </summary>
        public MethodInfo[] Methods { get; private set; }
    }

}
