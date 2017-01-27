using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using NLog;

namespace main
{
    public interface IConfiguraThisEndpoint
    {
        UnicastBus Configure(CancellationToken token);
    }

    public class UnicastBusWorkerRoleEntrypoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        List<UnicastBus> _endpoints = new List<UnicastBus>();

        public UnicastBusWorkerRoleEntrypoint()
        {
            AppDomain.CurrentDomain.UnhandledException += (CurrentDomainUnhandledException);
        }

        public void Start()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            Logger.Info("Starting endpoints");

            var bin = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll")
                .Select(Assembly.LoadFile);

            var types = bin
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterfaces().Contains(typeof(IConfiguraThisEndpoint)));

            foreach (var type in types)
            {
                Logger.Info("Discovered endpoint " + type);

                var ep = (IConfiguraThisEndpoint)Activator.CreateInstance(type);
                var unicastBus = ep.Configure(this._cancellationTokenSource.Token);
                unicastBus.Start();

                _endpoints.Add(unicastBus);
            }

            this._runCompleteEvent.Set();
        }

        public void Stop()
        {
            Logger.Info("Stopping endpoints");

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            _endpoints.ForEach(x=>x.Dispose());
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Info("Unhandled exception occured: " + e.ExceptionObject);
        }
    }
}