using System;
using Shuttle.Scenarious;

namespace Shuttle
{
    class Program
    {
        static readonly string Conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");

        static void Main(string[] args)
        {
            Scenario.RunAll(Conn);
            
            //dev flow
            //Scenario.RunThis<S06_UnicastBus_event_handler>(conn);
            
            Console.ReadKey();
            
        }
    }
}
