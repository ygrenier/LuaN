using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace LuaStudio.Services
{
    [Export(typeof(IMessengerService))]
    class MessengerService : IMessengerService
    {
        List<MessageSubscription> _Subscriptions = new List<MessageSubscription>();

        class MessageSubscription : IMessageSubscription
        {
            public MessageSubscription(MessengerService service, Delegate callback, Type tMessage)
            {
                this.Service = service;
                this.TypeMessage = tMessage;
                this.MessageInvoker = callback;
            }
            public void Dispose()
            {
                Service._Subscriptions.Remove(this);
            }
            public void Invoke(Message message)
            {
                if (message == null) return;
                var tMessage = message.GetType();
                if (tMessage == TypeMessage || tMessage.IsSubclassOf(TypeMessage) || TypeMessage.IsAssignableFrom(tMessage))
                {
                    MessageInvoker.DynamicInvoke(message);
                }
            }
            public MessengerService Service { get; private set; }
            public Type TypeMessage { get; private set; }
            public Delegate MessageInvoker { get; private set; }
        }
        class MessageSubscription<T> : MessageSubscription, IMessageSubscription<T> where T : Message
        {
            public MessageSubscription(MessengerService service, Action<T> callback) : base(service, callback, typeof(T))
            {
            }
        }

        public IMessageSubscription<TMessage> Subscribe<TMessage>(Action<TMessage> callback) where TMessage : Message
        {
            var sub = new MessageSubscription<TMessage>(this, callback);
            _Subscriptions.Add(sub);
            return sub;
        }

        public void SendMessage<TMessage>(TMessage message) where TMessage : Message
        {
            if (message == null) return;
            var tMessage = message.GetType();
            foreach (var sub in _Subscriptions.Where(s => tMessage == s.TypeMessage || tMessage.IsSubclassOf(s.TypeMessage) || s.TypeMessage.IsAssignableFrom(tMessage)))
            {
                sub.Invoke(message);
            }
        }

    }

}
