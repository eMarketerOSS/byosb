using System;
using System.Threading;
using main;
using sample;
using sample.Messages.Barista;
using sample.Messages.Cashier;

namespace multiendpoints
{
    public class ConfigureCustomerEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            var conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");
            var bus = new UnicastBus("Customer", conn);

            var customer = new CustomerService(bus);
            bus.RegisterHandler<PaymentDue>(customer.Hadler);
            bus.RegisterHandler<DrinkReady>(customer.Hadler);
            bus.Subscribe<DrinkReady>("Barista@" + conn);

            return bus;
        }
    }

    public class ConfigureCashierEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            var conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");
            var bus = new UnicastBus("Cashier", conn);

            var cashier = new CashierService(bus);
            bus.RegisterHandler<NewOrder>(cashier.Hadler);
            bus.RegisterHandler<SubmitPayment>(cashier.Hadler);

            return bus;
        }
    }

    public class ConfigureBaristaEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            var conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");
            var bus = new UnicastBus("Barista", conn);

            var barista = new BaristaService(bus);
            bus.RegisterHandler<PrepareDrink>(barista.Hadler);
            bus.RegisterHandler<PaymentComplete>(barista.Hadler);
            bus.Subscribe<PaymentComplete>("Cashier@" + conn);
            bus.Subscribe<PrepareDrink>("Cashier@" + conn);

            return bus;
        }
    }

    /*class Program
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
    }*/
}
