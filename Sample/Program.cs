using System;
using PubNub;

namespace Sample
{
    public class MyTestEvent : Event
    {
        public string Time { get; set; }
    }

    public class MyEvent : Event
    {
        public int Foo { get; set; }
        public string Bar { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var pubNub = new Pubnub();
            var channel = pubNub.Channel("test events");

            var sub1 = channel.Subscribe<MyEvent>(m => Console.WriteLine("sub 1: " + m.Bar));
            var sub2 = channel.Subscribe<MyEvent>(m => Console.WriteLine("sub 2: " + m.Foo));
            var sub3 = channel.Subscribe<MyEvent>(GotMessage);

            var sub4 = channel.Subscribe<MyTestEvent>(m => Console.WriteLine("it is " + m.Time));

            Console.WriteLine("CTRL + C to quit.  Enter to publish again.");
            while (true)
            {
                channel.Publish(new MyEvent { Bar = "twelve", Foo = 12 });
                channel.Publish(new MyTestEvent { Time = DateTimeOffset.Now.ToString()});
                Console.ReadLine();
            }
        }

        private static void GotMessage(MyEvent message)
        {
            Console.WriteLine(message.Bar + message.Foo);
        }
    }
}
