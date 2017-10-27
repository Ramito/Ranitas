using System;
using System.Collections.Generic;

namespace Ranitas.Core.EventSystem
{
    public sealed class EventSystem
    {
        Dictionary<Type, List<IMessageHandler>> mHandlers = new Dictionary<Type, List<IMessageHandler>>();

        public void AddMessageReceiver<T>(Action<T> action) where T : struct
        {
            if (!mHandlers.TryGetValue(TypeCache<T>.Type, out List<IMessageHandler> handlers))
            {
                handlers = new List<IMessageHandler>();
                mHandlers.Add(TypeCache<T>.Type, handlers);
            }
            handlers.Add(new MessageHandler<T>(action));
        }

        public void PostMessage<T>(T message)
        {
            if (mHandlers.TryGetValue(TypeCache<T>.Type, out List<IMessageHandler> handlers))
            {
                foreach (var handler in handlers)
                {
                    handler.Handle(message);
                }
            }
        }

        private interface IMessageHandler
        {
            void Handle(object message);
        }

        private class MessageHandler<T> : IMessageHandler
        {
            private readonly Action<T> mAction;

            public MessageHandler(Action<T> action)
            {
                mAction = action;
            }

            public void Handle(object message)
            {
                mAction((T)message);
            }
        }
    }

    public static class TypeCache<T>
    {
        public static Type Type = typeof(T);
    }
}
