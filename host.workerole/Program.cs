using System;
using System.Threading;
using main;
using Microsoft.Azure;
using sample;
using sample.Messages.Barista;
using sample.Messages.Cashier;

namespace multiendpoints
{
    public class ConfigureCustomerEndpoint : IConfiguraThisEndpoint
    {
        public UnicastBus Configure(CancellationToken token)
        {
            var conn = Environment.GetEnvironmentVariable("shuttle-sb-connection") ?? CloudConfigurationManager.GetSetting("shuttle-sb-connection");
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
            var conn = Environment.GetEnvironmentVariable("shuttle-sb-connection") ?? CloudConfigurationManager.GetSetting("shuttle-sb-connection");
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
            var conn = Environment.GetEnvironmentVariable("shuttle-sb-connection") ?? CloudConfigurationManager.GetSetting("shuttle-sb-connection");
            var bus = new UnicastBus("Barista", conn);

            var barista = new BaristaService(bus);
            bus.RegisterHandler<PrepareDrink>(barista.Hadler);
            bus.RegisterHandler<PaymentComplete>(barista.Hadler);
            bus.Subscribe<PaymentComplete>("Cashier@" + conn);
            bus.Subscribe<PrepareDrink>("Cashier@" + conn);

            return bus;
        }
    }
}
