﻿using Microsoft.ServiceBus.Messaging;

namespace Shuttle.Scenarious
{
    class S04_UnicastBus_publish : Scenario
    {
        public override void Run()
        {
            using (var sb1 = new UnicastBus("ep1", Conn))
            using (var sb2 = new UnicastBus("ep2", Conn))
            {
                sb1.Start();
                sb2.Start();

                sb2.Subscribe("ep1", "all");

                sb1.Publish(new BrokeredMessage("This is a test message!"));
            }
        }
    }
}