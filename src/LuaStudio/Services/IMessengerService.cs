using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.Services
{
    public interface IMessengerService
    {
        void SendMessage<TMessage>(TMessage message) where TMessage : Message;
        IMessageSubscription<TMessage> Subscribe<TMessage>(Action<TMessage> callback) where TMessage : Message;
    }
}
