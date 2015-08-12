using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LuaN
{
    /// <summary>
    /// Library dotnet
    /// </summary>
    public class DotnetLibrary
    {
        public const String DotnetObjectMetatable = "dotnet:object";

        static IDictionary<String, LuaCFunction> _LibRegs = new Dictionary<String, LuaCFunction>()
        {
            { "new", Lib_New },
            { "each", Lib_Each },
            { "typeof", Lib_TypeOf },
        };

        /// <summary>
        /// Call require
        /// </summary>
        public static void Require(ILuaState L)
        {
            var lib = new DotnetLibrary();
            L.LuaLRequireF(lib.LibraryName, lib.LuaOpenDotnet, true);
        }

        /// <summary>
        /// dotnet.new
        /// </summary>
        static int Lib_New(ILuaState L)
        {
            try
            {
                int oldTop = L.LuaGetTop();

                if (!L.LuaIsString(1))
                    L.LuaLError("Name of .Net type expected");
                String typeName = L.LuaToString(1);
                Type tp = Type.GetType(typeName);
                if (tp == null)
                    L.LuaLError(".Net type '%s' not found.", typeName);

                List<Object> args = new List<object>();
                for (int i = 2; i <= oldTop; i++)
                    args.Add(L.ToObject(i));
                var ctor = FindBestMethod(tp.GetConstructors(), args);
                Object obj = null;
                if (ctor != null)
                {
                    obj = ctor.Invoke(args.ToArray());
                }
                else
                {
                    obj = Activator.CreateInstance(tp, args.ToArray());
                }
                L.Push(obj);
                L.LuaLSetMetatable(DotnetObjectMetatable);
            }
            catch (LuaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                L.LuaLError(ex.Message);
            }
            return 1;
        }

        /// <summary>
        /// dotnet.each
        /// </summary>
        static int Lib_Each(ILuaState L)
        {
            try {
                var obj = L.ToObject(1);
                IEnumerable enObj = obj as IEnumerable;
                if (enObj == null) L.LuaLError("Not an enumerable");
                L.LuaPushCFunction(EachNext);
                var enumerator = enObj != null ? enObj.GetEnumerator() : null;
                L.LuaPushLightUserData(enumerator);
                L.LuaPushNil();
                return 3;
            }
            catch (LuaException) { throw; }
            catch(Exception ex) { L.LuaLError(ex.Message); }
            return 0;
        }
        static int EachNext(ILuaState L)
        {
            try
            {
                var obj = L.ToObject(1);
                IEnumerator enObj = obj as IEnumerator;
                if (enObj != null)
                {
                    if (enObj.MoveNext())
                        L.Push(enObj.Current);
                    else
                        L.LuaPushNil();
                    return 1;
                }
            }
            catch (LuaException) { throw; }
            catch (Exception ex) { L.LuaLError(ex.Message); }
            return 0;
        }

        /// <summary>
        /// dotnet.typeof
        /// </summary>
        static int Lib_TypeOf(ILuaState L)
        {
            try
            {
                var top = L.LuaGetTop();
                for (int i = 1; i <= top; i++)
                {
                    var val = L.ToObject(i);
                    if (val == null) L.LuaPushNil();
                    else PushObject(L, val.GetType());
                }
                return top;
            }
            catch (LuaException) { throw; }
            catch (Exception ex) { L.LuaLError(ex.Message); }
            return 0;
        }

        /// <summary>
        /// __tostring
        /// </summary>
        private int Mto_ToString(ILuaState L)
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
        private int Mto_Index(ILuaState L)
        {
            try
            {
                var obj = L.LuaToUserData(1);
                if (obj == null) L.LuaLError("Attempt to access a member of a null object.");
                var key = L.ToObject(2);
                if (key == null) L.LuaLError("Attempt to access a null index.");
                var tpObj = obj.GetType();
                // If key is 'string' try get members
                if (L.LuaType(2) == LuaType.String)
                {
                    String memberName = L.LuaToString(2);
                    var member = tpObj.GetMember(memberName).FirstOrDefault();
                    if (member == null)
                        L.LuaLError(String.Format("Unknown member '{0}' on object '{1}'.", memberName, tpObj.FullName));
                    // Property or Field ?
                    if (member is PropertyInfo)
                    {
                        PushObject(L, ((PropertyInfo)member).GetValue(obj, null));
                        return 1;
                    }
                    else if (member is FieldInfo)
                    {
                        PushObject(L, ((FieldInfo)member).GetValue(obj));
                        return 1;
                    }
                    else if (member is MethodInfo)
                    {
                        PushObject(L, new DotnetMethod(obj, tpObj.GetMember(memberName).OfType<MethodInfo>().ToArray()));
                        return 1;
                    }
                    else
                        L.LuaLError(String.Format("Not supported member '{0}' of type '{1}'", memberName, member.GetType().FullName));
                }
                // Search an indexed property
                var idxProperty = tpObj.GetProperties().Where(p =>
                {
                    var prms = p.GetIndexParameters();
                    if (prms.Length != 1) return false;
                    if (prms[0].ParameterType != key.GetType()) return false;
                    return true;
                }).FirstOrDefault();
                if (idxProperty == null)
                    L.LuaLError("Attempt to access an unknown member.");
                PushObject(L, idxProperty.GetValue(obj, new Object[] { key }));
                return 1;
            }
            catch (LuaException) { throw; }
            catch (Exception ex)
            {
                L.LuaLError(ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// __call
        /// </summary>
        private int Mto_Call(ILuaState L)
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
                        var arg = L.ToObject(i);
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
                        PushObject(L, res);
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
                L.LuaLError(ex.Message);
            }
            return 0;
        }

        void CreateObjectMetatable(ILuaState L)
        {
            var oldTop = L.LuaGetTop();

            L.LuaLNewMetatable(DotnetObjectMetatable);

            L.LuaPushValue(-1);
            L.LuaPushCFunction(Mto_ToString);
            L.LuaSetField(-2, "__tostring");

            L.LuaPushValue(-1);
            L.LuaPushCFunction(Mto_Index);
            L.LuaSetField(-2, "__index");

            L.LuaPushValue(-1);
            L.LuaPushCFunction(Mto_Call);
            L.LuaSetField(-2, "__call");

            L.LuaSetTop(oldTop);
        }

        static void PushObject(ILuaState L, Object obj)
        {
            L.Push(obj);
            if (L.LuaType(-1) == LuaType.LightUserData)
                L.LuaLSetMetatable(DotnetObjectMetatable);
        }

        static T FindBestMethod<T>(IEnumerable<T> methods, IList<Object> args) where T : MethodBase
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
            if (methodFind != null && argsFind!=null)
            {
                // Copy the final args
                args.Clear();
                foreach (var a in argsFind)
                    args.Add(a);
            }
            return methodFind;
        }

        /// <summary>
        /// Open the library
        /// </summary>
        public int LuaOpenDotnet(ILuaState L)
        {
            L.LuaLNewLib(_LibRegs);
            CreateObjectMetatable(L);
            return 1;
        }

        /// <summary>
        /// The library name
        /// </summary>
        public String LibraryName { get { return "dotnet"; } }
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
