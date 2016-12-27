using System;
using System.Threading;
using main;
using usage.Scenarious.CreditCards;
using usage.Scenarious.FraudPrevention;
using usage.Scenarious.TreasuryAndCashManagement;

namespace usage.Scenarious
{
    namespace TreasuryAndCashManagement
    {
        public class CallMeAHeavyDutyTruck { }
    }

    namespace CreditCards
    {
        public class OrderMorePlastic { }
    }

    namespace FraudPrevention
    {
        public class CaptureNonStop { }
    }

    class S07_UnicastBus_command_routing : Scenario
    {
        public override void Run()
        {
            var hq = new UnicastBus("bank.Hq", Conn, router =>
            {
                // routed by assembly
                router["Shuttle"] = "bank.SecurityCameras@" + Conn;

                // routed by namespace
                router["Shuttle.Scenarious.TreasuryAndCashManagement"] = "bank.TreasuryAndCashManagement@" + Conn;
                
                // routed by type
                router["OrderMorePlastic"] = "bank.CreditCards@" + Conn;
            });

            using (hq)
            using (var fp = new UnicastBus("bank.SecurityCameras", Conn))
            using (var tcm = new UnicastBus("bank.TreasuryAndCashManagement", Conn))
            using (var cc = new UnicastBus("bank.CreditCards", Conn))
            {
                hq.Start();

                tcm.RegisterHandler<CallMeAHeavyDutyTruck>((ctx, changed) => Console.WriteLine(" getting the best truck for you"));
                cc.RegisterHandler<OrderMorePlastic>((ctx, changed) => Console.WriteLine(" clients demand more business !!!"));
                fp.RegisterHandler<CaptureNonStop>((ctx, changed) => Console.WriteLine(" everybody is on watch, no exceptions "));

                tcm.Start();
                cc.Start();
                fp.Start();

                hq.Send(new CallMeAHeavyDutyTruck());
                hq.Send(new OrderMorePlastic());
                hq.Send(new CaptureNonStop());

                Thread.Sleep(100);
            }
        }
    }
}