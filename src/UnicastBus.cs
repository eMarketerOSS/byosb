using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq.Expressions;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Shuttle
{
    public class UnicastBus : IDisposable
    {
        private readonly string _endpointName;
        private readonly string _conn;
        private QueueClient _mainQueue;
        private readonly OnMessageOptions _options = new OnMessageOptions{ AutoComplete = false, AutoRenewTimeout = TimeSpan.FromMinutes(1)};

        private readonly ConcurrentDictionary<Type, object> _messageHandlers = new ConcurrentDictionary<Type, object>();

        public UnicastBus(string endpointName, string conn)
        {
            _endpointName = endpointName;
            _conn = conn;

            CreateEndpointQueuesIfNotExists(_endpointName);
            CreateEndpointQueuesIfNotExists(_endpointName + ".error");

            CreateTopicIfNotExists(_endpointName + ".events");

            Info("Initialized");
        }

        private void CreateEndpointQueuesIfNotExists(string name)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_conn);

            if (!namespaceManager.QueueExists(name))
                namespaceManager.CreateQueue(name);
        }

        private void CreateTopicIfNotExists(string name)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_conn);

            if (!namespaceManager.TopicExists(name))
                namespaceManager.CreateTopic(name);
        }

        public void Start()
        {
            _mainQueue = QueueClient.CreateFromConnectionString(_conn, _endpointName);

            _mainQueue.OnMessage(message =>
            {
                try
                {
                    TryDispatchMessage(message);

                    message.Complete();
                }
                catch (Exception e)
                {
                    Error(e.ToString());
                    message.Abandon();
                }

            }, _options);

            Info("Started");
        }

        /* Request Reply */

        public void Send(string destination, object msg)
        {
            var packed = new BrokeredMessage(JsonConvert.SerializeObject(msg));
            packed.Properties["type"] = msg.GetType().FullName;

            var sender = QueueClient.CreateFromConnectionString(_conn, destination);
            sender.Send(packed);
        }


        /* Publish Subscribe */

        public void Publish(object evt)
        {
            var packed = new BrokeredMessage(JsonConvert.SerializeObject(evt));
            packed.Properties["type"] = evt.GetType().FullName;

            var epTopic = _endpointName + ".events";

            var publisher = TopicClient.CreateFromConnectionString(_conn, epTopic);
            publisher.Send(packed);
        }

        public void Subscribe(string endpoint, string all)
        {
            var epTopic = endpoint + ".events";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_conn);

            if (!namespaceManager.SubscriptionExists(epTopic, "all"))
                namespaceManager.CreateSubscription(epTopic, "all");

            var subscriber = SubscriptionClient.CreateFromConnectionString(_conn, epTopic, "all");


            subscriber.OnMessage((message) =>
            {
                try
                {
                    TryDispatchMessage(message);

                    message.Complete();
                }
                catch (Exception e)
                {
                    Error(e.ToString());
                    message.Abandon();
                }

            }, _options);
        }

        /* Command and event handling */

        public void RegisterHandler<TCommand>(Action<TCommand> handler) where TCommand : class
        {
            var name = typeof(TCommand).Name;
            if (name == null) throw new InvalidOperationException("Should not happen");

            _messageHandlers[typeof(TCommand)] = CastArgument<object, TCommand>(x => handler(x));
        }

        private static Action<TBase> CastArgument<TBase, TDerived>(Expression<Action<TDerived>> source) where TDerived : TBase
        {
            if (typeof(TDerived) == typeof(TBase))
                return (Action<TBase>)((Delegate)source.Compile());
            var sourceParameter = Expression.Parameter(typeof(TBase), "source");
            var result = Expression.Lambda<Action<TBase>>(Expression.Invoke(source, Expression.Convert(sourceParameter, typeof(TDerived))), sourceParameter);
            return result.Compile();
        }

        private void TryDispatchMessage(BrokeredMessage msg)
        {
            var msgPayload = msg.GetBody<String>();

            Info("Message body: " + msgPayload);
            Info("Message id: " +msg.MessageId);

            var msgType = Type.GetType(msg.Properties["type"].ToString());
            if (msgType == null)
            {
                Error("Message does not have a messagebodytype specified, message " + msg.MessageId);
                msg.DeadLetter();
            }

            if (_messageHandlers.ContainsKey(msgType))
            {
                var handler = _messageHandlers[msgType] as Action<object>;

                if (handler != null)
                {
                    var typedMsg = JsonConvert.DeserializeObject(msgPayload, msgType);
                    handler(typedMsg);
                }
            }
            else
                Info("Skipping message " + msgType + " no handler registered.");
        }

        private void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [" + _endpointName + "] i " + text);
            Console.ResetColor();
        }

        private void Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [" + _endpointName + "] e " + text);
            Console.ResetColor();
        }

        public void Dispose()
        {
            _mainQueue.Close();

            Info("Disposed");
        }
    }
}