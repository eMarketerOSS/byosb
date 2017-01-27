using System;
using main;
using sample;
using sample.Messages.Barista;
using sample.Messages.Cashier;

namespace onpremise
{
    class Program
    {
        static readonly string Conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");

        static void Main(string[] args)
        {
            var customerEndpoint = new UnicastBus(
                "Customer", 
                Conn, 
                router => {
                    router["NewOrder"] = "Cashier@" + Conn;
                }
            );

            var cashierEndpoint = new UnicastBus("Cashier", Conn);
            var baristaEndpoint = new UnicastBus("Barista", Conn);
            
            using (customerEndpoint)
            using (cashierEndpoint)
            using (baristaEndpoint)
            {
                var customer = new CustomerService(customerEndpoint);
                customerEndpoint.RegisterHandler<PaymentDue>(customer.Hadler);
                customerEndpoint.RegisterHandler<DrinkReady>(customer.Hadler);
                customerEndpoint.Subscribe<DrinkReady>("Barista@" + Conn);

                var cashier = new CashierService(cashierEndpoint);
                cashierEndpoint.RegisterHandler<NewOrder>(cashier.Hadler);
                cashierEndpoint.RegisterHandler<SubmitPayment>(cashier.Hadler);

                var barista = new BaristaService(baristaEndpoint);
                baristaEndpoint.RegisterHandler<PrepareDrink>(barista.Hadler);
                baristaEndpoint.RegisterHandler<PaymentComplete>(barista.Hadler);
                baristaEndpoint.Subscribe<PaymentComplete>("Cashier@" + Conn);
                baristaEndpoint.Subscribe<PrepareDrink>("Cashier@" + Conn);

                customerEndpoint.Start();
                cashierEndpoint.Start();
                baristaEndpoint.Start();

                customer.BuyDrinkSync();
            }

            Console.ReadKey();
        }
    }
}
