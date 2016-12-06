using System;
using System.Configuration;
using System.Linq;
using Shuttle.Scenarious;

namespace Shuttle
{
    class Program
    {
        public class Ping { }

        public class PingHandler
        {
            public void Handle(Ping ping)
            {
                Console.WriteLine("got the ping ...");
            }
        }

        static void Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("shuttle-sb-connection");
            Scenario.RunAll(connectionString);
            return;

            var handler = new PingHandler();

            using (var sb1 = new UnicastBus("ep1",connectionString))
            using (var sb2 = new UnicastBus("ep2", connectionString))
            {
                sb1.Start();

                sb1.Subscribe("ep2", "all");
                sb1.RegisterHandler<Ping>(handler.Handle);

                sb2.Start();
                sb2.Publish(new Ping());


                //sb2.Subscribe("ep1", "all");

                //sb1.Send("ep2", new BrokeredMessage("This is a test message!"));
                //sb1.Publish(new BrokeredMessage("This is a test message!"));

                Console.ReadKey();
            }
        }
    }
}
