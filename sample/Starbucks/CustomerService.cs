using System;
using System.Threading;
using sample.Starbucks.Starbucks.Messages.Barista;
using sample.Starbucks.Starbucks.Messages.Cashier;
using Shuttle;

namespace sample.Starbucks
{
    class CustomerService
    {
        private readonly UnicastBus _bus;
        private ManualResetEvent _wait;

        public CustomerService(UnicastBus bus)
        {
            _bus = bus;
        }

        public void BuyDrinkSync()
        {
            _wait = new ManualResetEvent(false);

            _bus.Send(new NewOrder { CustomerName = "c1", DrinkName = "Latte", Size = DrinkSize.Tall });

            if (_wait.WaitOne(TimeSpan.FromSeconds(30), false) == false)
                throw new InvalidOperationException("didn't get my coffee in time");

            Console.WriteLine("Customer: got my coffee, let's go!");
        }

        public void Hadler(MessageContext ctx, PaymentDue message)
        {
            ctx.Reply(new SubmitPayment
            {
                Amount = message.Amount,
                CorrelationId = message.StarbucksTransactionId
            });
        }

        public void Hadler(MessageContext ctx, DrinkReady message)
        {
            if (_wait != null)
                _wait.Set();
        }
    }
}