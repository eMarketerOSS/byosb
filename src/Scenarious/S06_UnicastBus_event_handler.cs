using System;

namespace Shuttle.Scenarious
{
    class S06_UnicastBus_event_handler : Scenario
    {
        public class RateChanged { }

        public class PingHandler
        {
            public void Handle(MessageContext ctx, RateChanged rateChanged)
            {
                Console.WriteLine("handling rateChanged ...");
            }
        }

        public override void Run()
        {
            var handler = new PingHandler();

            using (var sb1 = new UnicastBus("ep1", Conn))
            using (var sb2 = new UnicastBus("ep2", Conn))
            {
                sb1.Start();

                sb1.Subscribe("ep2", "all");
                sb1.RegisterHandler<RateChanged>(handler.Handle);

                sb2.Start();
                sb2.Publish(new RateChanged());
            }
        }
    }
}