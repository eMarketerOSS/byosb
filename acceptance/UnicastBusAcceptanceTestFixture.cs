using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using main;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace acceptance
{
    public class UnicastBusAcceptanceTestFixture : IDisposable
    {
        readonly string _conn;

        public UnicastBusAcceptanceTestFixture()
        {
            _conn = Environment.GetEnvironmentVariable("shuttle-sb-connection");
        }

        public UnicastBus GetUnicastBus(string epName,Action<ConcurrentDictionary<string, string>> cfg)
        {
            var bus = new UnicastBus(epName, _conn, cfg);
            bus.Start();

            return bus;
        }

        public UnicastBus GetUnicastBus(string epName)
        {
            var bus = new UnicastBus(epName, _conn);
            bus.Start();

            return bus;
        }

        public virtual void Dispose()
        {
            var manager = NamespaceManager.CreateFromConnectionString(_conn);

            foreach (var q in manager.GetQueues())
            {
                Console.WriteLine("Dispose q " + q.Path);
                manager.DeleteQueue(q.Path);
            }

            foreach (var q in manager.GetTopics())
            {
                Console.WriteLine("Dispose t " + q.Path);
                manager.DeleteTopic(q.Path);
            }
        }

        public void InjectMessage(string ep, object msg)
        {
            var packed = new BrokeredMessage(JsonConvert.SerializeObject(msg));
            packed.Properties["type"] = msg.GetType().FullName;
            packed.Properties["from"] = "simulator" + "@" + _conn;

            var sender = QueueClient.CreateFromConnectionString(_conn, ep);
            sender.Send(packed);

            Thread.Sleep(100);
        }

        public void InjectRawMessage(string ep)
        {
            var packed = new BrokeredMessage();
            var sender = QueueClient.CreateFromConnectionString(_conn, ep);
            sender.Send(packed);

            Thread.Sleep(100);
        }
    }
}