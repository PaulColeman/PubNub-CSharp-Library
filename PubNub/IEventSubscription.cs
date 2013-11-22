namespace PubNub
{
    internal interface IEventSubscription
    {
        void Call(object message);
        void Unsubscribe(IPrivateEventSubscription unsubscriber);
        int SubscriberCount { get; }
    }
}