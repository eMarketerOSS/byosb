using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

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

        List<UnicastBus> _endpoints = new List<UnicastBus>();

        public UnicastBusWorkerRoleEntrypoint()
        {
            AppDomain.CurrentDomain.UnhandledException += (CurrentDomainUnhandledException);
        }

        public IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public void Start()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            Trace.TraceInformation("BackedHa has been started");

            var bin = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll")
                .Select(Assembly.LoadFile)
                .ToList();

            var types = bin
                .SelectMany(GetLoadableTypes)
                .Where(p => p.GetInterfaces().Contains(typeof(IConfiguraThisEndpoint)));

            foreach (var type in types)
            {
                Trace.TraceInformation("Discovered endpoint " + type);

                var ep = (IConfiguraThisEndpoint)Activator.CreateInstance(type);
                var unicastBus = ep.Configure(this._cancellationTokenSource.Token);
                unicastBus.Start();

                _endpoints.Add(unicastBus);
            }

            this._runCompleteEvent.Set();

        }

        public void Stop()
        {
            Trace.TraceInformation("BackedHa is stopping");

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            _endpoints.ForEach(x=>x.Dispose());

            Trace.TraceInformation("BackedHa has stopped");
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine("Unhandled exception occured: " + e.ExceptionObject);
        }
    }
}