using System;
using main;
using sample.Starbucks.Starbucks.Messages.Cashier;

namespace sample.Starbucks
{
    internal class CashierService
    {
        private readonly UnicastBus _bus;

        public CashierService(UnicastBus bus)
        {
            _bus = bus;
        }

        public void Hadler(MessageContext ctx, NewOrder message)
        {
            Console.WriteLine("Cashier: got new order");
            var correlationId = Guid.NewGuid();

            _bus.Publish(new PrepareDrink
            {
                CorrelationId = correlationId,
                CustomerName = message.CustomerName,
                DrinkName = message.DrinkName,
                Size = message.Size
            });
            
            ctx.Reply(new PaymentDue
            {
                CustomerName = message.CustomerName,
                StarbucksTransactionId = correlationId,
                Amount = ((int)message.Size) * 1.25m
            });
        }

        public void Hadler(MessageContext ctx, SubmitPayment message)
        {
            Console.WriteLine("Cashier: got payment");
            _bus.Publish(new PaymentComplete
            {
                CorrelationId = message.CorrelationId
            });
        }
    }
}