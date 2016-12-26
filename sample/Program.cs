using System;
using System.Threading;
using sample.Starbucks;
using sample.Starbucks.Starbucks.Messages.Barista;
using sample.Starbucks.Starbucks.Messages.Cashier;
using Shuttle;

namespace sample
{
    class Program
    {
        static readonly string Conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");

        static void Main(string[] args)
        {
            var customerEndpoint = new UnicastBus(
                "Starbucks.Customer", 
                Conn, 
                router => {
                    router["NewOrder"] = "Starbucks.Cashier@" + Conn;
                }
            );

            var cashierEndpoint = new UnicastBus("Starbucks.Cashier", Conn);
            var baristaEndpoint = new UnicastBus("Starbucks.Barista", Conn);
            
            using (customerEndpoint)
            using (cashierEndpoint)
            using (baristaEndpoint)
            {
                customerEndpoint.Start();
                cashierEndpoint.Start();
                baristaEndpoint.Start();

                var customer = new CustomerService(customerEndpoint);
                customerEndpoint.RegisterHandler<PaymentDue>(customer.Hadler);
                customerEndpoint.RegisterHandler<DrinkReady>(customer.Hadler);
                customerEndpoint.Subscribe<DrinkReady>("Starbucks.Barista@" + Conn);

                var cashier = new CashierService(cashierEndpoint);
                cashierEndpoint.RegisterHandler<NewOrder>(cashier.Hadler);
                cashierEndpoint.RegisterHandler<SubmitPayment>(cashier.Hadler);

                var barista = new BaristaService(baristaEndpoint);
                baristaEndpoint.RegisterHandler<PrepareDrink>(barista.Hadler);
                baristaEndpoint.RegisterHandler<PaymentComplete>(barista.Hadler);
                baristaEndpoint.Subscribe<PaymentComplete>("Starbucks.Cashier@" + Conn);
                baristaEndpoint.Subscribe<PrepareDrink>("Starbucks.Cashier@" + Conn);

                customer.BuyDrinkSync();
            }

            Console.ReadKey();
        }
    }
}
