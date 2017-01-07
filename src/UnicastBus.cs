using System;
using System.Collections.Concurrent;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace main
{
    public class MessageContext
    {
        public object Out { get; set; }

        public void Reply(object msg)
        {
            Out = msg;
        }
    }

    public class UnicastBus : IDisposable
    {
        private readonly string _endpointName;
        private readonly string _conn;
        private QueueClient _mainQueue;
        private readonly OnMessageOptions _options = new OnMessageOptions{ AutoComplete = false, AutoRenewTimeout = TimeSpan.FromMinutes(1)};

        private readonly ConcurrentDictionary<Type, Action<MessageContext, object>> _messageHandlers = new ConcurrentDictionary<Type, Action<MessageContext, object>>();
        private readonly ConcurrentDictionary<string, string> _destinations = new ConcurrentDictionary<string, string>();

        public UnicastBus(string endpointName, string conn, Action<ConcurrentDictionary<string, string>> configureRouter)
            : this(endpointName, conn)
        {
            configureRouter(_destinations);
        }

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

        public void Send(object msg)
        {
            // Routing rule precedence, first match is a hit
            //      type
            //      namespace
            //      assembly

            var type = msg.GetType();
            var route = string.Empty;

            if (_destinations.ContainsKey(type.Name))
            {
                route = _destinations[type.Name];
                //Debug(">> Route by Name: " + type.Name);
            }

            if (string.IsNullOrEmpty(route) && _destinations.ContainsKey(type.Namespace))
            {
                route = _destinations[type.Namespace];
                //Debug(">> Route by Namespace: " + type.Namespace);
            }

            if (string.IsNullOrEmpty(route) && _destinations.ContainsKey(type.Assembly.GetName().Name))
            {
                route = _destinations[type.Assembly.GetName().Name];
                //Debug(">> Route by Assembly: " + type.Assembly.GetName().Name);
            }

            if (string.IsNullOrEmpty(route))
                throw new InvalidOperationException("Please set the destination in the config or use explicit send() overload.");

            Send(route, msg);
        }

        public void Send(string destination, object msg)
        {
            var packed = new BrokeredMessage(JsonConvert.SerializeObject(msg));
            packed.Properties["type"] = msg.GetType().FullName;
            packed.Properties["from"] = _endpointName + "@" + _conn;

            string endpoint;
            string conn;

            if (destination.IndexOf('@') != -1)
            {
                var parts = destination.Split('@');
                
                endpoint = parts[0];
                conn = parts[1];
            }
            else
            {
                endpoint = destination;
                conn = _conn;
            }

            var sender = QueueClient.CreateFromConnectionString(conn, endpoint);
            sender.Send(packed);
        }


        /* Publish Subscribe */

        public void Publish(object evt)
        {
            var packed = new BrokeredMessage(JsonConvert.SerializeObject(evt));
            packed.Properties["type"] = evt.GetType().FullName;
            packed.Properties["from"] = _endpointName + "@" + _conn;

            var epTopic = _endpointName + ".events";

            var publisher = TopicClient.CreateFromConnectionString(_conn, epTopic);
            publisher.Send(packed);
        }

        public void Subscribe<T>(string endpointConn)
        {
            string endpoint;
            string conn;

            if (endpointConn.IndexOf('@') != -1)
            {
                var parts = endpointConn.Split('@');

                endpoint = parts[0];
                conn = parts[1];
            }
            else
            {
                endpoint = endpointConn;
                conn = _conn;
            }

            var epTopic = endpoint + ".events";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(conn);
            var subscriptionName = _endpointName + "." + typeof (T).Name;

            if (!namespaceManager.SubscriptionExists(epTopic, subscriptionName))
            {
                var evtName = typeof(T).FullName;
                var sqlFilter = new SqlFilter(string.Format("type = '{0}'", evtName));
                namespaceManager.CreateSubscription(epTopic, subscriptionName, sqlFilter);
            }

            var subscriber = SubscriptionClient.CreateFromConnectionString(_conn, epTopic, subscriptionName);
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

        public void RegisterHandler<TCommand>(Action<MessageContext,TCommand> handler) where TCommand : class
        {
            var name = typeof(TCommand).Name;
            if (name == null) throw new InvalidOperationException("Should not happen");

            _messageHandlers[typeof(TCommand)] = ((context, o) => handler(context, (TCommand)o));
        }

        private void TryDispatchMessage(BrokeredMessage msg)
        {
            var msgPayload = msg.GetBody<String>();

            //Info("Message body: " + msgPayload);
            Info("Dispatch Message id: " +msg.MessageId);

            var msgType = Type.GetType(msg.Properties["type"].ToString());
            if (msgType == null)
            {
                Error("Message does not have a messagebodytype specified, message " + msg.MessageId);
                msg.DeadLetter();
            }

            if (_messageHandlers.ContainsKey(msgType))
            {
                var handler = _messageHandlers[msgType] as Action<MessageContext, object>;

                if (handler != null)
                {
                    var typedMsg = JsonConvert.DeserializeObject(msgPayload, msgType);
                    var ctx = new MessageContext();

                    handler(ctx, typedMsg);

                    if (ctx.Out != null)
                    {
                        var from = msg.Properties["from"].ToString();
                        Send(from, ctx.Out);
                    }
                }
            }
            else
                Info("Skipping message " + msgType + " no handler registered.");
        }

        private void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [" + _endpointName + "] i " + text);
            Console.ResetColor();
        }

        private void Debug(string text)
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