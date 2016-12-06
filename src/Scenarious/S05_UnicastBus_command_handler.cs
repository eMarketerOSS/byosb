using System;

namespace Shuttle.Scenarious
{
    class S05_UnicastBus_command_handler : Scenario
    {
        public class ChargeCard { }

        public class ChargeCardHandler
        {
            public void Handle(ChargeCard chargeCard)
            {
                Console.WriteLine("Handle chargeCard ...");
            }
        }

        public override void Run()
        {
            var handler = new ChargeCardHandler();

            using (var sb1 = new UnicastBus("ep1", Conn))
            using (var sb2 = new UnicastBus("ep2", Conn))
            {
                sb1.Start();
                sb1.RegisterHandler<ChargeCard>(handler.Handle);

                sb2.Start();
                sb2.Send("ep1", new ChargeCard());
            }
        }
    }
}