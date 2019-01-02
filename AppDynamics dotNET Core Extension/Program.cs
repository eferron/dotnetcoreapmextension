using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ApplicationExtensions;

namespace AppDynamics.dotnetCore.Extension
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private static EventMonitor monitor = null;
        static void Main(string[] args)
        {


            if (Environment.UserInteractive)
            {
                CoreMetricService apmService = new CoreMetricService();
                apmService.DebugService(args);
                Console.ReadLine();
            }
            else
            {
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = Double.Parse(ConfigurationManager.AppSettings["Interval"]);
                timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
                timer.Enabled = true;

                monitor = new EventMonitor();
                monitor.Init();
                timer.Start();

                Console.ReadLine();

                //ServiceBase[] ServicesToRun;
                //ServicesToRun = new ServiceBase[]
                //{
                //    new CoreMetricService()
                //};
                //ServiceBase.Run(ServicesToRun);
            }

        }

        public static void Log(string msg)
        {
            string logPath = ConfigurationManager.AppSettings["DebugLogPath"];
            string debugLevel = ConfigurationManager.AppSettings["DebugLevel"].ToUpperInvariant();
            if (debugLevel.ToUpper() == "DEBUG")
            {
                using (StreamWriter logWriter = new StreamWriter(logPath, true))
                {
                    logWriter.WriteLine($"{DateTime.Now.ToString("mm/dd/yyy HH:mm")}\t{msg}");
                }
            }
        }

        private static void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!monitor.started)
            {
                monitor.Run();
            }

            var metricTask = Task.Run(() =>
            {
                List<string> monitoredApps = ConfigTools.GetConfiguredApps();
                foreach (string app in monitoredApps)
                {
                    
                    Process[] processList = Process.GetProcessesByName(app);
                    if (!processList.IsNullOrEmpty())
                    {
                        foreach (Process processItem in processList)
                        {
                            Console.WriteLine($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Memory|{processItem.ProcessName}|Current Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.WorkingSet64 / 1000}");
                            Console.WriteLine($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Memory|{processItem.ProcessName}|Current Committed,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.PeakWorkingSet64 / 1000}");
                            Console.WriteLine($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Process|{processItem.ProcessName}|Thread Count,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.Threads.Count}");
                            Console.WriteLine($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Process|{processItem.ProcessName}|Handles,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.HandleCount}");

                            Log($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Memory|{processItem.ProcessName}|Current Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.WorkingSet64 / 1000}");
                            Log($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Memory|{processItem.ProcessName}|Current Committed,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.PeakWorkingSet64 / 1000}");
                            Log($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Process|{processItem.ProcessName}|Thread Count,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.Threads.Count}");
                            Log($"name=Custom Metrics|Nodes|{System.Environment.MachineName}|Process|{processItem.ProcessName}|Handles,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={processItem.HandleCount}");
                        }
                    }

                }
            });
            metricTask.Wait();

        }
    }
}
namespace ApplicationExtensions
{
    public static class ArrayExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
    }
}
