using System;

namespace Shuttle.Scenarious
{
    public abstract class Scenario
    {
        protected string Id;
        protected string Conn;

        public void Initialize(string conn, string id)
        {
            Id = id;
            Conn = conn;
        }

        public abstract void Run();

        public static void RunAll(string connectionString)
        {
            var scenarios = new Scenario[]
            {
                new S01_Queue_send_receive(),
                new S02_Topic_publish_subscribe(),
                new S03_UnicastBus_send(), 
                new S04_UnicastBus_publish(), 
                new S05_UnicastBus_command_handler(), 
                new S06_UnicastBus_event_handler(), 
                new S07_UnicastBus_command_routing(), 
            };

            RunThemOneByOne(connectionString, scenarios);
        }

        public static void RunThis<T>(string connectionString) where T : Scenario 
        {
            var scenarios = new Scenario[]
            {
                Activator.CreateInstance(typeof(T), new object[] { }) as T
            };

            RunThemOneByOne(connectionString, scenarios);
        }

        private static void RunThemOneByOne(string connectionString, Scenario[] scenarios)
        {
            for (var i = 0; i < scenarios.Length; i++)
            {
                var scenario = scenarios[i];
                var scenarioName = scenario.GetType().Name;

                Console.WriteLine("{0}", scenarioName.Replace("_", " "));
                Console.WriteLine(new string('-', 40));

                scenario.Initialize(connectionString, scenarioName);
                scenario.Run();

                Console.WriteLine();
            }

            Console.WriteLine("You can check out the contents with Service Bus Explorer in VS");
        }
    }
}