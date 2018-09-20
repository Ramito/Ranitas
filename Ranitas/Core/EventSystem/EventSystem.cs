using System;
using System.Collections.Generic;

namespace Ranitas.Core
{
    public sealed class EventSystem
    {
        private Dictionary<Type, int> mTypeLookup = new Dictionary<Type, int>();
        private List<IHandlerCollection> mHandlers = new List<IHandlerCollection>();

        public void AddMessageReceiver<TMessage>(Action<TMessage> action) where TMessage : struct
        {
            HandlerCollection<TMessage> handlerCollection;
            if (!mTypeLookup.TryGetValue(typeof(TMessage), out int typeIndex))
            {
                typeIndex = mHandlers.Count;
                mTypeLookup.Add(typeof(TMessage), typeIndex);
                handlerCollection = new HandlerCollection<TMessage>();
                mHandlers.Add(handlerCollection);
            }
            else
            {
                handlerCollection = (HandlerCollection<TMessage>)mHandlers[typeIndex];
            }
            handlerCollection.Add(action);
        }

        public void PostMessage<TMessage>(TMessage message) where TMessage : struct
        {
            if (mTypeLookup.TryGetValue(typeof(TMessage), out int typeIndex))
            {
                HandlerCollection<TMessage> collection = (HandlerCollection<TMessage>)mHandlers[typeIndex];
                collection.Handle(message);
            }
        }

        private interface IHandlerCollection { }

        private class HandlerCollection<TMessage> : IHandlerCollection where TMessage : struct
        {
            private List<Action<TMessage>> mHandlers = new List<Action<TMessage>>();

            public void Add(Action<TMessage> handler)
            {
                mHandlers.Add(handler);
            }

            public void Handle(TMessage message)
            {
                foreach (Action<TMessage> handler in mHandlers)
                {
                    handler(message);
                }
            }
        }
    }
}
