using System;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace Shuttle.Scenarious
{
    class S04_UnicastBus_publish : Scenario
    {
        public override void Run()
        {
            var bh = new BlueFlagHandler();
            var rf = new RedFlagHandler();

            using (var pub = new UnicastBus("pub", Conn))
            using (var sub1 = new UnicastBus("sub1", Conn))
            using (var sub2 = new UnicastBus("sub2", Conn))
            {
                sub1.RegisterHandler<BlueFlag>(bh.Handle);
                sub2.RegisterHandler<RedFlag>(rf.Handle);

                pub.Start();
                sub1.Start();
                sub2.Start();

                sub1.Subscribe<BlueFlag>("pub@" + Conn);

                pub.Publish(new BlueFlag());
                pub.Publish(new RedFlag());

                Thread.Sleep(1000);
            }
        }
    }

    public class BlueFlag { }
    public class RedFlag { }

    public class BlueFlagHandler
    {
        public void Handle(MessageContext ctx, BlueFlag rateChanged)
        {
            Console.WriteLine("handling BlueFlag ...");
        }
    }

    public class RedFlagHandler
    {
        public void Handle(MessageContext ctx, RedFlag rateChanged)
        {
            Console.WriteLine("handling RedFlag ...");
        }
    }

    // Topics
    // Name: OwnerEndpointName + ".event"
    //  Subscriptions
    //      Name: SubscriberEndpoint+EventType
    //          Rule: Sql rule for event type
}