using System;

namespace PubNub
{
    public interface IChannel
    {
        void Publish<T>(T message) where T : Event;
        ISubscription Subscribe<T>(Action<T> callback) where T : Event;
    }
}