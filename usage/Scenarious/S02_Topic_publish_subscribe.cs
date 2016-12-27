using System;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace usage.Scenarious
{
    class S02_Topic_publish_subscribe : Scenario
    {
        public override void Run()
        {
            var topicName = "TestTopic";

            var td = new TopicDescription(topicName)
            {
                MaxSizeInMegabytes = 5120,
                DefaultMessageTimeToLive = new TimeSpan(0, 1, 0)
            };

            var namespaceManager = NamespaceManager.CreateFromConnectionString(Conn);

            if (!namespaceManager.TopicExists(topicName))
                namespaceManager.CreateTopic(td);

            if (!namespaceManager.SubscriptionExists(topicName, "AllMessages"))
                namespaceManager.CreateSubscription(topicName, "AllMessages");



            var publisher = TopicClient.CreateFromConnectionString(Conn, topicName);
            var evt = new BrokeredMessage("Test message 1000");
            publisher.Send(evt);




            var subscriber = SubscriptionClient.CreateFromConnectionString(Conn, topicName, "AllMessages");

            var options = new OnMessageOptions
            {
                AutoComplete = false,
                AutoRenewTimeout = TimeSpan.FromMinutes(1)
            };

            subscriber.OnMessage((message) =>
            {
                try
                {
                    Console.WriteLine("Body: " + message.GetBody<string>());
                    Console.WriteLine("MessageID: " + message.MessageId);
                    message.Complete();
                }
                catch (Exception)
                {
                    message.Abandon();
                }
            }, options);


            // https://azure.microsoft.com/en-us/documentation/articles/service-bus-dotnet-how-to-use-topics-subscriptions/

            Thread.Sleep(500);
        }
    }
}