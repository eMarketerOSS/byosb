using System;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace acceptance
{
    [TestFixture]
    public partial class UnicastBusAcceptanceTests
    {
        private readonly string _conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");

        public long GetQueueMessageCount(string queueName)
        {
            var nsmgr = Microsoft.ServiceBus.NamespaceManager.CreateFromConnectionString(_conn);
            return nsmgr.GetQueue(queueName).MessageCount;
        }

        public long GetQueueDeadLetterMessageCount(string queueName)
        {
            var namespaceManager = Microsoft.ServiceBus.NamespaceManager.CreateFromConnectionString(_conn);
            var messageDetails = namespaceManager.GetQueue(queueName).MessageCountDetails;
            return messageDetails.DeadLetterMessageCount;
        }
    }
}
