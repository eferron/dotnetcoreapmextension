using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace AppDynamics.dotnetCore.Extension
{
    public partial class CoreMetricService : ServiceBase
    {


        string logPath = ConfigurationManager.AppSettings["DebugLogPath"];
        public EventMonitor monitor = null;
       

        internal void DebugService(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            // Console.ReadLine();
            // this.OnStop();
        }
        
        public CoreMetricService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Timer timer = new Timer();
            timer.Interval = 15000;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Enabled = true;
            
            
            
            monitor = new EventMonitor();
            monitor.Init();
            timer.Start();

        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            if (!monitor.started)
                monitor.Run();
        }

        protected override void OnStop()
        {
        }
    }

    class StatusChecker
    {

    }
}
