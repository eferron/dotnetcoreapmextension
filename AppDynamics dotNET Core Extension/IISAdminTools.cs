using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using Microsoft.Web.Administration;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace AppDynamics.dotnetCore.Extension
{

    public static class IISAdminTools
    {
        public static void Log(string msg)
        {
            string logPath = ConfigurationManager.AppSettings["DebugLogPath"];
            StreamWriter logWriter;
            using (logWriter = new StreamWriter(logPath, true))
            {
                logWriter.WriteLine(msg);
            }
        }

        public static string GetAppPoolName(int pid)
        {

            try
            {
                ServerManager manager = new ServerManager();
                Site defaultSite = manager.Sites["Default Web Site"];

                foreach (Application app in defaultSite.Applications)
                {
                    Log($"{app.Path} is assigned to the '{app.ApplicationPoolName}' application pool.");
                }
            }
            catch (Exception)
            {

                throw;
            }

            return string.Empty;
        }
        public static List<string> GetApplicationPools()
        {
            List<string> result = new List<string>();
            try
            {
                using (var serverManager = new ServerManager())
                {
                    result = serverManager.ApplicationPools.Select(x => x.Name).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public static List<int> GetAppPoolProcesses(string appPoolName)
        {
            List<int> result = new List<int>();

            using (var serverManager = new ServerManager())
            {
                var appPool = serverManager.ApplicationPools[appPoolName];
                if (appPool.Name == appPoolName)
                {
                    foreach (var workerProcess in appPool.WorkerProcesses)
                    {
                        result.Add(workerProcess.ProcessId);
                    }
                }
            }

            return result;
        }
    }
}