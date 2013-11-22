using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PubNub
{
    internal class EventSubscription<T> : IEventSubscription
        where T : Event
    {
        readonly List<EventCallback<T>> _subscribers = new List<EventCallback<T>>();
        public int SubscriberCount { get { return _subscribers.Count; } }
        internal string EventId { get; private set; }
        internal Channel Channel { get; private set; }

        internal EventSubscription(Channel channel, string eventId)
        {
            Channel = channel;
            EventId = eventId;
        }

        public void Call(object message)
        {
            var serializer = new JavaScriptSerializer();
            var t = serializer.ConvertToType<T>(message);

            foreach(var subscriber in _subscribers)
            {
                var subscriber1 = subscriber;
                Task.Factory.StartNew(() => subscriber1.Call(t));
            }
        }

        public IPrivateEventSubscription Subscribe(Action<T> callback)
        {
            var subscriber = new EventCallback<T>(Channel, EventId, callback);
            _subscribers.Add(subscriber);

            return subscriber;
        }

        public void Unsubscribe(IPrivateEventSubscription unsubscriber)
        {
            _subscribers.Remove(unsubscriber as EventCallback<T>);
        }
    }
}