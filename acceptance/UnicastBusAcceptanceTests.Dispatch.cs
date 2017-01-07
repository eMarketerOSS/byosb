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
            }
        }

        [Test]
        public void Given_handler_throws_1st_time_Then_it_is_considered_transient()
        {
        }

        [Test]
        public void Given_handler_throws_and_exceeds_3_times_time_Then_it_is_send_to_error_queue()
        {
        }

        public class Cmd {}
    }
}