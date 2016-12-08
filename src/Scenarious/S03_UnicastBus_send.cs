using System;
using Microsoft.ServiceBus.Messaging;

namespace Shuttle.Scenarious
{
    class S03_UnicastBus_send : Scenario
    {
        public override void Run()
        {
            using (var sb1 = new UnicastBus("ep1", Conn))
            using (var sb2 = new UnicastBus("ep2", Conn))
            {
                sb1.Start();
                sb2.Start();

                sb1.Send("ep2", new RegisterUser("This is a test message!"));
            }
        }
    }

    public class RegisterUser
    {
        private readonly string _note;

        public RegisterUser(string note)
        {
            _note = note;
        }
    }
}