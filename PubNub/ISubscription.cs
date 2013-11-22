namespace PubNub
{
    public interface ISubscription
    {
        string EventId { get; }
        void Unsubscribe();
    }
}