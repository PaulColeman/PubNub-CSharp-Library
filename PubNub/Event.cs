namespace PubNub
{
    abstract public class Event
    {
        virtual public string EventId { get { return this.GetType().ToString(); } }
    }
}