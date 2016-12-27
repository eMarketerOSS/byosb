using System;
using usage.Scenarious;

namespace usage
{
    class Program
    {
        static readonly string Conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");

        static void Main(string[] args)
        {
            //Scenario.RunAll(Conn);

            //dev flow
            Scenario.RunThis<S04_UnicastBus_publish>(Conn);

            Console.ReadKey();
        }
    }
}
