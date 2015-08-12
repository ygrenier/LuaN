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
                var ctor = LuaDotnetHelper.FindBestMethod(tp.GetConstructors(), args);
                Object obj = null;
                if (ctor != null)
                {
                    obj = ctor.Invoke(args.ToArray());
                }
                else
                {
                    obj = Activator.CreateInstance(tp, args.ToArray());
                }
                L.PushNetObject(obj);
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
                    else L.PushNetObject(val.GetType());
                }
                return top;
            }
            catch (LuaException) { throw; }
            catch (Exception ex) { L.LuaLError(ex.Message); }
            return 0;
        }

        /// <summary>
        /// Open the library
        /// </summary>
        public int LuaOpenDotnet(ILuaState L)
        {
            L.RegisterDotnetMetatable();
            L.LuaLNewLib(_LibRegs);
            return 1;
        }

        /// <summary>
        /// The library name
        /// </summary>
        public String LibraryName { get { return "dotnet"; } }
    }

}
