using System;
using System.Web.Script.Serialization;

namespace PubNub
{
    internal class EventCallback<T> : IPrivateEventSubscription
        where  T : Event
    {
        internal Channel Channel { get; private set; }
        internal Action<T> Callback { get; private set; }
        public string EventId { get; private set; }

        internal EventCallback(Channel channel, string eventId, Action<T> callback)
        {
            Channel = channel;
            EventId = eventId;
            Callback = callback;
        }

        public void Unsubscribe()
        {
            Channel.Unsubscribe(this);
        }

        public void Call(object message)
        {
            // todo: deserialization should happen once per pub nub message, not once per subscription
            var serializer = new JavaScriptSerializer();
            var t = serializer.ConvertToType<T>(message);

            Callback(t);
        }
    }
}