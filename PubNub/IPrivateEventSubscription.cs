namespace PubNub
{
    internal interface IPrivateEventSubscription : ISubscription
    {
        void Call(object message);
    }
}