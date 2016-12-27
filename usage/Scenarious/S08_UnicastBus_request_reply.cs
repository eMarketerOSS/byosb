using System;
using System.Threading;
using main;

namespace usage.Scenarious
{
    public class S08_UnicastBus_request_reply : Scenario
    {
        public class RequestTimeOff{}
        public class TimeOffApproved {}

        public override void Run()
        {
            using (var sp =  new UnicastBus("bank.Supervisor", Conn))
            using (var wk1 = new UnicastBus("bank.Worker1", Conn))
            using (var wk2 = new UnicastBus("bank.Worker2", Conn))
            {
                sp.Start();
                wk1.Start();
                wk2.Start();

                sp.RegisterHandler<RequestTimeOff>((ctx, changed) =>
                {
                    Console.WriteLine(" thinking ... dm ...dm .. ");
                    
                    ctx.Reply(new TimeOffApproved());
                });

                wk1.RegisterHandler<TimeOffApproved>((ctx, changed) =>
                {
                    Console.WriteLine(" where is my luggage, baby ? ");
                });

                wk1.Send("bank.Supervisor", new RequestTimeOff());

                Thread.Sleep(100);
            }
        }
    }
}