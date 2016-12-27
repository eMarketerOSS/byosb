using System;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace usage.Scenarious
{
    class S01_Queue_send_receive : Scenario
    {
        public override void Run()
        {
            var queueName = "aaaaa";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(Conn);

            if (!namespaceManager.QueueExists(queueName))
                namespaceManager.CreateQueue(queueName);

            var sender = QueueClient.CreateFromConnectionString(Conn, queueName);
            sender.Send(new BrokeredMessage("This is a test message!"));


            var client = QueueClient.CreateFromConnectionString(Conn, queueName);

            client.OnMessage(message =>
            {
                Console.WriteLine(String.Format("Message body: {0}", message.GetBody<String>()));
                Console.WriteLine(String.Format("Message id: {0}", message.MessageId));
            });

            Thread.Sleep(100);
        }
    }
}