using System;
using System.Collections.Generic;
using System.Threading;
using main;
using NLog;
using sample.Messages.Barista;
using sample.Messages.Cashier;

namespace sample
{
    public class BaristaService
    {
        private readonly UnicastBus _bus;
        private readonly Dictionary<Guid,BaristaState> _states = new Dictionary<Guid, BaristaState>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BaristaService(UnicastBus bus)
        {
            _bus = bus;
        }

        public void Hadler(MessageContext ctx, PrepareDrink message)
        {
            BaristaState state;

            if (_states.ContainsKey(message.CorrelationId) == false)
            {
                state = new BaristaState()
                {
                    CorrelationId = message.CorrelationId,
                    Drink = message.DrinkName
                };

                _states[message.CorrelationId] = state;
            }
            else
            {
                state = _states[message.CorrelationId];
                state.Drink = message.DrinkName;
            }

            for (int i = 0; i < 10; i++)
            {
                Logger.Info("Barista: preparing drink: " + state.Drink);
                Thread.Sleep(500);
            }
            state.DrinkIsReady = true;
            SubmitOrderIfDone(state);
        }

        public void Hadler(MessageContext ctx, PaymentComplete message)
        {
            BaristaState state;

            if (_states.ContainsKey(message.CorrelationId) == false)
            {
                state = new BaristaState()
                {
                    CorrelationId = message.CorrelationId,
                };

                _states[message.CorrelationId] = state;
            }
            else
                state = _states[message.CorrelationId];

            Logger.Info("Barista: got payment notification");
            state.GotPayment = true;
            SubmitOrderIfDone(state);
        }

        private void SubmitOrderIfDone(BaristaState state)
        {
            if (state.GotPayment && state.DrinkIsReady)
            {
                Logger.Info("Barista: drink is ready");
                _bus.Publish(new DrinkReady
                {
                    CorrelationId = state.CorrelationId,
                    Drink = state.Drink
                });
            }
        }

        public class BaristaState
        {
            public bool DrinkIsReady { get; set; }
            public bool GotPayment { get; set; }
            public string Drink { get; set; }
            public Guid CorrelationId { get; set; }
        }
    }
}