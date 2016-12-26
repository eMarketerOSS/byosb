using System;

namespace sample.Starbucks
{
    public enum DrinkSize
    {
        Tall,
        Grande,
        Venti
    }

    namespace Starbucks.Messages.Barista
    {
        public class DrinkReady
        {
            public Guid CorrelationId { get; set; }
            public string Drink { get; set; }
        }
    }

    namespace Starbucks.Messages.Cashier
    {
        public class NewOrder
        {
            public string DrinkName { get; set; }
            public DrinkSize Size { get; set; }
            public string CustomerName { get; set; }
        }

        public class PaymentComplete
        {
            public Guid CorrelationId { get; set; }
        }

        public class PaymentDue
        {
            public string CustomerName { get; set; }
            public Guid StarbucksTransactionId { get; set; }
            public decimal Amount { get; set; }
        }

        public class PrepareDrink
        {
            public string DrinkName { get; set; }
            public DrinkSize Size { get; set; }
            public string CustomerName { get; set; }
            public Guid CorrelationId { get; set; }
        }

        public class SubmitPayment
        {
            public Guid CorrelationId { get; set; }
            public decimal Amount { get; set; }
        }
    }
}