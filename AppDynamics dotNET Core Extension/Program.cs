using System;
using System.Configuration;

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
        private static void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!monitor.started)
                monitor.Run();
        }
    }
}
