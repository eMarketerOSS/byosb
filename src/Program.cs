﻿using System;
using Microsoft.ServiceBus;
using Shuttle.Scenarious;

namespace Shuttle
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
