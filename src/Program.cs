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
            //Scenario.RunThis<S07_UnicastBus_command_routing>(Conn);
            
            Console.ReadKey();
            
        }
    }
}
