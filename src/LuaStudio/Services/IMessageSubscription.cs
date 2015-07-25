using System;

namespace LuaStudio.Services
{
    public interface IMessageSubscription : IDisposable
    {
    }

    public interface IMessageSubscription<TMessage> : IMessageSubscription where TMessage : Message
    {
    }

}