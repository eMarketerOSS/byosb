using System;
using NUnit.Framework;

namespace acceptance
{
    [TestFixture]
    public partial class UnicastBusAcceptanceTests
    {
        [Test]
        public void Given_a_message_is_received_Then_it_gets_dispatched_to_handler()
        {
            using (var fx = new UnicastBusAcceptanceTestFixture())
            using (var bus = fx.GetUnicastBus("commandhandler"))
            {
                Cmd handled = null;
                bus.RegisterHandler<Cmd>((ctx, h) => { handled = h; });

                fx.InjectMessage("commandhandler", new Cmd());

                Assert.IsNotNull(handled);
                Assert.AreEqual(0, GetQueueMessageCount("commandhandler"));
            }
        }

        [Test]
        public void Given_a_message_is_received_Then_no_handler_will_pass_through()
        {
            using (var fx = new UnicastBusAcceptanceTestFixture())
            using (var bus = fx.GetUnicastBus("nohandler"))
            {
                fx.InjectMessage("nohandler", new Cmd());

                Assert.AreEqual(0, GetQueueMessageCount("nohandler"));
            }
        }

        [Test]
        public void Given_handler_throws_1st_time_Then_it_is_considered_transient()
        {
            using (var fx = new UnicastBusAcceptanceTestFixture())
            using (var bus = fx.GetUnicastBus("commandhandlerfirstattemptthrows"))
            {
                bus.RegisterHandler<Cmd>((ctx, h) => { throw new Exception(); });

                fx.InjectMessage("commandhandlerfirstattemptthrows", new Cmd());

                Assert.AreEqual(1, GetQueueMessageCount("commandhandlerfirstattemptthrows"));
            }
        }

        [Test]
        public void Given_message_has_missing_type_Then_move_to_dead_letter()
        {
            using (var fx = new UnicastBusAcceptanceTestFixture())
            using (var bus = fx.GetUnicastBus("commandhandlermissingtypeinmessage"))
            {
                bus.RegisterHandler<Cmd>((ctx, h) => { throw new Exception(); });

                fx.InjectRawMessage("commandhandlermissingtypeinmessage");

                Assert.AreEqual(1, GetQueueDeadLetterMessageCount("commandhandlermissingtypeinmessage"));
            }
        }
        

        public class Cmd {}
    }
}