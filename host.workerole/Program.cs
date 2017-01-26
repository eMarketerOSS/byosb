using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using main;

namespace Starbucks.Host
{
    public class ConfigureCustomerEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            return new UnicastBus("Customer", Environment.GetEnvironmentVariable("shuttle-sb-connection"));
        }
    }

    public class ConfigureCashierEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            return new UnicastBus("Cashier", Environment.GetEnvironmentVariable("shuttle-sb-connection"));
        }
    }

    public class ConfigureBaristaEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            return new UnicastBus("Barista", Environment.GetEnvironmentVariable("shuttle-sb-connection"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bin = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll")
                .Select(Assembly.LoadFile)
                .Union(new List<Assembly>() { Assembly.GetExecutingAssembly() })
                .ToList();

            var types = bin
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterfaces().Contains(typeof(IConfiguraThisEndpoint)))
                .ToList();

            foreach (var type in types)
            {
                Console.WriteLine("Discovered endpoint " + type);

                var ep = (IConfiguraThisEndpoint)Activator.CreateInstance(type);
                var unicastBus = ep.Configure(CancellationToken.None);
                unicastBus.Start();
            }

            Console.ReadLine();
        }
    }
}
