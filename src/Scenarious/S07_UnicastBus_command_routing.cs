﻿using System;
using System.Threading;
using Shuttle.Scenarious.CreditCards;
using Shuttle.Scenarious.FraudPrevention;
using Shuttle.Scenarious.TreasuryAndCashManagement;

namespace Shuttle.Scenarious
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

                tcm.RegisterHandler<CallMeAHeavyDutyTruck>(changed => Console.WriteLine(" getting the best truck for you"));
                cc.RegisterHandler<OrderMorePlastic>(changed => Console.WriteLine(" clients demand more business !!!"));
                fp.RegisterHandler<CaptureNonStop>(changed => Console.WriteLine(" everybody is on watch, no exceptions "));

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