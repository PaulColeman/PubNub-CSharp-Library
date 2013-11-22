using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubNub
{
    public class Pubnub
    {
        private static readonly IDictionary<string, Channel> Channels = new ConcurrentDictionary<string, Channel>();
        private readonly static object LockChannel = new Object();

        public Pubnub(string subscriberKey = "demo", string publisherKey = "demo", string secretKey = "", bool useSsl = false)
        {
            SubscriberKey = subscriberKey;
            PublisherKey = publisherKey;
            SecretKey = secretKey;
            UseSsl = useSsl;
        }

        protected bool UseSsl { get; set; }
        protected string SecretKey { get; set; }
        protected string PublisherKey { get; set; }
        protected string SubscriberKey { get; set; }

        public IChannel Channel(string channel)
        {
            Channel c;
            if (Channels.TryGetValue(channel, out c))
                return c;

            lock (LockChannel)
            {
                if (Channels.TryGetValue(channel, out c))
                    return c;

                c = new Channel(SubscriberKey, PublisherKey, SecretKey, UseSsl, channel);
                Channels.Add(channel, c);

                return c;
            }
        }
    }
}