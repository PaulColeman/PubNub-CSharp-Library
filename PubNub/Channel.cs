using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PubNub
{
    internal class Channel : IChannel
    {
        private readonly static IDictionary<string, IEventSubscription> Subscriptions = new Dictionary<string, IEventSubscription>();

        private readonly PubnubBase _pubnub;
        public string Name { get; private set; }
        private object _timeToken = 0;
        private volatile bool _exit = true;

        internal Channel(string subscriberKey, string publisherKey, string secretKey, bool useSsl, string channel)
        {
            Name = channel;
            _pubnub = new PubnubBase(subscriberKey, publisherKey, secretKey, useSsl);
        }

        public void Publish<T>(T message)
            where T : Event
        {
            Task.Factory.StartNew(() => _pubnub.Publish(Name, message));
        }

        public ISubscription Subscribe<T>(Action<T> callback)
            where T : Event
        {
            var eventId = ((T) Activator.CreateInstance(typeof (T))).EventId;

            ISubscription subscription;
            IEventSubscription eventSubscription;
            if (!Subscriptions.TryGetValue(eventId, out eventSubscription))
            {
                var eS = new EventSubscription<T>(this, eventId);
                Subscriptions.Add(eventId, eS);
                subscription = eS.Subscribe(callback);
            }
            else
                subscription = ((EventSubscription<T>)eventSubscription).Subscribe(callback);

            if (_exit)
            {
                _exit = false;
                Task.Factory.StartNew(Run);
            }

            return subscription;
        }

        internal void Unsubscribe(IPrivateEventSubscription subscriber)
        {
            var eventSubscription = Subscriptions[subscriber.EventId];
            eventSubscription.Unsubscribe(subscriber);

            if (eventSubscription.SubscriberCount == 0)
                Subscriptions.Remove(subscriber.EventId);

            if (Subscriptions.Keys.Count == 0)
            {
                // no more subscriptions for this channel
                _exit = true;
            }
        }

        private void Run()
        {
            while (!_exit)
            {
                try
                {
                    _pubnub.Subscribe(Name, Callback, ref _timeToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                }
            }
        }

        internal bool Callback(Object message)
        {
            if (_exit)
                return true;

            var objs = message as IDictionary<string, object>;
            if (objs == null)
                return true;

            var eventId = objs["EventId"] as string;
            if (string.IsNullOrEmpty(eventId))
                return true;

            IEventSubscription eventSubscription;
            if (Subscriptions.TryGetValue(eventId, out eventSubscription))
            {
                eventSubscription.Call(message);
            }

            return true;
        }
    }
}