using main;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Starbucks.Host
{
    public class WorkerRole : RoleEntryPoint
    {
        readonly UnicastBusWorkerRoleEntrypoint _busRole = new UnicastBusWorkerRoleEntrypoint();

        public override bool OnStart()
        {
            _busRole.Start();
            return base.OnStart();
        }

        public override void OnStop()
        {
            _busRole.Stop();
            base.OnStop();
        }
    }
}
