using NUnit.Framework;

namespace acceptance
{
    [TestFixture]
    public partial class UnicastBusAcceptanceTests
    {
        [Test]
        public void SmokeTest()
        {
            using (var fx = new UnicastBusAcceptanceTestFixture())
            {
                
            }
        }
    }
}
