using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN
{

    /// <summary>
    /// Default ILuaDotnet service
    /// </summary>
    public class DefaultLuaDotnetService : ILuaDotnet
    {
        /// <summary>
        /// Create a new service
        /// </summary>
        public DefaultLuaDotnetService(ILuaState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            State = state;
        }

        /// <summary>
        /// Collect the user data
        /// </summary>
        public virtual void CollectUserData(int idx) { }

        /// <summary>
        /// Push
        /// </summary>
        public virtual void Push(object value)
        {
            LuaDotnetHelper.DefaultPush(State, value);
        }

        /// <summary>
        /// Push a dotnet value
        /// </summary>
        public virtual void PushNetObject(object value)
        {
            LuaDotnetHelper.DefaultPushNetObject(State, value);
        }

        /// <summary>
        /// Register 
        /// </summary>
        public virtual void RegisterDotnetMetatable(string metatableName)
        {
            LuaDotnetHelper.CreateDotnetObjectMetatable(State, metatableName);
        }

        /// <summary>
        /// To function
        /// </summary>
        public virtual ILuaFunction ToFunction(int idx)
        {
            return LuaDotnetHelper.DefaultToFunction(State, idx);
        }

        /// <summary>
        /// To table
        /// </summary>
        public virtual ILuaTable ToTable(int idx)
        {
            return LuaDotnetHelper.DefaultToTable(State, idx);
        }

        /// <summary>
        /// To user data
        /// </summary>
        public virtual ILuaUserData ToUserData(int idx)
        {
            return LuaDotnetHelper.DefaultToUserData(State, idx);
        }

        /// <summary>
        /// To value
        /// </summary>
        public virtual object ToValue(int idx)
        {
            return LuaDotnetHelper.DefaultToValue(State, idx);
        }

        /// <summary>
        /// State
        /// </summary>
        public ILuaState State { get; private set; }
    }

}
